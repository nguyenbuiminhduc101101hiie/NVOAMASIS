using System.Globalization;
using System.Xml.Linq;
using NVOAMASIS.Models;

namespace NVOAMASIS.Services
{
    public static class BillSeaLayoutMrtHelper
    {
        public static List<BillSeaDataSource> LoadDataSources(XDocument doc)
        {
            var result = new List<BillSeaDataSource>();
            var dataSourcesNode = doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "DataSources");
            if (dataSourcesNode is null)
                return result;

            foreach (var sourceNode in dataSourcesNode.Elements())
            {
                var name = (sourceNode.Element("Name")?.Value ?? sourceNode.Name.LocalName)?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                var alias = sourceNode.Element("Alias")?.Value?.Trim();
                var columnsContainer = sourceNode.Element("Columns");
                if (columnsContainer is null)
                    continue;

                var columns = new List<BillSeaDataColumn>();
                var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var col in columnsContainer.Elements("value"))
                {
                    var raw = col.Value?.Trim();
                    if (string.IsNullOrWhiteSpace(raw))
                        continue;

                    var parts = raw.Split(',', StringSplitOptions.TrimEntries);
                    string columnName;
                    string dataType;

                    if (parts.Length >= 5 && parts[0].Equals("ORIGINAL", StringComparison.OrdinalIgnoreCase))
                    {
                        columnName = parts[1];
                        dataType = parts[4];
                    }
                    else if (parts.Length >= 2)
                    {
                        columnName = parts[0];
                        dataType = parts[1];
                    }
                    else
                    {
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(columnName) || !seen.Add(columnName))
                        continue;

                    columns.Add(new BillSeaDataColumn
                    {
                        Name = columnName,
                        DataType = dataType ?? string.Empty
                    });
                }

                if (columns.Count == 0)
                    continue;

                result.Add(new BillSeaDataSource
                {
                    Name = name,
                    Alias = string.IsNullOrWhiteSpace(alias) ? name : alias!,
                    Columns = columns.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase).ToList()
                });
            }

            return result.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase).ToList();
        }

        public static List<BillSeaPageLayout> LoadPages(XDocument doc)
        {
            var pages = new List<BillSeaPageLayout>();
            var pagesContainer = doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "Pages");
            if (pagesContainer is null)
                return pages;

            foreach (var pageNode in pagesContainer.Elements())
            {
                if (pageNode.Attribute("type")?.Value != "Page" && pageNode.Element("PageWidth") is null)
                    continue;

                var pageName = pageNode.Element("Name")?.Value?.Trim() ?? pageNode.Name.LocalName;
                var pageRef = pageNode.Attribute("Ref")?.Value ?? string.Empty;

                var pageWidthInches = 8.27;
                var pageHeightInches = 11.69;
                if (double.TryParse(pageNode.Element("PageWidth")?.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var pageWidth) && pageWidth > 0)
                    pageWidthInches = pageWidth;
                if (double.TryParse(pageNode.Element("PageHeight")?.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var pageHeight) && pageHeight > 0)
                    pageHeightInches = pageHeight;

                var componentsParent = FindDesignComponentsParent(pageNode);
                var pageComponents = pageNode.Element("Components");
                var elements = new List<BillSeaDesignElement>();
                var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var node in componentsParent.Descendants())
                    TryParseDesignNode(node, elements, seenNames);

                if (pageComponents is not null)
                {
                    foreach (var node in pageComponents.Elements())
                    {
                        if (node.Name.LocalName.Equals("PageHeaderBand1", StringComparison.OrdinalIgnoreCase))
                            continue;

                        TryParseDesignNode(node, elements, seenNames);
                    }
                }

                pages.Add(new BillSeaPageLayout
                {
                    PageName = pageName,
                    PageRef = pageRef,
                    ComponentsParentRef = componentsParent.Attribute("Ref")?.Value ?? pageRef,
                    PageWidthInches = pageWidthInches,
                    PageHeightInches = pageHeightInches,
                    Elements = elements.OrderBy(x => x.Name).ToList()
                });
            }

            return pages;
        }

        public static int SavePages(XDocument doc, IReadOnlyList<BillSeaPageLayout> pages)
        {
            var updated = 0;
            var refAllocator = new RefAllocator(doc);

            foreach (var pageLayout in pages)
            {
                var pageNode = FindPageNode(doc, pageLayout.PageName);
                if (pageNode is null)
                    continue;

                var headerParent = FindDesignComponentsParent(pageNode);
                var map = pageLayout.Elements.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
                var managedNames = new HashSet<string>(map.Keys, StringComparer.OrdinalIgnoreCase);

                foreach (var node in pageNode.Descendants().ToList())
                {
                    var name = node.Element("Name")?.Value?.Trim();
                    if (string.IsNullOrWhiteSpace(name) || !managedNames.Contains(name))
                        continue;

                    if (!map.TryGetValue(name, out var element))
                        continue;

                    if (IsHorizontalLineNode(node))
                    {
                        var rectElement = node.Element("ClientRectangle");
                        if (rectElement is null)
                            continue;

                        rectElement.Value = BuildClientRectangle(element);
                        ApplyLineSize(node, element.LineSize);
                        updated++;
                        continue;
                    }

                    if (IsVerticalLineNode(node))
                    {
                        if (IsInsideHeaderBand(node, headerParent))
                        {
                            var guid = node.Element("Guid")?.Value;
                            if (!string.IsNullOrWhiteSpace(guid))
                            {
                                element.LineGuid = guid;
                                RemoveVerticalLinePoints(pageNode, guid);
                            }

                            node.Remove();
                            continue;
                        }

                        var rectElement = node.Element("ClientRectangle");
                        if (rectElement is not null)
                            rectElement.Value = BuildClientRectangle(element);

                        ApplyLineSize(node, element.LineSize);
                        var lineGuid = EnsureLineGuid(element, node);
                        var guidElement = node.Element("Guid");
                        if (guidElement is null)
                            node.Add(new XElement("Guid", lineGuid));
                        else
                            guidElement.Value = lineGuid;

                        EnsureVerticalLinePoints(pageNode, headerParent, element, pageLayout.PageRef, pageLayout.ComponentsParentRef, refAllocator);
                        SyncVerticalLinePoints(pageNode, element);
                        updated++;
                        continue;
                    }

                    if (IsImageNode(node))
                    {
                        var rectElement = node.Element("ClientRectangle");
                        if (rectElement is not null)
                            rectElement.Value = BuildClientRectangle(element);
                        updated++;
                        continue;
                    }

                    if (IsTextNode(node))
                    {
                        var rectElement = node.Element("ClientRectangle");
                        if (rectElement is not null)
                            rectElement.Value = BuildClientRectangle(element);

                        if (element.Kind == BillSeaElementKind.StaticText)
                        {
                            var textElement = node.Element("Text");
                            if (textElement is not null)
                                textElement.Value = element.Text;
                        }

                        WriteFontToNode(node, element);
                        updated++;
                    }
                }

                var headerComponents = headerParent.Element("Components") ?? headerParent;
                var pageComponents = pageNode.Element("Components");
                var existingNames = pageNode
                    .Descendants()
                    .Select(x => x.Element("Name")?.Value?.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                foreach (var element in pageLayout.Elements.Where(x => !existingNames.Contains(x.Name)))
                {
                    if (element.Kind == BillSeaElementKind.VerticalLine)
                    {
                        AddVerticalLineToPage(pageNode, headerParent, element, pageLayout.PageRef, pageLayout.ComponentsParentRef, refAllocator);
                        updated++;
                        continue;
                    }

                    var newNode = CreateElementNode(element, pageLayout.PageRef, pageLayout.ComponentsParentRef, refAllocator);
                    if (newNode is null)
                        continue;

                    if (headerComponents.Name.LocalName == "Components")
                        headerComponents.Add(newNode);
                    else
                        headerParent.Add(newNode);

                    updated++;
                }

                foreach (var element in pageLayout.Elements.Where(x => x.Kind == BillSeaElementKind.VerticalLine))
                {
                    var lineNode = FindNodeByName(pageNode, element.Name);
                    if (lineNode is not null && !IsInsideHeaderBand(lineNode, headerParent))
                        continue;

                    if (lineNode is not null)
                    {
                        var guid = lineNode.Element("Guid")?.Value;
                        if (!string.IsNullOrWhiteSpace(guid))
                            element.LineGuid = guid;
                        lineNode.Remove();
                        if (!string.IsNullOrWhiteSpace(element.LineGuid))
                            RemoveVerticalLinePoints(pageNode, element.LineGuid);
                    }

                    AddVerticalLineToPage(pageNode, headerParent, element, pageLayout.PageRef, pageLayout.ComponentsParentRef, refAllocator);
                    updated++;
                }

                updated += RemoveDeletedElements(pageNode, pageLayout.Elements);
                UpdateComponentsCount(headerParent);
                if (pageComponents is not null)
                    UpdateComponentsCount(pageNode);
            }

            return updated;
        }

        private static int RemoveDeletedElements(XElement pageNode, IReadOnlyList<BillSeaDesignElement> elements)
        {
            var currentNames = elements
                .Select(x => x.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var removed = 0;
            foreach (var node in pageNode.Descendants().ToList())
            {
                var name = node.Element("Name")?.Value?.Trim();
                if (string.IsNullOrWhiteSpace(name) || currentNames.Contains(name))
                    continue;

                if (IsVerticalLineNode(node))
                {
                    var guid = node.Element("Guid")?.Value;
                    node.Remove();
                    if (!string.IsNullOrWhiteSpace(guid))
                        RemoveVerticalLinePoints(pageNode, guid);
                    removed++;
                    continue;
                }

                if (IsHorizontalLineNode(node))
                {
                    node.Remove();
                    removed++;
                    continue;
                }

                if (IsTextNode(node))
                {
                    node.Remove();
                    removed++;
                }
            }

            return removed;
        }

        private static XElement FindDesignComponentsParent(XElement pageNode)
        {
            var headerBand = pageNode
                .Descendants()
                .FirstOrDefault(x => x.Name.LocalName.Equals("PageHeaderBand1", StringComparison.OrdinalIgnoreCase));

            return headerBand ?? pageNode;
        }

        private static void UpdateComponentsCount(XElement componentsParent)
        {
            var componentsElement = componentsParent.Element("Components");
            if (componentsElement is null)
                return;

            var count = componentsElement.Elements().Count();
            componentsElement.SetAttributeValue("isList", "true");
            componentsElement.SetAttributeValue("count", count.ToString(CultureInfo.InvariantCulture));
        }

        private static XElement? CreateElementNode(BillSeaDesignElement element, string pageRef, string parentRef, RefAllocator refAllocator)
        {
            var nextRef = refAllocator.Next();
            var elementLocalName = element.Kind switch
            {
                BillSeaElementKind.HorizontalLine => "HorizontalLinePrimitive",
                BillSeaElementKind.VerticalLine => "VerticalLinePrimitive",
                BillSeaElementKind.StaticText => "Text",
                BillSeaElementKind.DataText => "Text",
                _ => null
            };

            if (elementLocalName is null)
                return null;

            var uniqueLocalName = $"{elementLocalName}_{SanitizeName(element.Name)}";
            var node = new XElement(uniqueLocalName);
            node.SetAttributeValue("Ref", nextRef.ToString(CultureInfo.InvariantCulture));
            node.SetAttributeValue("type", elementLocalName);
            node.SetAttributeValue("isKey", "true");
            node.Add(new XElement("ClientRectangle", BuildClientRectangle(element)));

            if (IsLineKind(element.Kind))
            {
                node.Add(new XElement("Color", "Black"));
                node.Add(new XElement("Conditions", new XAttribute("isList", "true"), new XAttribute("count", "0")));
                node.Add(CreateCapElement(refAllocator, "EndCap"));
                node.Add(new XElement("Expressions", new XAttribute("isList", "true"), new XAttribute("count", "0")));

                if (element.Kind == BillSeaElementKind.VerticalLine)
                {
                    var guid = EnsureLineGuid(element);
                    node.Add(new XElement("Guid", guid));
                }

                node.Add(new XElement("Name", element.Name));
                node.Add(new XElement("Page", new XAttribute("isRef", pageRef)));
                node.Add(new XElement("Parent", new XAttribute("isRef", parentRef)));
                node.Add(new XElement("Size", FormatLineSize(element.LineSize)));
                node.Add(CreateCapElement(refAllocator, "StartCap"));
                return node;
            }

            node.Add(new XElement("Brush", "Transparent"));
            if (element.Kind == BillSeaElementKind.StaticText)
                node.Add(new XElement("CanGrow", "True"));

            node.Add(new XElement("Font", BuildFontValue(element)));
            node.Add(new XElement("Margins", "0,0,0,0"));
            node.Add(new XElement("Name", element.Name));
            node.Add(new XElement("Page", new XAttribute("isRef", pageRef)));
            node.Add(new XElement("Parent", new XAttribute("isRef", parentRef)));
            node.Add(new XElement("Text", element.Text));
            node.Add(new XElement("TextBrush", "Black"));
            if (element.Kind == BillSeaElementKind.StaticText)
                node.Add(new XElement("TextOptions", ",,,,WordWrap=True,A=0"));

            node.Add(new XElement("Type", "Expression"));
            return node;
        }

        private static XElement CreateCapElement(RefAllocator refAllocator, string capName)
        {
            var capRef = refAllocator.Next();
            var cap = new XElement(capName);
            cap.SetAttributeValue("Ref", capRef.ToString(CultureInfo.InvariantCulture));
            cap.SetAttributeValue("type", "Cap");
            cap.SetAttributeValue("isKey", "true");
            cap.Add(new XElement("Color", "Black"));
            return cap;
        }

        private sealed class RefAllocator
        {
            private int _nextRef;

            public RefAllocator(XDocument doc)
            {
                _nextRef = doc
                    .Descendants()
                    .Select(x => int.TryParse(x.Attribute("Ref")?.Value, out var value) ? value : 0)
                    .DefaultIfEmpty(0)
                    .Max() + 1;
            }

            public int Next() => _nextRef++;
        }

        private static void ParseFontFromNode(XElement node, BillSeaDesignElement element) =>
            ParseFontValue(element, node.Element("Font")?.Value);

        private static void WriteFontToNode(XElement node, BillSeaDesignElement element)
        {
            var fontElement = node.Element("Font");
            var value = BuildFontValue(element);

            if (fontElement is null)
                node.Add(new XElement("Font", value));
            else
                fontElement.Value = value;
        }

        private static void ParseFontValue(BillSeaDesignElement target, string? fontValue)
        {
            if (string.IsNullOrWhiteSpace(fontValue))
                return;

            var parts = fontValue.Split(',', StringSplitOptions.TrimEntries);
            if (parts.Length > 0 && !string.IsNullOrWhiteSpace(parts[0]))
                target.FontFamily = parts[0];

            if (parts.Length > 1
                && double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var fontSize)
                && fontSize > 0)
            {
                target.FontSize = fontSize;
            }

            if (parts.Length > 2)
                target.FontBold = parts.Skip(2).Any(x => x.Contains("Bold", StringComparison.OrdinalIgnoreCase));
        }

        public static string BuildFontValue(BillSeaDesignElement element)
        {
            var size = Math.Clamp(element.FontSize, 4, 72)
                .ToString("0.##", CultureInfo.InvariantCulture);
            var family = string.IsNullOrWhiteSpace(element.FontFamily) ? "Times New Roman" : element.FontFamily.Trim();
            return element.FontBold ? $"{family},{size},Bold" : $"{family},{size}";
        }

        private static double ParseLineSize(XElement node)
        {
            if (double.TryParse(node.Element("Size")?.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var size) && size > 0)
                return size;

            return 1;
        }

        private static void ApplyLineSize(XElement node, double lineSize)
        {
            var sizeElement = node.Element("Size");
            var value = FormatLineSize(lineSize);

            if (sizeElement is null)
                node.Add(new XElement("Size", value));
            else
                sizeElement.Value = value;
        }

        private static string FormatLineSize(double lineSize) =>
            Math.Clamp(lineSize, 1, 50)
                .ToString("0.##", CultureInfo.InvariantCulture);

        private static bool IsLineKind(BillSeaElementKind kind) =>
            kind is BillSeaElementKind.HorizontalLine or BillSeaElementKind.VerticalLine;

        private static bool IsHorizontalLineNode(XElement node) =>
            string.Equals(GetComponentType(node), "HorizontalLinePrimitive", StringComparison.OrdinalIgnoreCase)
            || node.Name.LocalName.StartsWith("HorizontalLinePrimitive", StringComparison.OrdinalIgnoreCase);

        private static bool IsVerticalLineNode(XElement node) =>
            string.Equals(GetComponentType(node), "VerticalLinePrimitive", StringComparison.OrdinalIgnoreCase)
            || node.Name.LocalName.StartsWith("VerticalLinePrimitive", StringComparison.OrdinalIgnoreCase);

        private static bool IsImageNode(XElement node) =>
            string.Equals(GetComponentType(node), "Image", StringComparison.OrdinalIgnoreCase)
            || node.Name.LocalName.StartsWith("Image", StringComparison.OrdinalIgnoreCase);

        private static bool IsTextNode(XElement node)
        {
            if (IsImageNode(node))
                return false;

            if (node.Element("ClientRectangle") is null || node.Element("Text") is null)
                return false;

            if (string.Equals(GetComponentType(node), "Text", StringComparison.OrdinalIgnoreCase))
                return true;

            var localName = node.Name.LocalName;
            return localName.StartsWith("Text", StringComparison.OrdinalIgnoreCase)
                || localName.StartsWith("Label", StringComparison.OrdinalIgnoreCase);
        }

        private static string GetImageDisplayLabel(string name, XElement node)
        {
            if (string.Equals(name, "Image1", StringComparison.OrdinalIgnoreCase))
                return "Form Bill";

            if (string.Equals(name, "CompanyLogo", StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, "Text1", StringComparison.OrdinalIgnoreCase))
            {
                return "Logo";
            }

            var imageUrl = node.Element("ImageURL")?.Value?.Trim();
            return string.IsNullOrWhiteSpace(imageUrl) ? name : imageUrl;
        }

        private static string? GetComponentType(XElement node) => node.Attribute("type")?.Value;

        private static void TryParseDesignNode(XElement node, List<BillSeaDesignElement> elements, HashSet<string> seenNames)
        {
            var name = node.Element("Name")?.Value?.Trim();
            if (string.IsNullOrWhiteSpace(name) || !seenNames.Add(name))
                return;

            if (IsHorizontalLineNode(node))
            {
                var rect = ParseClientRectangle(node.Element("ClientRectangle")?.Value);
                if (rect is null)
                    return;

                elements.Add(new BillSeaDesignElement
                {
                    Name = name,
                    Kind = BillSeaElementKind.HorizontalLine,
                    Left = rect.Value.Left,
                    Top = rect.Value.Top,
                    Width = rect.Value.Width,
                    Height = rect.Value.Height,
                    LineSize = ParseLineSize(node)
                });
                return;
            }

            if (IsVerticalLineNode(node))
            {
                var rect = ParseClientRectangle(node.Element("ClientRectangle")?.Value);
                if (rect is null)
                    return;

                elements.Add(new BillSeaDesignElement
                {
                    Name = name,
                    Kind = BillSeaElementKind.VerticalLine,
                    Left = rect.Value.Left,
                    Top = rect.Value.Top,
                    Width = rect.Value.Width,
                    Height = rect.Value.Height,
                    LineSize = ParseLineSize(node),
                    LineGuid = node.Element("Guid")?.Value?.Trim() ?? string.Empty
                });
                return;
            }

            if (IsImageNode(node))
            {
                var rect = ParseClientRectangle(node.Element("ClientRectangle")?.Value);
                if (rect is null || rect.Value.Width <= 0 || rect.Value.Height <= 0)
                    return;

                elements.Add(new BillSeaDesignElement
                {
                    Name = name,
                    Kind = BillSeaElementKind.Image,
                    Left = rect.Value.Left,
                    Top = rect.Value.Top,
                    Width = rect.Value.Width,
                    Height = rect.Value.Height,
                    ImageUrl = node.Element("ImageURL")?.Value?.Trim() ?? string.Empty,
                    Text = GetImageDisplayLabel(name, node)
                });
                return;
            }

            if (!IsTextNode(node))
                return;

            var clientRectangle = node.Element("ClientRectangle")?.Value?.Trim();
            if (string.IsNullOrWhiteSpace(clientRectangle))
                return;

            var textRect = ParseClientRectangle(clientRectangle);
            if (textRect is null || textRect.Value.Width <= 0 || textRect.Value.Height <= 0)
                return;

            var textValue = node.Element("Text")?.Value ?? string.Empty;
            var textElement = new BillSeaDesignElement
            {
                Name = name,
                Kind = textValue.Contains('{') ? BillSeaElementKind.DataText : BillSeaElementKind.StaticText,
                Left = textRect.Value.Left,
                Top = textRect.Value.Top,
                Width = textRect.Value.Width,
                Height = textRect.Value.Height,
                Text = textValue
            };
            ParseFontFromNode(node, textElement);
            elements.Add(textElement);
        }

        private static bool IsInsideHeaderBand(XElement node, XElement headerBand) =>
            node.Ancestors().Any(x => x == headerBand);

        private static XElement? FindNodeByName(XElement pageNode, string name) =>
            pageNode.Descendants().FirstOrDefault(x =>
                string.Equals(x.Element("Name")?.Value?.Trim(), name, StringComparison.OrdinalIgnoreCase));

        private static string EnsureLineGuid(BillSeaDesignElement element, XElement? lineNode = null)
        {
            if (!string.IsNullOrWhiteSpace(element.LineGuid))
                return element.LineGuid;

            var guid = lineNode?.Element("Guid")?.Value?.Trim();
            if (string.IsNullOrWhiteSpace(guid))
                guid = Guid.NewGuid().ToString("N");

            element.LineGuid = guid;
            return guid;
        }

        private static void AddVerticalLineToPage(
            XElement pageNode,
            XElement headerParent,
            BillSeaDesignElement element,
            string pageRef,
            string headerParentRef,
            RefAllocator refAllocator)
        {
            var pageComponents = pageNode.Element("Components")
                ?? throw new InvalidOperationException("Page Components container is missing.");

            var headerComponents = headerParent.Element("Components") ?? headerParent;
            var guid = EnsureLineGuid(element);

            var lineNode = CreateElementNode(element, pageRef, pageRef, refAllocator);
            if (lineNode is null)
                return;

            pageComponents.Add(lineNode);

            var startPoint = CreatePointPrimitive(refAllocator, isStart: true, pageRef, headerParentRef, guid, element.Left, element.Top);
            var endPoint = CreatePointPrimitive(refAllocator, isStart: false, pageRef, headerParentRef, guid, element.Left, element.Top + element.Height);
            headerComponents.Add(startPoint);
            headerComponents.Add(endPoint);
        }

        private static XElement CreatePointPrimitive(
            RefAllocator refAllocator,
            bool isStart,
            string pageRef,
            string parentRef,
            string lineGuid,
            double x,
            double y)
        {
            var typeName = isStart
                ? "Stimulsoft.Report.Components.StiStartPointPrimitive"
                : "Stimulsoft.Report.Components.StiEndPointPrimitive";
            var prefix = isStart ? "StartPointPrimitive" : "EndPointPrimitive";
            var localName = $"{prefix}_{lineGuid[..8]}";
            var node = new XElement(localName);
            node.SetAttributeValue("Ref", refAllocator.Next().ToString(CultureInfo.InvariantCulture));
            node.SetAttributeValue("type", typeName);
            node.SetAttributeValue("isKey", "true");
            node.Add(new XElement("ClientRectangle", BuildPointRectangle(x, y)));
            node.Add(new XElement("Conditions", new XAttribute("isList", "true"), new XAttribute("count", "0")));
            node.Add(new XElement("Expressions", new XAttribute("isList", "true"), new XAttribute("count", "0")));
            node.Add(new XElement("Name", localName));
            node.Add(new XElement("Page", new XAttribute("isRef", pageRef)));
            node.Add(new XElement("Parent", new XAttribute("isRef", parentRef)));
            node.Add(new XElement("ReferenceToGuid", lineGuid));
            return node;
        }

        private static string BuildPointRectangle(double x, double y) =>
            string.Join(",",
                x.ToString("0.####", CultureInfo.InvariantCulture),
                y.ToString("0.####", CultureInfo.InvariantCulture),
                "0",
                "0");

        private static void SyncVerticalLinePoints(XElement pageNode, BillSeaDesignElement element)
        {
            var guid = EnsureLineGuid(element);
            foreach (var point in pageNode.Descendants().Where(x => x.Element("ReferenceToGuid")?.Value == guid))
            {
                var isStart = IsStartPointNode(point);
                var x = element.Left;
                var y = isStart ? element.Top : element.Top + element.Height;
                var rectElement = point.Element("ClientRectangle");
                if (rectElement is null)
                    point.Add(new XElement("ClientRectangle", BuildPointRectangle(x, y)));
                else
                    rectElement.Value = BuildPointRectangle(x, y);
            }
        }

        private static void EnsureVerticalLinePoints(
            XElement pageNode,
            XElement headerParent,
            BillSeaDesignElement element,
            string pageRef,
            string headerParentRef,
            RefAllocator refAllocator)
        {
            var guid = EnsureLineGuid(element);
            if (pageNode.Descendants().Any(x => string.Equals(x.Element("ReferenceToGuid")?.Value, guid, StringComparison.OrdinalIgnoreCase)))
                return;

            var headerComponents = headerParent.Element("Components") ?? headerParent;
            headerComponents.Add(CreatePointPrimitive(refAllocator, isStart: true, pageRef, headerParentRef, guid, element.Left, element.Top));
            headerComponents.Add(CreatePointPrimitive(refAllocator, isStart: false, pageRef, headerParentRef, guid, element.Left, element.Top + element.Height));
        }

        private static bool IsStartPointNode(XElement node) =>
            node.Name.LocalName.Contains("StartPoint", StringComparison.OrdinalIgnoreCase)
            || string.Equals(node.Attribute("type")?.Value, "Stimulsoft.Report.Components.StiStartPointPrimitive", StringComparison.OrdinalIgnoreCase);

        private static void RemoveVerticalLinePoints(XElement pageNode, string lineGuid)
        {
            foreach (var point in pageNode.Descendants()
                         .Where(x => string.Equals(x.Element("ReferenceToGuid")?.Value, lineGuid, StringComparison.OrdinalIgnoreCase))
                         .ToList())
            {
                point.Remove();
            }
        }

        private static XElement? FindPageNode(XDocument doc, string pageName)
        {
            var pagesContainer = doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "Pages");
            if (pagesContainer is null)
                return null;

            return pagesContainer.Elements().FirstOrDefault(pageNode =>
            {
                var name = pageNode.Element("Name")?.Value?.Trim() ?? pageNode.Name.LocalName;
                return string.Equals(name, pageName, StringComparison.OrdinalIgnoreCase);
            });
        }

        public static BillSeaRect? ParseClientRectangle(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var parts = value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 4)
                return null;

            if (!double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var left))
                return null;
            if (!double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var top))
                return null;
            if (!double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var width))
                return null;
            if (!double.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var height))
                return null;

            return new BillSeaRect(left, top, width, height);
        }

        public static string BuildClientRectangle(BillSeaDesignElement element) =>
            string.Join(",",
                element.Left.ToString("0.####", CultureInfo.InvariantCulture),
                element.Top.ToString("0.####", CultureInfo.InvariantCulture),
                element.Width.ToString("0.####", CultureInfo.InvariantCulture),
                element.Height.ToString("0.####", CultureInfo.InvariantCulture));

        public static byte[]? ExtractImageBytes(byte[] mrtBytes, string imageName = "Image1")
        {
            if (mrtBytes is not { Length: > 0 })
                return null;

            using var ms = new MemoryStream(mrtBytes);
            var doc = XDocument.Load(ms);
            return ExtractImageBytes(doc, imageName);
        }

        public static byte[]? ExtractImageBytes(XDocument doc, string imageName = "Image1")
        {
            var imageNode = doc.Descendants()
                .FirstOrDefault(x =>
                    string.Equals(x.Attribute("type")?.Value, "Image", StringComparison.OrdinalIgnoreCase)
                    && (
                        string.Equals(x.Name.LocalName, imageName, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(x.Element("Name")?.Value, imageName, StringComparison.OrdinalIgnoreCase)
                    ));

            var base64 = imageNode?.Element("ImageBytes")?.Value;
            if (string.IsNullOrWhiteSpace(base64))
                return null;

            try
            {
                base64 = new string(base64.Where(c => !char.IsWhiteSpace(c)).ToArray());
                return Convert.FromBase64String(base64);
            }
            catch
            {
                return null;
            }
        }

        private static string SanitizeName(string name) =>
            new string(name.Where(char.IsLetterOrDigit).ToArray());
    }

    public readonly record struct BillSeaRect(double Left, double Top, double Width, double Height);
}
