using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace NVOAMASIS.Services
{
    public class FontService
    {
        private readonly IJSRuntime _jsRuntime;

        public FontService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task SetFontSize(string headerh6, string noidung, string tieude, string noidungluoi)
        {
            string noidungluoiRem = ConvertPxToRem(noidungluoi);
            await _jsRuntime.InvokeVoidAsync("updateFontSize", headerh6, noidung, tieude, noidungluoi);
        }

        private string ConvertPxToRem(string pxString)
        {
            if (pxString.EndsWith("px", StringComparison.OrdinalIgnoreCase))
            {
                if (double.TryParse(pxString.Replace("px", "").Trim(), out double pxValue))
                {
                    double remValue = pxValue / 16; // Giả định 1rem = 16px
                    return $"{remValue:0.###}rem"; // làm tròn đến 3 chữ số
                }
            }
            return pxString; // fallback nếu không phải px
        }
    }
    
}

