using System.Globalization;
using System.Xml.Linq;

namespace NVOAMASIS.Services
{
    public static class BillSeaAttachMrtHelper
    {
        public const string AttachSkeletonFileName = "BillSea_NVOCC_Att_1.mrt.bak";

        public const string DescriptionReplacement =
            "*** ATTACHED PAGE FOR BILL OF " + "\r\n" + "LADING***";

        public static byte[] BuildAttachMrt(byte[] mainFormBytes, byte[] attachSkeletonBytes)
        {
            using var mainStream = new MemoryStream(mainFormBytes);
            using var skeletonStream = new MemoryStream(attachSkeletonBytes);
            var doc = BuildAttachDocument(XDocument.Load(mainStream), XDocument.Load(skeletonStream));
            return BillSeaLayoutFormService.DocumentToBytes(doc);
        }

        public static byte[] EnsureExportableAttachMrt(byte[] attachMrtBytes)
        {
            using var stream = new MemoryStream(attachMrtBytes);
            var doc = XDocument.Load(stream);
            EnsureUniquePage2Refs(doc);
            return BillSeaLayoutFormService.DocumentToBytes(doc);
        }

        public static XDocument BuildAttachDocument(XDocument mainFormDoc, XDocument attachSkeletonDoc)
        {
            var result = new XDocument(attachSkeletonDoc);
            var pagesContainer = FindPagesContainer(result)
                ?? throw new InvalidOperationException("Template Attach không có phần Pages.");

            var skeletonPages = FindPageNodes(attachSkeletonDoc).ToList();
            var skeletonPage2 = skeletonPages.Count > 1 ? skeletonPages[1] : null;
            var mainPage1 = FindPageNodes(mainFormDoc).FirstOrDefault()
                ?? throw new InvalidOperationException("Form chính không có page.");

            var newPage1 = new XElement(mainPage1);
            EnsurePageName(newPage1, "Page1");
            ReplaceDescriptionBinding(newPage1);

            pagesContainer.RemoveNodes();
            pagesContainer.SetAttributeValue("count", skeletonPage2 is null ? "1" : "2");
            pagesContainer.Add(newPage1);

            if (skeletonPage2 is not null)
            {
                var newPage2 = new XElement(skeletonPage2);
                EnsurePageName(newPage2, "Page2");
                RemapSubtreeRefs(newPage2, CollectRefIds(result));
                pagesContainer.Add(newPage2);
            }

            EnsureUniquePage2Refs(result);
            return result;
        }

        public static void EnsureUniquePage2Refs(XDocument doc)
        {
            var pages = FindPageNodes(doc).ToList();
            if (pages.Count < 2)
                return;

            var page2 = pages[1];
            var usedRefs = CollectRefIds(doc);
            foreach (var element in page2.DescendantsAndSelf())
            {
                if (element.Attribute("Ref") is { } refAttr
                    && int.TryParse(refAttr.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var refId))
                {
                    usedRefs.Remove(refId);
                }
            }

            RemapSubtreeRefs(page2, usedRefs);
        }

        private static HashSet<int> CollectRefIds(XDocument doc)
        {
            var refs = new HashSet<int>();
            foreach (var element in doc.Descendants())
            {
                if (element.Attribute("Ref") is { } refAttr
                    && int.TryParse(refAttr.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var refId))
                {
                    refs.Add(refId);
                }
            }

            return refs;
        }

        private static void RemapSubtreeRefs(XElement subtree, HashSet<int> usedRefs)
        {
            var refMap = new Dictionary<string, string>(StringComparer.Ordinal);
            var nextRef = usedRefs.Count == 0 ? 1 : usedRefs.Max() + 1;

            foreach (var element in subtree.DescendantsAndSelf().Where(x => x.Attribute("Ref") is not null))
            {
                var oldRef = element.Attribute("Ref")!.Value;
                if (refMap.ContainsKey(oldRef))
                    continue;

                while (usedRefs.Contains(nextRef))
                    nextRef++;

                var newRef = nextRef.ToString(CultureInfo.InvariantCulture);
                refMap[oldRef] = newRef;
                usedRefs.Add(nextRef);
                nextRef++;
            }

            foreach (var element in subtree.DescendantsAndSelf())
            {
                if (element.Attribute("Ref") is { } refAttribute
                    && refMap.TryGetValue(refAttribute.Value, out var mappedRef))
                {
                    refAttribute.Value = mappedRef;
                }

                foreach (var attribute in element.Attributes())
                {
                    // Bỏ qua "Ref" — đã remap ở nhánh trên. Nếu xử lý lại ở đây, attribute.Value
                    // lúc này đã là giá trị MỚI (vừa gán), có thể trùng với một oldRef khác trong
                    // refMap và bị remap chồng lần 2 → sinh Ref trùng (bug gốc gây "Item has already
                    // been added"). Chỉ còn "isRef" (Page/Parent) mới cần xử lý ở đây.
                    if (attribute.Name.LocalName == "Ref"
                        || !attribute.Name.LocalName.EndsWith("Ref", StringComparison.Ordinal))
                        continue;

                    if (refMap.TryGetValue(attribute.Value, out var mappedReference))
                        attribute.Value = mappedReference;
                }
            }
        }

        private static XElement? FindPagesContainer(XDocument doc) =>
            doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "Pages");

        private static IEnumerable<XElement> FindPageNodes(XDocument doc)
        {
            var pagesContainer = FindPagesContainer(doc);
            if (pagesContainer is null)
                yield break;

            foreach (var pageNode in pagesContainer.Elements())
            {
                if (pageNode.Attribute("type")?.Value == "Page" || pageNode.Element("PageWidth") is not null)
                    yield return pageNode;
            }
        }

        private static void EnsurePageName(XElement pageNode, string pageName)
        {
            var nameElement = pageNode.Element("Name");
            if (nameElement is null)
                pageNode.Add(new XElement("Name", pageName));
            else
                nameElement.Value = pageName;
        }

        private static void ReplaceDescriptionBinding(XElement pageNode)
        {
            foreach (var component in pageNode.Descendants())
            {
                var textNode = component.Element("Text");
                if (textNode is null)
                    continue;

                var name = component.Element("Name")?.Value?.Trim();
                if (string.Equals(name, "Text54", StringComparison.OrdinalIgnoreCase)
                    || ContainsDescriptionBinding(textNode.Value))
                {
                    textNode.Value = DescriptionReplacement;
                    component.Element("Type")?.Remove();
                    component.Element("Expressions")?.Remove();
                }
            }
        }

        private static bool ContainsDescriptionBinding(string? value) =>
            !string.IsNullOrEmpty(value)
            && value.Contains("{HBL.description}", StringComparison.OrdinalIgnoreCase);
    }
}
