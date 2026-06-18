using NVOAMASIS.Models;

namespace NVOAMASIS.Services
{
    public static class BillSeaLayoutClone
    {
        public static List<BillSeaPageLayout> ClonePages(IReadOnlyList<BillSeaPageLayout> pages) =>
            pages.Select(ClonePage).ToList();

        private static BillSeaPageLayout ClonePage(BillSeaPageLayout page) => new()
        {
            PageName = page.PageName,
            PageRef = page.PageRef,
            ComponentsParentRef = page.ComponentsParentRef,
            PageWidthInches = page.PageWidthInches,
            PageHeightInches = page.PageHeightInches,
            Elements = page.Elements.Select(CloneElement).ToList()
        };

        private static BillSeaDesignElement CloneElement(BillSeaDesignElement element) => new()
        {
            Name = element.Name,
            Kind = element.Kind,
            Left = element.Left,
            Top = element.Top,
            Width = element.Width,
            Height = element.Height,
            Text = element.Text,
            ImageUrl = element.ImageUrl,
            LineSize = element.LineSize,
            LineGuid = element.LineGuid,
            FontFamily = element.FontFamily,
            FontSize = element.FontSize,
            FontBold = element.FontBold
        };
    }
}
