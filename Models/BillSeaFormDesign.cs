namespace NVOAMASIS.Models
{
    public enum BillSeaElementKind
    {
        HorizontalLine,
        VerticalLine,
        StaticText,
        DataText,
        Image
    }

    public class BillSeaDesignElement
    {
        public string Name { get; set; } = string.Empty;
        public BillSeaElementKind Kind { get; set; }
        public double Left { get; set; }
        public double Top { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string Text { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public double LineSize { get; set; } = 1;
        public string LineGuid { get; set; } = string.Empty;
        public string FontFamily { get; set; } = "Times New Roman";
        public double FontSize { get; set; } = 8;
        public bool FontBold { get; set; }
    }

    public class BillSeaPageLayout
    {
        public string PageName { get; set; } = string.Empty;
        public string PageRef { get; set; } = string.Empty;
        public string ComponentsParentRef { get; set; } = string.Empty;
        public double PageWidthInches { get; set; } = 8.27;
        public double PageHeightInches { get; set; } = 11.69;
        public List<BillSeaDesignElement> Elements { get; set; } = new();
    }
}
