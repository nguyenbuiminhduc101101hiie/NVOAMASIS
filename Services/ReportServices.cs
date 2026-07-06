using Humanizer;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using MudBlazor;
using OfficeOpenXml;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.Style;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Net;
using NVOAMASIS.Components.BaoCaoQuyTienMat.Pages;
using NVOAMASIS.Data;
using NVOAMASIS.Models;
using NVOAMASIS.Response;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static MudBlazor.CategoryTypes;
using static MudBlazor.Icons;
using static Stimulsoft.Report.StiOptions.Export;
using static NVOAMASIS.Components.BaoCaoQuyTienMat.Pages.BaoCaoQuyTienMat_Index;
using static NVOAMASIS.Components.Report.Pages.TruckingReport7_3;
using static NVOAMASIS.Components.Report.Pages.ShippingStatisticsReport;

namespace NVOAMASIS.Services
{
    public class ReportServices(AppDbContext _context,  NavigationManager nav, IJSRuntime JSRuntime,CustomerService Cussv, ShipmentService shipsv)
    {
        //demo
        public async Task<List<ListRef>> GetListREF(string brand)
        {
            try
            {
                var continuedValue = 1;
                var gFLCValue = brand + "%";
                var SOREF = await _context.Inbound.FromSqlInterpolated($@"
                 SELECT gFLC, REF as soref, MBL, COUNT(REF) AS Container20, CAST(DateUpdate AS date) as DateUpdate
                 FROM Inbound WHERE REF != '' 
                 AND REF IS NOT NULL AND Continued = {continuedValue} and gFLC like {gFLCValue}
                 GROUP BY REF , MBL, gFLC, CAST(DateUpdate AS date)
                ").Select(x => new ListRef
                {
                    gFLC = x.Gflc,
                    TotalHBL = x.Container20,
                    REF = x.soref,
                    MBL = x.MBL,
                    DateUpdate = x.DateUpdate
                }).OrderByDescending(x => x.DateUpdate)
                .ToListAsync();
                return SOREF;
            }
            catch(Exception ex)
            {
                return new List<ListRef>();
            }
        }

        public async Task<List<InboundFull>> GetDetailsInBound()
        {
            try
            {
                var result = await _context.InboundFulls.FromSqlInterpolated($@"
                   SELECT * FROM Inbound")
                    .ToListAsync();
                return result;

            }
            catch(Exception ex)
            {
                return new List<InboundFull>();
            }
        }

        public async Task<List<In_bound>> GetListInBound()
        {
            try
            {
				var result = await _context.Inbound.FromSqlInterpolated($@"
                   SELECT blib_id,ref as soref,gflc,status,eta,vessel,voyage,consignee,pol,pod,dest,macangfcl,kho,
                   stuff(ref,1,7,'') as [order],bl_type,diadiemgiaohang,canggiaohang,stt,air,fcl,
                   lcl,consol,paidreceived,lot,InvoiceRequest,InvoiceRequestDate,DebitIssued,
                   InvoiceIssued,Paid,Paiddebit,PaidCredit,nhanlenh,closeFile,MBL,HBL,BKNO,shipper,
                   notify,nodebit,nocredit,datereport,approve,editable,continued,userupdate,
                   DateUpdate,nvocc,
                   (select sum(convert(float,sokien)) from containerrepair where inboundid=inbound.blib_id group by inboundid ) as packages,
                   (select case when CONVERT(float, sokg) <> null then sum(convert(float,sokg)) else 0 end from containerrepair where inboundid=inbound.blib_id group by inboundid ) as kgs,
                   (select sum(convert(float,sokhoi)) from containerrepair where inboundid=inbound.blib_id group by inboundid ) as cbm,
                   (select count(*) from containerrepair where inboundid=inbound.blib_id and containertype like '%20%' group by inboundid ) as container20,
                   (select count(*) from containerrepair where inboundid=inbound.blib_id and containertype like '%4%' group by inboundid ) as container40   
            FROM Inbound 
           
                ").ToListAsync();
				return result;
			}
			catch (Exception ex) 
            {
                return new List<In_bound>();
            }
        }



        public async Task<string[]> PrintExportReport()
        {
            try
            {
                var exportsucces = await ExportToExcel();

                if (!exportsucces)
                    return ["Export Report Fail","0"];

                return ["Export Report Successfully", "1"];

            }
            catch (Exception ex)
            {
                return ["Export Report Fail with error code:" +ex.Message, "0"];
            }
           
        }
        public async Task<string[]> PrintExportReport_BaoCaoQuyTienMat(double? sodudauky, double? soducuoiky, double? tongsotien, DateTime? startdate, DateTime? Enddate, List<BaoCaoQuyTienMat_Index.M_BaocaoQuyTienMat> Listdata)
        {
            try
            {
                var exportsucces = await ExportToExcel_BaoCaoQuyTienMat(sodudauky, soducuoiky, tongsotien, startdate, Enddate , Listdata);

                if (!exportsucces)
                    return ["Export Report Fail", "0"];

                return ["Export Report Successfully", "1"];

            }
            catch (Exception ex)
            {
                return ["Export Report Fail with error code:" + ex.Message, "0"];
            }

        }

        private async Task<byte[]> ExportToExcelLocal()
        {

            var imagePath = "ImageForReport/ImgTesting.png";
			var image = new FileInfo(Path.Combine("wwwroot", imagePath));

			ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using(var memoryStream = new MemoryStream())
            {
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Sheet1");
                    worksheet.Cells["A1:AJ1"].Merge = true;
                    worksheet.Cells["A1"].Value = "LMS,LTD";
                    worksheet.Cells["A1"].Style.Font.Size = 30;
                    worksheet.Cells["A1"].Style.Font.Bold = true;
                    worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells["A1"].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                    worksheet.Cells["A1"].Style.Font.Color.SetColor(System.Drawing.Color.Red);

                    var excelImage = worksheet.Drawings.AddPicture("ImageName", image);
                    excelImage.SetPosition(1, 1);
                    excelImage.SetSize(200, 100); 

                    //worksheet.Cells.LoadFromCollection(data, true);
                    await package.SaveAsAsync(memoryStream);
                }

                return memoryStream.ToArray();
            }
        }
        private List<M_Customer> List_Cus = new List<M_Customer>();
        public CultureInfo _en = CultureInfo.GetCultureInfo("en-US");
     
        private async Task<byte[]> ExportToExcelLocal_BaoCaoQuyTienMat(double? sodudauky, double? soducuoiky, double? tongsotien, DateTime? startdate, DateTime? Enddate , List<BaoCaoQuyTienMat_Index.M_BaocaoQuyTienMat> Listdata)
        {
            List_Cus = await Cussv.GetList();

            var imagePath = "ImageForReport/ImgTesting.png";
            var image = new FileInfo(Path.Combine("wwwroot", imagePath));

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var memoryStream = new MemoryStream())
            {
                using (var package = new ExcelPackage())
                {
                    
                    var worksheet = package.Workbook.Worksheets.Add("Báo cáo quỹ tiền mặt");

                    // Gộp và tạo tiêu đề chính
                    worksheet.Cells["C2:F2"].Merge = true;
                    worksheet.Cells["C2"].Value = "BÁO CÁO QUỸ TIỀN MẶT";
                    worksheet.Cells["C2"].Style.Font.Size = 16;
                    worksheet.Cells["C2"].Style.Font.Bold = true;
                    worksheet.Cells["C2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells["C2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    // Tiêu đề cột (row 4)
                    worksheet.Cells[4, 2].Value = "Ngày";
                    worksheet.Cells[4, 3].Value = "Khách hàng";
                    worksheet.Cells[4, 4].Value = "Loại giao dịch";
                    worksheet.Cells[4, 5].Value = "Số tiền";
                    worksheet.Cells[4, 6].Value = "Ghi chú";
                    worksheet.Cells[4, 7].Value = "Loại tiền";

                    using (var range = worksheet.Cells[4, 2, 4, 7])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    }

                    // Đổ dữ liệu bắt đầu từ dòng 5
                    int row = 5;
                    foreach (var item in Listdata)
                    {
                        worksheet.Cells[row, 2].Value = item.Ngay?.ToString("dd/MMM/yyyy");
                        worksheet.Cells[row, 3].Value = CustomerName(item.Customer_id);
                        worksheet.Cells[row, 4].Value = item.Loaigiaodich;
                        worksheet.Cells[row, 5].Value = item.Sotien.HasValue ? item.Sotien.Value.ToString("#,##0", _en) : "0";
                        worksheet.Cells[row, 6].Value = item.GhiChu;
                        worksheet.Cells[row, 7].Value = item.Currency;
                        row++;
                    }

                    // Đóng khung dữ liệu
                    int dataStartRow = 5;
                    int dataEndRow = row - 1;
                    using (var dataRange = worksheet.Cells[dataStartRow, 2, dataEndRow, 7])
                    {
                        dataRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        dataRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        dataRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        dataRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    }

                    // Tổng và số dư
                    worksheet.Cells[row, 4].Value = "Total(Thu-Chi):";
                    worksheet.Cells[row, 5].Value = tongsotien.HasValue ? tongsotien.Value.ToString("#,##0", _en) : "0";
                    row++;

                    worksheet.Cells[row, 4].Value = "Số dư đầu kỳ:";
                    worksheet.Cells[row, 5].Value = sodudauky.HasValue ? sodudauky.Value.ToString("#,##0", _en) : "0";
                    row++;

                    worksheet.Cells[row, 4].Value = "Số dư cuối kỳ:";
                    worksheet.Cells[row, 5].Value = soducuoiky.HasValue ? soducuoiky.Value.ToString("#,##0", _en) : "0";

                    // Căn chỉnh độ rộng cột
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                    await package.SaveAsAsync(memoryStream);
                }

                return memoryStream.ToArray();
            }
        }

        // Sự kiện click để xuất dữ liệu ra Excel
        public async Task<bool> ExportToExcel()
        {
            try
            {
                var excelData = await ExportToExcelLocal();
                var fileName = "Report.xlsx"; // Tên tệp Excel

                // Tạo một MemoryStream để lưu trữ file Excel
                var memoryStream = new MemoryStream(excelData);

                // Thêm kiểu MIME cho phản hồi
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                // Tạo phản hồi HTTP
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(excelData)
                };

                // Thiết lập header Content-Disposition để chỉ định tên tệp khi tải về
                response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                {
                    FileName = fileName
                };

                // Thiết lập kiểu MIME của nội dung
                response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

                // Gọi JavaScript để tải tệp xuống
                await JSRuntime.InvokeVoidAsync("saveAsFile", fileName, Convert.ToBase64String(excelData));
                return true;

            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ExportToExcel_BaoCaoQuyTienMat(double? sodudauky, double? soducuoiky, double? tongsotien, DateTime? startdate, DateTime? Enddate , List<BaoCaoQuyTienMat_Index.M_BaocaoQuyTienMat> Listdata)
        {
            try
            {
                var excelData = await ExportToExcelLocal_BaoCaoQuyTienMat(sodudauky, soducuoiky, tongsotien, startdate, Enddate , Listdata);
                var fileName = "Report.xlsx"; // Tên tệp Excel

                // Tạo một MemoryStream để lưu trữ file Excel
                var memoryStream = new MemoryStream(excelData);

                // Thêm kiểu MIME cho phản hồi
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                // Tạo phản hồi HTTP
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(excelData)
                };

                // Thiết lập header Content-Disposition để chỉ định tên tệp khi tải về
                response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                {
                    FileName = fileName
                };

                // Thiết lập kiểu MIME của nội dung
                response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

                // Gọi JavaScript để tải tệp xuống
                await JSRuntime.InvokeVoidAsync("saveAsFile", fileName, Convert.ToBase64String(excelData));
                return true;

            }
            catch
            {
                return false;
            }
        }
        string CustomerName(Guid? customerID)
        {
            var rs = List_Cus.FirstOrDefault(x => x.Customer_ID == customerID);
            return rs != null ? rs.COMPANY! : "";
        }
        public async Task<List<string>> GetVesselcmb()
        {
            try
            {
                var result = await _context.Inbound.FromSqlInterpolated($@"
                SELECT DISTINCT VESSEL FROM INBOUND WHERE CONTINUED = 1
                ").Select(x => x.Vessel!)
                .OrderBy(x => x)
                .ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                return new List<string>()!;
            }
            
        }
        public async Task<List<string>> GetPOL_POD_DESTcmb()
        {
            try
            {
                var result = await _context.InboundFulls.FromSqlInterpolated($@"
                Select port_code AS POLCode from PORT where show=1 and PORT is not null and PORT != ''
                ")
                .Select(x => x.POLCode!)
                .OrderBy(x => x)
                .ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                return new List<string>()!;
            }

        }
        public async Task<List<string>> GetSalescmb()
        {
            try
            {
                var result = await _context.InboundFulls.FromSqlInterpolated($@"
                Select salecode from sale where CONTINUED=1
                ")
                .Select(x => x.SaleCode!)
                .OrderBy(x => x)
                .ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
               return new List<string>()!;
            }

        }

        public async Task<string> PortNamefromPortCode(string port_code)
        {
            try
            {
                var result = await _context.InboundFulls.FromSqlInterpolated($@"
                Select port AS POLCode from PORT where PORT_CODE = {port_code}
                ").Select(x => x.POLCode!)
                .FirstOrDefaultAsync();
                return result!;
            }
            catch (Exception ex)
            {
               return string.Empty;
            }

        }

        public async Task<List<string>> GetDelscmb()
        {
            try
            {
                var result = await _context.InboundFulls.FromSqlInterpolated($@"
               Select distinct del from inbound where CONTINUED=1 and DEL is not null and del != ''
                ")
                .Select(x => x.DEL!)
                .OrderBy(x => x)
                .ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
               return new List<string>()!;
            }

        }
        public async Task<List<string>> GetCYcmb()
        {
            try
            {
                var result = await _context.InboundFulls.FromSqlInterpolated($@"
                Select terminalName as  ImportCY From terminal where Continued = 1
                ")
                .Select(x => x.ImportCY!)
                .OrderBy(x => x)
                .ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
               return new List<string>()!;
            }

        }

        public async Task<List<string>> GetCarriercmb()
        {
            try
            {
                var result = await _context.InboundFulls.FromSqlInterpolated($@"
                Select company as ShippingLine From customer where(maincode like '%Shipping%') and Continued = 1
                ")
                .Select(x => x.ShippingLine!)
                .OrderBy(x => x)
                .ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
               return new List<string>()!;
            }

        }
        public async Task<List<string>> GetOPScmb()
        {
            try
            {
                var result = await _context.InboundFulls.FromSqlInterpolated($@"
                SELECT usr as ops from UserList
                ")
                .Select(x => x.ops!)
                .OrderBy(x => x)
                .ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
               return new List<string>()!;
            }

        }
        public async Task<List<string>> GetPortandWHcmb()
        {
            try
            {
                var result = await _context.InboundFulls.FromSqlInterpolated($@"
                Select terminalid, terminalname +'-'+ code as macangFCL  from terminal where CONTINUED=1 
                ")
                .Select(x => x.macangFCL!)
                .OrderBy(x => x)
                .ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
               return new List<string>()!;
            }

        }
        public async Task<List<string>> GetLoaiHangcmb()
        {
            try
            {
                var result = await _context.InboundFulls.FromSqlInterpolated($@"
                Select distinct loaihang from inbound where CONTINUED=1
                ")
                .Select(x => x.loaihang!)
                .OrderBy(x => x)
                .ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
               return new List<string>()!;
            }

        }
        public async Task<List<string>> GetGroupcmb()
        {
            try
            {
                var result = await _context.InboundFulls.FromSqlInterpolated($@"
                Select distinct itemSITC from inbound
                ")
                .Select(x => x.ItemSITC!)
                .OrderBy(x => x)
                .ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
               return new List<string>()!;
            }

        }
        public async Task<List<string>> GetCoLoadercmb()
        {
            try
            {
                var result = await _context.InboundFulls.FromSqlInterpolated($@"
                Select Customer_ID, shortname + '_' + Company as COLOADER_INBOUND From Customer where Continued= 1
                ")
                .Select(x => x.COLOADER_INBOUND!)
                .OrderBy(x => x)
                .ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
               return new List<string>()!;
            }

        }
        public async Task<List<Container_InBound>> GetListContainer(string InboundID)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var result = await _context.ContainerRepair.FromSqlInterpolated($@"
               SELECT * FROM Containerrepair
                WHERE inboundId = {InboundID.ToUpper()}
                ").ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                return new List<Container_InBound>();
            }

        }
        public async Task<List<string>> GetContTypecmb()
        {
            try
            {
                var result = await _context.ContainerRepair.FromSqlInterpolated($@"
                SELECT distinct containertype FROM Containerrepair where containertype != ''
                ")
                .Select(x => x.containertype!)
                .OrderBy(x => x)
                .ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
               return new List<string>()!;
            }

        }
        public async Task<List<string>> GetOwnercmb()
        {
            try
            {
                var result = await _context.ContainerRepair.FromSqlInterpolated($@"
                Select taxcode + '-' + company as Owner from customer where CONTINUED=1 and maincode like '%agent%'
                ")
                .Select(x => x.Owner!)
                .OrderBy(x => x)
                .ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
               return new List<string>()!;
            }

        }

        public async Task<List<string>> AddOrUpdateContainer(Container_InBound cons)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if(cons?.inboundId == null)
                {
                    return ["Nothing To Save!", "2"];

                }
                else if (cons?.InboundContainersID == Guid.Empty)
                {
                    //add
                    _context?.ContainerRepair.Add(cons!);
                    await _context?.SaveChangesAsync()!;
                    return ["Create New Container Successfully", "1"];
                }
                else
                {
                    //update
                    _context?.ContainerRepair.Update(cons!);
                    await _context?.SaveChangesAsync()!;
                    return ["Update Container Successfully", "1"];
                }

            }
            catch(Exception ex)
            {
                return ["Can not Add or Update with error code: " + ex.Message, "0"];
            }

        }
        public async Task<List<string>> DeleteAnContainer(Container_InBound contDetail)
        {
            _context.ChangeTracker.Clear();
            try 
            { 
                if(contDetail?.InboundContainersID == Guid.Empty)
                    return ["Container ID empty", "0"];

                _context?.ContainerRepair.Remove(contDetail!);
                await _context?.SaveChangesAsync()!;
                return ["Deleted an Container Successfully", "1"];
            }
            catch (Exception ex)
            {
                return ["Can not Delete with error code: " + ex.Message, "0"];
            }
        }
        public async Task<List<M_Customer>> GetCustomerCmb()
        {
            try
            {
                var result = await _context.Customer
                .Where(x=>x.Continued == true)
                .ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                return new List<M_Customer>();
            }

        }
        public async Task<List<string>> GetShipperCmb()
        {
            try
            {
                var result = await _context.InboundFulls.FromSqlInterpolated($@"
                Select distinct Shipper from inbound where CONTINUED=1 and Shipper is not null
                ")
                .Select(x => x.SHIPPER!)
                .OrderBy(x => x)
                .ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
               return new List<string>()!;
            }

        }
        public async Task<List<string>> GetConsigneeCmb()
        {
            try
            {
                var result = await _context.InboundFulls.FromSqlInterpolated($@"
                Select distinct CONSIGNEE from inbound where CONTINUED=1 and CONSIGNEE is not null
                ")
                .Select(x => x.CONSIGNEE!)
                .OrderBy(x => x)
                .ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
               return new List<string>()!;
            }

        }
        public async Task<List<string>> GetNotifyCmb()
        {
            try
            {
                var result = await _context.InboundFulls.FromSqlInterpolated($@"
                Select distinct Notify from inbound where CONTINUED=1 and Notify is not null
                ")
                .Select(x => x.NOTIFY!)
                .OrderBy(x => x)
                .ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
               return new List<string>()!;
            }

        }
        public async Task<List<string>> GetAgentCmb()
        {
            try
            {
                var result = await _context.Customer.FromSqlInterpolated($@"
                Select customer_id,shortname + '__________-' + company as company from customer where CONTINUED=1 and maincode like '%agent%'
                ")
                .Select(x => x.COMPANY!)
                .OrderBy(x => x)
                .ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
               return new List<string>()!;
            }

        }
        public async Task<List<string>> GetPortCmb()
        {
            try
            {
                var result = await _context.Port.FromSqlInterpolated($@"
                Select Port_code + '-' + Port as PORT From Port where show=1 and Continued=1 AND PORT IS NOT NULL AND PORT_CODE IS NOT NULL 
                ")
                .Select(x => x.PORT!)
                .ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
               return new List<string>();
            }

        }
        public async Task<List<ListDebit>> GetListDebit(string inboundID)
        {
            try
            {
                var Result = await _context.listDebits.FromSqlInterpolated($@"
                Select company + '-' + taxcode  as company,dept,charge_code + '__________' + dvt  + '__________' + charge as item,currency,quantity,
                containertype,unitprice_,taxprice,price_ ,unitprice,price,thanhtiensauthueVND,inboundfreight.tigia, note,itemid, no_, inboundid,inboundfreightid,
                customerid,container,boss,ktt,freedem,freedet, pricetruocthue,pricenotaxvnd,  pricethue, paycheck, os, ngay, ngayhoadon, dongiatruocthueVND,
                thanhtientruocthueVND,tienthueVND,eraseno,bkno,inboundfreight.userupdate,inboundfreight.dateupdate,inboundfreight.approve,showarrival,
                soNgayCongNo,daily ,housebill_debitcredit,ref_debitcredit,inboundfreight.stt,showvnd,f1,f2,t1,t2,p1,p2,level,freightdatereport,quyenbaocao 
                From inboundfreight left join customer on inboundfreight.customerid=customer.customer_id  
					                left join charge on inboundfreight.itemid=charge.charge_id 
					                left join inbound on inboundfreight.inboundid = inbound.blib_id 
                Where inboundid = {inboundID}  and debitcredit = 'Debit'
                ")
                .OrderBy(x => x.stt)
                .ToListAsync();
                return Result;
            }
            catch(Exception ex)
            {
                return new List<ListDebit>();
            }
        }
        public async Task<List<string>> GetStyleCmb()
        {
            try
            {
                var result = await _context.Port.FromSqlInterpolated($@"
                Select OptionValue as Port From [option] where frmname='servicetype' and continued=1 
                ")
                .Select(x => x.PORT!)
                .ToListAsync();
                string[] ResultSplit = result?.FirstOrDefault()!.Split(',')!;
                result!.Clear();
                foreach(var data in ResultSplit)
                {
                    result.Add(data);
                }
                return result;
            }
            catch (Exception ex)
            {
               return new List<string>();
            }

        }
        public async Task<List<ChargeModel>> GetChargeCmb()
        {
            try
            {
                var result = await _context.Charge.FromSqlInterpolated($@"
                Select charge_id,charge_code + '__________' + dvt  + '__________' + 
                charge as charge_code from charge where CONTINUED=1 
                ")
                .Select( x => new ChargeModel
                {
                    CHARGE_ID = x.CHARGE_ID,
                    CHARGE_CODE = x.CHARGE_CODE
                })
                .ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                return new List<ChargeModel>();
            }

        }
        public Task<BoolandMessReponse> Emanifest(M_MBL mbl) => Emanifest(mbl, null);

        public async Task<BoolandMessReponse> Emanifest(M_MBL mbl, M_HBL? hbl)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var templatePath = new FileInfo(Path.Combine("wwwroot", "Reports", "EManifestReport.xlsx"));
                using var package = new ExcelPackage(templatePath);
                var ws = package.Workbook.Worksheets[0];

                _context.ChangeTracker.Clear();
                var hbls = hbl != null
                    ? _context.HBL.Where(h => h.hblID == hbl.hblID).ToList()
                    : _context.HBL.Where(h => h.mblid == mbl.MblID).ToList();
                var allcont = _context.Container.ToList();
                var conts = allcont
                               .Where(c => hbls.Select(h => (Guid?)h.hblID).Contains(c.hblid))
                               .ToList();

                // --- 1) Chèn dòng cho HBL nếu cần ---
                int hblCount = hbls.Count;
                int extraHblRows = Math.Max(0, hblCount - 1);
                if (extraHblRows > 0)
                {
                    // Chèn thêm extraHblRows dòng tại row 5, copy style từ row 4
                    ws.InsertRow(5, extraHblRows, 4);
                }

                // --- 2) Ghi HBL bắt đầu từ row 4 ---
                int currentRow = 4;
                foreach (var h in hbls)
                {
                    var hblConts = conts.Where(c => c.hblid == h.hblID).ToList();
                    int col = 1;
                    ws.Cells[currentRow, col++].Value = 1;
                    ws.Cells[currentRow, col++].Value = "";
                    ws.Cells[currentRow, col++].Value = DateTime.Now.Year;
                    ws.Cells[currentRow, col++].Value = "";
                    ws.Cells[currentRow, col++].Value = h.shipper;
                    ws.Cells[currentRow, col++].Value = h.consignee;
                    ws.Cells[currentRow, col++].Value = h.notify1;
                    ws.Cells[currentRow, col++].Value = h.notify2;
                    ws.Cells[currentRow, col++].Value = "";
                    ws.Cells[currentRow, col++].Value = h.podcode;
                    ws.Cells[currentRow, col++].Value = h.polcode;
                    ws.Cells[currentRow, col++].Value = h.podcode;
                    ws.Cells[currentRow, col++].Value = h.delcode;
                    ws.Cells[currentRow, col++].Value = h.Air_type;
                    ws.Cells[currentRow, col++].Value = h.hbl;
                    ws.Cells[currentRow, col++].Value = h.dateLaden;
                    ws.Cells[currentRow, col++].Value = mbl.Mbl;
                    ws.Cells[currentRow, col++].Value = mbl.DateLaden;
                    ws.Cells[currentRow, col++].Value = h.dateLaden;

                    ws.Cells[currentRow, col++].Value = hblConts.Sum(c => double.TryParse(c.cbm, out double cbm) ?  cbm : 0);
                    ws.Cells[currentRow, col++].Value = hblConts.Count > 0 ? hblConts.First().pkgsCode : string.Empty;
                    ws.Cells[currentRow, col++].Value = hblConts.Sum(c => c.GrossWeight);
                    ws.Cells[currentRow, col++].Value = "KGM";
                    ws.Cells[currentRow, col++].Value = h.description;

                    // disable wrapping on the entire HBL block
                    ws.Cells[currentRow, 1, currentRow, 25]
                      .Style.WrapText = false;
                    currentRow++;
                }

                // --- 3) Chèn dòng cho containers nếu cần ---
                // Vị trí bắt đầu container trong template là row 7 trên file gốc
                int baseContRow = 7;
                int contStartRow = baseContRow + extraHblRows;
                int contCount = conts.Count;
                int extraContRows = Math.Max(0, contCount - 1);
                if (extraContRows > 0)
                {
                    // Chèn thêm extraContRows dòng ngay sau contStartRow
                    ws.InsertRow(contStartRow + 1, extraContRows, contStartRow);
                }

                // --- 4) Ghi container ---
                currentRow = contStartRow;
                foreach (var c in conts)
                {
                    int col = 2; // cột 1 để STT nếu cần, ta để trống
                    ws.Cells[currentRow, col++].Value = "";
                    ws.Cells[currentRow, col++].Value = c.description;
                    ws.Cells[currentRow, col++].Value = c.GrossWeight;
                    ws.Cells[currentRow, col++].Value = 0;
                    ws.Cells[currentRow, col++].Value = c.CONTAINER_NO;
                    ws.Cells[currentRow, col++].Value = c.Seal;
                    currentRow++;
                }

                // --- 5) Xuất file về client ---
                using var ms = new MemoryStream();
                await package.SaveAsAsync(ms);
                var base64 = Convert.ToBase64String(ms.ToArray());
                var fileName = hbl != null
                    ? $"EManifest_{hbl.hbl}_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
                    : $"EManifest{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                await JSRuntime.InvokeVoidAsync("saveAsFile", fileName, base64);

                return new BoolandMessReponse(true, "Export successfully");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, $"Export failed: {ex.Message}");
            }
        }

        // 7.4 Agent Report Export
        public async Task<BoolandMessReponse> AgentReportExportExcel(List<AgentReportRow> rows, DateRange dateRange)
        {
            try
            {
                if (rows == null || rows.Count == 0)
                    return new BoolandMessReponse(false, "No data to export");

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var package = new ExcelPackage();

                // Group by Agent(Carrier). Each agent -> one sheet
                var groups = rows.GroupBy(r => string.IsNullOrWhiteSpace(r.Carrier) ? "(NO AGENT)" : r.Carrier!.Trim())
                                 .OrderBy(g => g.Key);

                foreach (var g in groups)
                {
                    // Sheet name max 31 chars, remove invalid characters
                    var sheetNameRaw = g.Key;
                    foreach (var c in Path.GetInvalidFileNameChars())
                        sheetNameRaw = sheetNameRaw.Replace(c.ToString(), " ");
                    var sheetName = sheetNameRaw.Length > 31 ? sheetNameRaw[..31] : sheetNameRaw;

                    var ws = package.Workbook.Worksheets.Add(sheetName);

                    int headerRow = 1;
                    ws.Cells[headerRow, 1].Value = "NO";
                    ws.Cells[headerRow, 2].Value = "JOB FILE NO";
                    ws.Cells[headerRow, 3].Value = "POL";
                    ws.Cells[headerRow, 4].Value = "POD";
                    ws.Cells[headerRow, 5].Value = "AGENT NAME";
                    ws.Cells[headerRow, 6].Value = "CARRIER";
                    ws.Cells[headerRow, 7].Value = "HB/L";
                    ws.Cells[headerRow, 8].Value = "MB/L";
                    ws.Cells[headerRow, 9].Value = "AMOUNT";
                    ws.Cells[headerRow, 10].Value = "CLIENT";

                    int dataRow = headerRow + 1;
                    foreach (var r in g)
                    {
                        int col = 1;
                        ws.Cells[dataRow, col++].Value = r.No;
                        ws.Cells[dataRow, col++].Value = r.JobFileNo;
                        ws.Cells[dataRow, col++].Value = r.POL;
                        ws.Cells[dataRow, col++].Value = r.POD;
                        ws.Cells[dataRow, col++].Value = r.Agentname;
                        ws.Cells[dataRow, col++].Value = r.Carrier;
                        ws.Cells[dataRow, col++].Value = r.HBL;
                        ws.Cells[dataRow, col++].Value = r.MBL;
                        ws.Cells[dataRow, col++].Value = r.Amount;
                        ws.Cells[dataRow, col++].Value = r.Client;
                        dataRow++;
                    }
                    ws.Cells[dataRow, 8].Value = "TOTAL";
                    ws.Cells[dataRow, 9].Formula = $"SUM(I{headerRow + 1}:I{dataRow - 1})";
                    ws.Cells[ws.Dimension.Address].AutoFitColumns();
                }

                using var memoryStream = new MemoryStream();
                await package.SaveAsAsync(memoryStream);
                var fileData = memoryStream.ToArray();
                var fileName = $"AGENT_REPORT_{dateRange.Start:yyyyMMdd}_{dateRange.End:yyyyMMdd}_{DateTime.Now:HHmmss}.xlsx";
                await JSRuntime.InvokeVoidAsync("saveAsFile", fileName, Convert.ToBase64String(fileData));
                return new BoolandMessReponse(true, "Export successfully");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, $"Export failed: {ex.Message}");
            }
        }

        public async Task<BoolandMessReponse> ExportDebtReportToExcelWithTemplateAsync(List<DebtReportRow> data, string customerName, string cur)
        {
            try
            {
                if (data == null || data.Count == 0)
                    return new BoolandMessReponse(false, "No data to export");

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                // Đọc từ file mẫu
                var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Reports", "TemplateCongNoKhach.xlsx");
                if (!File.Exists(templatePath))
                    return new BoolandMessReponse(false, "Template file not found");

                using var package = new ExcelPackage(new FileInfo(templatePath));
                var ws = package.Workbook.Worksheets.First();

                // Ghi Customer vào B6
                ws.Cells["B6"].Value = customerName;

                // Bắt đầu từ dòng 11
                int startRow = 11;
                int row = startRow;

                int stt = 1;
                foreach (var item in data)
                {
                    ws.Cells[row, 2].Value = stt;                  // Cột B (STT)
                    ws.Cells[row, 3].Value = item.Description;     // Cột C
                    ws.Cells[row, 4].Value = item.Hbl;             // D
                    ws.Cells[row, 5].Value = item.Qty;             // E
                    ws.Cells[row, 6].Value = item.Unit;            // F
                    ws.Cells[row, 7].Value = item.Total_Debit;   // G
                    ws.Cells[row, 8].Value = item.Total_Credit; // H
                    ws.Cells[row, 9].Value = item.Total_HoaDonDauRa;   // G
                    ws.Cells[row, 10].Value = item.Total_HoaDonDauVao; // H
                    ws.Cells[row, 11].Value = item.InvoiceNo;       // I

                    row++;
                    stt++;
                }


                // Thêm TOTAL ngay dưới dữ liệu
                ws.Cells[row, 2, row, 6].Merge = true;       // Merge cột B..F
                ws.Cells[row, 2].Value = "TOTAL";
                ws.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                ws.Cells[row, 2].Style.Font.Bold = true;

                ws.Cells[row, 7].Formula = $"SUM(G{startRow}:G{row - 1})";
                ws.Cells[row, 8].Formula = $"SUM(H{startRow}:H{row - 1})";

                // Format số cho cột tiền
                ws.Column(7).Style.Numberformat.Format = "#,##0.00";
                ws.Column(8).Style.Numberformat.Format = "#,##0.00";
                package.Workbook.Calculate();

                double gValue = Convert.ToDouble(ws.Cells[row, 7].Value ?? 0);
                double hValue = Convert.ToDouble(ws.Cells[row, 8].Value ?? 0);

                // Tính tổng
                double totalAmount = gValue + hValue;

                row++;

                // Thêm TOTAL ngay dưới dữ liệu
                ws.Cells[row, 2, row, 7].Merge = true;       // Merge cột B..F
                string amountInWords = NumberToWords(totalAmount, cur);
                ws.Cells[row, 2].Value = $"Amount in words ((Bằng chữ)): {amountInWords}";
                ws.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                ws.Cells[row, 2].Style.Font.Bold = true;
                ws.Cells[row, 8].Formula = totalAmount.ToString();
                ws.Column(8).Style.Numberformat.Format = "#,##0.00";

                // Đóng khung toàn bộ dữ liệu + dòng TOTAL
                int endRow = row; // bao gồm cả TOTAL
                using (var range = ws.Cells[startRow, 2, endRow, 9])
                {
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }
                row++;
                row++;
                // Bổ sung Remarks từ dòng 16
                int remarkRow = row;

                // Gộp cột B..I cho Remarks
                ws.Cells[remarkRow, 2, remarkRow, 9].Merge = true;
                ws.Cells[remarkRow, 2].Value = "Remarks: Please pay at the exchange rate of VCB sold at the date of payment.";
                ws.Cells[remarkRow, 2].Style.WrapText = true;
                ws.Cells[remarkRow, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                ws.Cells[remarkRow, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                remarkRow++;
                ws.Cells[remarkRow, 2, remarkRow, 9].Merge = true;
                ws.Cells[remarkRow, 2].Value = "Freight payment by bank transfer tax rate of 0% but by cash tax rate of 8%";
                ws.Cells[remarkRow, 2].Style.WrapText = true;
                ws.Cells[remarkRow, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                ws.Cells[remarkRow, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                remarkRow++;
                ws.Cells[remarkRow, 2, remarkRow, 9].Merge = true;
                ws.Cells[remarkRow, 2].Value = "(Vui lòng thanh toán theo tỷ giá bán ra của ngân hàng HSBC tại thời điểm thanh toán.";
                ws.Cells[remarkRow, 2].Style.WrapText = true;
                ws.Cells[remarkRow, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                ws.Cells[remarkRow, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                remarkRow++;
                ws.Cells[remarkRow, 2, remarkRow, 9].Merge = true;
                ws.Cells[remarkRow, 2].Value = "Cước chuyển khoản thuế suất 0% và Tiền mặt thuế suất 8%)";
                ws.Cells[remarkRow, 2].Style.WrapText = true;
                ws.Cells[remarkRow, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                ws.Cells[remarkRow, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                remarkRow++;
                ws.Cells[remarkRow, 2, remarkRow, 6].Merge = true;
                ws.Cells[remarkRow, 2].Value = "Please help us Please remit to below account ";
                ws.Cells[remarkRow, 2].Style.Font.Bold = true;
                ws.Cells[remarkRow, 2].Style.WrapText = true;
                ws.Cells[remarkRow, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                ws.Cells[remarkRow, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                remarkRow++;
                ws.Cells[remarkRow, 2, remarkRow, 6].Merge = true;
                ws.Cells[remarkRow, 2].Value = "ACCCOUNT INFORMATION ( VND ):";
                ws.Cells[remarkRow, 2].Style.Font.Bold = true;
                ws.Cells[remarkRow, 2].Style.WrapText = true;
                ws.Cells[remarkRow, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                ws.Cells[remarkRow, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                remarkRow++;
                ws.Cells[remarkRow, 2, remarkRow, 6].Merge = true;
                ws.Cells[remarkRow, 2].Value = "Company name: GS GLOBAL";
                ws.Cells[remarkRow, 2].Style.WrapText = true;
                ws.Cells[remarkRow, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                ws.Cells[remarkRow, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Top;


                remarkRow++;
                ws.Cells[remarkRow, 2, remarkRow, 6].Merge = true;
                ws.Cells[remarkRow, 2].Value = "Account no: ";
                ws.Cells[remarkRow, 2].Style.WrapText = true;
                ws.Cells[remarkRow, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                ws.Cells[remarkRow, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Top;


                remarkRow++;
                ws.Cells[remarkRow, 2, remarkRow, 6].Merge = true;
                ws.Cells[remarkRow, 2].Value = "At: ";
                ws.Cells[remarkRow, 2].Style.WrapText = true;
                ws.Cells[remarkRow, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                ws.Cells[remarkRow, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                remarkRow++;
                remarkRow++;
                ws.Cells[remarkRow, 4, remarkRow, 9].Merge = true;
                ws.Cells[remarkRow, 4].Value = "Thanks & Best Regards!";
                ws.Cells[remarkRow, 2].Style.Font.Bold = true;
                ws.Cells[remarkRow, 4].Style.WrapText = true;
                ws.Cells[remarkRow, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[remarkRow, 4].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                remarkRow++;
                ws.Cells[remarkRow, 4, remarkRow, 9].Merge = true;
                ws.Cells[remarkRow, 4].Value = "GS GLOBAL";
                ws.Cells[remarkRow, 2].Style.Font.Bold = true;
                ws.Cells[remarkRow, 4].Style.WrapText = true;
                ws.Cells[remarkRow, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[remarkRow, 4].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                // Xuất file
                using var memoryStream = new MemoryStream();
                await package.SaveAsAsync(memoryStream);
                var fileData = memoryStream.ToArray();
                var fileName = $"DEBT_REPORT_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                await JSRuntime.InvokeVoidAsync("saveAsFile", fileName, Convert.ToBase64String(fileData));

                return new BoolandMessReponse(true, "Export successfully");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, $"Export failed: {ex.Message}");
            }
        }
        public static string NumberToWords(double amount, string cur)
        {
            // Làm tròn trước khi đổi sang chữ
            long rounded = (long)Math.Round(amount);

            if (cur.Equals("VND", StringComparison.OrdinalIgnoreCase))
            {
                return rounded.ToWords(new System.Globalization.CultureInfo("vi-VN")) + " đồng";
            }
            else if (cur.Equals("USD", StringComparison.OrdinalIgnoreCase))
            {
                return rounded.ToWords(new System.Globalization.CultureInfo("en-US")) + " dollars";
            }
            else
            {
                return amount.ToString();
            }
        }
        public async Task<HBLTruckGridItem> GetHBLTruckGridItem(M_HBL h)
        {
            var item = new HBLTruckGridItem
            {
                DateReport = h.datereport,
                HBL = h.hbl,
                TruckReqNo = h.Truck_YeucauTruckingNo,
                LenhNo = h.Truck_LenhDieuXeNo,
                NoOfPackages = h.NoOfPackages
            };

            var yctruck = await _context.YeuCauTrucking.FirstOrDefaultAsync(x => x.YeuCauTruckingNo == h.Truck_YeucauTruckingNo);
            var lenh = await _context.LenhDieuXe.FirstOrDefaultAsync(x => x.Lenhdieuxeno == h.Truck_LenhDieuXeNo);

            item.Pickup = yctruck?.DiaDiemNhanHang;
            item.Dropoff = yctruck?.DiaDiemTraHang;
            item.TruckNo = lenh?.Soxe;

            var debits = await _context.Debit.Where(d => d.hblid == h.hblID).ToListAsync();
            var credits = await _context.Credit.Where(c => c.hblid == h.hblID).ToListAsync();
            var allCharges = await _context.Charge.ToListAsync();

            string Norm(string? s) => string.IsNullOrWhiteSpace(s) ? "" : s.ToLower();
            bool IsMatch(ChargeModel c, string key) => Norm(c.CHARGE).Contains(key);

            Func<M_Debit, bool> IsPhiNang = d => allCharges.Any(c => c.CHARGE_ID == d.itemid && IsMatch(c, "nâng"));
            Func<M_Debit, bool> IsPhiHa = d => allCharges.Any(c => c.CHARGE_ID == d.itemid && IsMatch(c, "hạ"));
            Func<M_Debit, bool> IsCuocVCNoiDia = d => allCharges.Any(c => c.CHARGE_ID == d.itemid && (IsMatch(c, "trucking") || IsMatch(c, "vận chuyển") || IsMatch(c, "van chuyen")));
            Func<M_Debit, bool> IsLocalCharge = d => allCharges.Any(c => c.CHARGE_ID == d.itemid && (IsMatch(c, "local") || IsMatch(c, "handling")));
            Func<M_Debit, bool> IsCuocVCQuocTe = d => allCharges.Any(c => c.CHARGE_ID == d.itemid && (IsMatch(c, "ocean") || IsMatch(c, "freight") || IsMatch(c, "sea freight")));
            Func<M_Debit, bool> IsPhiKhac = d => !IsPhiNang(d) && !IsPhiHa(d) && !IsCuocVCNoiDia(d) && !IsLocalCharge(d) && !IsCuocVCQuocTe(d);

            double? Convert(double? amount = 0, string? loaitien = "USD", double? tigia = 0) =>
                loaitien != null && loaitien.Equals("USD", StringComparison.OrdinalIgnoreCase)
                    ? amount * tigia
                    : amount;

            item.PhiNang = debits.Where(d => d.thuho == true && IsPhiNang(d)).Sum(d => Convert(d.dongia * d.soluong, d.tiente, d.tigiadebit))
                             + credits.Where(c => c.chiho == true && allCharges.Any(ch => ch.CHARGE_ID == c.itemid && Norm(ch.CHARGE).Contains("nâng")))
                                      .Sum(c => Convert(c.dongia * c.soluong, c.tiente, c.tigiacredit));

            item.PhiHa = debits.Where(d => d.thuho == true && IsPhiHa(d)).Sum(d => Convert(d.dongia * d.soluong, d.tiente, d.tigiadebit))
                            + credits.Where(c => c.chiho == true && allCharges.Any(ch => ch.CHARGE_ID == c.itemid && Norm(ch.CHARGE).Contains("hạ")))
                                     .Sum(c => Convert(c.dongia * c.soluong, c.tiente, c.tigiacredit));

            item.CuocVCNoiDia = debits.Where(d => IsCuocVCNoiDia(d)).Sum(d => Convert(d.dongia * d.soluong, d.tiente, d.tigiadebit));
            item.LocalCharge = debits.Where(d => IsLocalCharge(d)).Sum(d => Convert(d.dongia * d.soluong, d.tiente, d.tigiadebit));
            item.CuocVCQuocTe = debits.Where(d => IsCuocVCQuocTe(d)).Sum(d => Convert(d.dongia * d.soluong, d.tiente, d.tigiadebit));

            item.PhiKhac = debits.Where(d => d.thuho == true && IsPhiKhac(d)).Sum(d => Convert(d.dongia * d.soluong, d.tiente, d.tigiadebit))
                             + credits.Where(c => c.chiho == true && allCharges.Any(ch => ch.CHARGE_ID == c.itemid
                             && !Norm(ch.CHARGE).Contains("nâng") && !Norm(ch.CHARGE).Contains("hạ") && !Norm(ch.CHARGE).Contains("trucking")
                             && !Norm(ch.CHARGE).Contains("vận chuyển") && !Norm(ch.CHARGE).Contains("van chuyen")
                             && !Norm(ch.CHARGE).Contains("local") && !Norm(ch.CHARGE).Contains("handling")
                             && !Norm(ch.CHARGE).Contains("ocean") && !Norm(ch.CHARGE).Contains("freight") && !Norm(ch.CHARGE).Contains("sea freight")))
                                      .Sum(c => Convert(c.dongia * c.soluong, c.tiente, c.tigiacredit));

            item.VAT8 = debits.Where(d => d.thue.HasValue && Math.Abs(d.thue.Value - 8) < 0.01)
                             .Sum(d => Convert(d.thanhtiensauthue - (d.dongia * d.soluong), d.tiente, d.tigiadebit));

            item.ThanhTienTruocThue = debits.Sum(d => Convert(d.dongia * d.soluong, d.tiente, d.tigiadebit)) ?? 0;

            item.InvoiceNos = string.Join(", ", debits.Where(d => !string.IsNullOrEmpty(d.sohoadondaura)).Select(d => d.sohoadondaura).Distinct());
            item.Remarks = string.Join(" | ", debits.Where(d => !string.IsNullOrEmpty(d.ghichu)).Select(d => d.ghichu).Distinct());

            return item;
        }
        public async Task<BoolandMessReponse> BangKeTruckingReportExcel(List<M_HBL> hbls, DateRange dateRange)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var templatePath = new FileInfo(Path.Combine("wwwroot", "Reports", "BANGKETRUCKINGSample.xlsx"));
                if (!templatePath.Exists)
                    return new BoolandMessReponse(false, "Template BANGKETRUCKINGSample.xlsx not found");

                using var package = new ExcelPackage(templatePath);
                var ws = package.Workbook.Worksheets[0];

                // Lấy danh sách charge đầy đủ để phân loại theo tên
                var allCharges = _context.Charge.ToList();
                // Chuẩn hóa hàm so khớp tên
                string Norm(string? s) => string.IsNullOrWhiteSpace(s) ? string.Empty : s.ToLower();

                bool IsMatch(ChargeModel c, string key) => Norm(c.CHARGE).Contains(key);

                // Key xác định nhóm phí (có thể tinh chỉnh sau)
                Func<M_Debit, bool> IsPhiNang = d => allCharges.Any(c => c.CHARGE_ID == d.itemid && IsMatch(c, "nâng"));
                Func<M_Debit, bool> IsPhiHa = d => allCharges.Any(c => c.CHARGE_ID == d.itemid && IsMatch(c, "hạ"));
                Func<M_Debit, bool> IsCuocVCNoiDia = d => allCharges.Any(c => c.CHARGE_ID == d.itemid && (IsMatch(c, "trucking") || IsMatch(c, "vận chuyển") || IsMatch(c, "van chuyen")));
                Func<M_Debit, bool> IsLocalCharge = d => allCharges.Any(c => c.CHARGE_ID == d.itemid && (IsMatch(c, "local") || IsMatch(c, "handling")));
                Func<M_Debit, bool> IsCuocVCQuocTe = d => allCharges.Any(c => c.CHARGE_ID == d.itemid && (IsMatch(c, "ocean") || IsMatch(c, "freight") || IsMatch(c, "sea freight")));
                Func<M_Debit, bool> IsPhiKhac = d => !IsPhiNang(d) && !IsPhiHa(d) && !IsCuocVCNoiDia(d) && !IsLocalCharge(d) && !IsCuocVCQuocTe(d);

                // Convert tiền về VND nếu cần (theo logic hiện có: USD * tigia, VND giữ nguyên)
                double? Convert(double? amount = 0, string? loaitien = "USD", double? tigia = 0) =>
                    loaitien != null && loaitien.Equals("USD", StringComparison.OrdinalIgnoreCase)
                        ? amount * tigia
                        : amount;

                int startRow = 11; // dòng bắt đầu (theo yêu cầu)
                int row = startRow;
                if (hbls != null && hbls.Count > 1)
                    ws.InsertRow(startRow + 1, hbls.Count - 1, startRow);

                int stt = 1;
                foreach (var h in (hbls ?? new()).Where(x => !string.IsNullOrEmpty(x.Truck_YeucauTruckingNo)))
                {
                    var yctruck = _context.YeuCauTrucking.FirstOrDefault(x => x.YeuCauTruckingNo == h.Truck_YeucauTruckingNo);
                    var lenh = _context.LenhDieuXe.FirstOrDefault(x => x.Lenhdieuxeno == h.Truck_LenhDieuXeNo);
                    var debits = _context.Debit.Where(d => d.hblid == h.hblID).ToList();
                    var credits = _context.Credit.Where(c => c.hblid == h.hblID).ToList();

                    // Gộp debit & credit để phân nhóm (thu hộ / chi hộ) chỉ lấy những dòng có cờ thuho/chiho đối với nhóm nâng, hạ, chi phí khác
                    var phiNang = debits.Where(d => d.thuho == true && IsPhiNang(d)).Sum(d => Convert(d.dongia * d.soluong, d.tiente, d.tigiadebit))
                                  + credits.Where(c => c.chiho == true && allCharges.Any(ch => ch.CHARGE_ID == c.itemid && Norm(ch.CHARGE).Contains("nâng")))
                                           .Sum(c => Convert(c.dongia * c.soluong, c.tiente, c.tigiacredit));
                    var phiHa = debits.Where(d => d.thuho == true && IsPhiHa(d)).Sum(d => Convert(d.dongia * d.soluong, d.tiente, d.tigiadebit))
                                 + credits.Where(c => c.chiho == true && allCharges.Any(ch => ch.CHARGE_ID == c.itemid && Norm(ch.CHARGE).Contains("hạ")))
                                          .Sum(c => Convert(c.dongia * c.soluong, c.tiente, c.tigiacredit));

                    var cuocVCNoiDia = debits.Where(d => IsCuocVCNoiDia(d)).Sum(d => Convert(d.dongia * d.soluong, d.tiente, d.tigiadebit));
                    var localCharge = debits.Where(d => IsLocalCharge(d)).Sum(d => Convert(d.dongia * d.soluong, d.tiente, d.tigiadebit));
                    var cuocVCQuocTe = debits.Where(d => IsCuocVCQuocTe(d)).Sum(d => Convert(d.dongia * d.soluong, d.tiente, d.tigiadebit));
                    var phiKhac = debits.Where(d => d.thuho == true && IsPhiKhac(d)).Sum(d => Convert(d.dongia * d.soluong, d.tiente, d.tigiadebit))
                                   + credits.Where(c => c.chiho == true && allCharges.Any(ch => ch.CHARGE_ID == c.itemid && !Norm(ch.CHARGE).Contains("nâng") && !Norm(ch.CHARGE).Contains("hạ") && !Norm(ch.CHARGE).Contains("trucking") && !Norm(ch.CHARGE).Contains("vận chuyển") && !Norm(ch.CHARGE).Contains("van chuyen") && !Norm(ch.CHARGE).Contains("local") && !Norm(ch.CHARGE).Contains("handling") && !Norm(ch.CHARGE).Contains("ocean") && !Norm(ch.CHARGE).Contains("freight") && !Norm(ch.CHARGE).Contains("sea freight")))
                                            .Sum(c => Convert(c.dongia * c.soluong, c.tiente, c.tigiacredit));

                    var vat8 = debits.Where(d => d.thue.HasValue && Math.Abs(d.thue.Value - 8) < 0.01)
                                      .Sum(d => Convert(d.thanhtiensauthue - (d.dongia * d.soluong), d.tiente, d.tigiadebit));
                    double? thanhTienTruocThue = debits.Sum(d => Convert(d.dongia * d.soluong, d.tiente, d.tigiadebit)) ?? 0;
                    var invoiceNos = string.Join(", ", debits.Where(d => !string.IsNullOrEmpty(d.sohoadondaura)).Select(d => d.sohoadondaura).Distinct());
                    var remarks = string.Join(" | ", debits.Where(d => !string.IsNullOrEmpty(d.ghichu)).Select(d => d.ghichu).Distinct());

                    int col = 2; // cột B
                    ws.Cells[row, col++].Value = stt++; // STT
                    ws.Cells[row, col++].Value = h.datereport?.ToString("dd/MM/yyyy"); // NGÀY (datereport)
                    ws.Cells[row, col++].Value = h.hbl; // BILL/TỜ KHAI (theo yêu cầu chỉ HBL)
                    ws.Cells[row, col++].Value = yctruck?.DiaDiemNhanHang; // NƠI ĐI
                    ws.Cells[row, col++].Value = yctruck?.DiaDiemTraHang; // NƠI ĐẾN
                    ws.Cells[row, col++].Value = h.NoOfPackages; // SỐ LƯỢNG (KIỆN/CONT)
                    ws.Cells[row, col++].Value = lenh?.Soxe; // BIỂN SỐ XE
                    ws.Cells[row, col++].Value = ""; // TTHQ (tạm bỏ trống)
                    ws.Cells[row, col++].Value = cuocVCNoiDia; // CƯỚC V/CHUYỂN (nội địa)
                    ws.Cells[row, col++].Value = phiNang; // PHÍ NÂNG
                    ws.Cells[row, col++].Value = phiHa;  // PHÍ HẠ
                    ws.Cells[row, col++].Value = localCharge; // LOCAL CHARGE
                    ws.Cells[row, col++].Value = cuocVCQuocTe; // CƯỚC V/C QUỐC TẾ
                    ws.Cells[row, col++].Value = phiKhac; // CHI PHÍ KHÁC
                    ws.Cells[row, col++].Value = vat8; // VAT 8%
                    ws.Cells[row, col++].Value = thanhTienTruocThue; // THÀNH TIỀN (trước thuế)
                    ws.Cells[row, col++].Value = invoiceNos; // INVOICE NO
                    ws.Cells[row, col++].Value = remarks; // REMARK

                    row++;
                }

                // Lưu file
                using var memoryStream = new MemoryStream();
                await package.SaveAsAsync(memoryStream);
                var excelData = memoryStream.ToArray();
                var fileName = $"BANG_KE_TRUCKING_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                await JSRuntime.InvokeVoidAsync("saveAsFile", fileName, System.Convert.ToBase64String(excelData));
                return new BoolandMessReponse(true, "Export successfully");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, $"Export failed: {ex.Message}");
            }
        }
        private static double? ConvertShippingAmount(double? amount = 0, string? loaitien = "USD", double? tigia = 0) =>
            string.Equals(loaitien, "USD", StringComparison.OrdinalIgnoreCase)
                ? amount * tigia
                : amount;

        private async Task<ShippingStatisticGridItem> BuildShippingStatisticGridItem(M_HBL hbl, List<M_HoaDonDauRa> allHoaDonDauRa)
        {
            Guid id_cuocvanchuyen = Guid.Parse("4BAD5BEA-4111-4EEA-8C7D-FB487736CF29");
            var idphinang = Guid.Parse("A74FCB54-EDF8-4E58-B0A2-0CDD011EDA6F");
            var idphiha = Guid.Parse("B7DBA4DF-1889-456A-BA83-46370D3FFA32");
            List<Guid> idbocxep = new List<Guid> { Guid.Parse("341A1F85-C6AA-4F12-9A07-96726414E6EB"), Guid.Parse("38716C3B-F260-49EB-B032-A47F6CF2CC30") };
            var idphican = Guid.Parse("D66E656E-5CAB-498F-B0C9-BA953370BE57");

            var conts = await _context.Container.Where(c => c.hblid == hbl.hblID).ToListAsync();
            var debts = await _context.Debit.Where(d => d.hblid == hbl.hblID).ToListAsync();
            var credts = await _context.Credit.Where(c => c.hblid == hbl.hblID).ToListAsync();
            var cuocvanchuyen = debts.Where(x => x.itemid == id_cuocvanchuyen).ToList();
            var yctruck = await _context.YeuCauTrucking.FirstOrDefaultAsync(x => x.YeuCauTruckingNo == hbl.Truck_YeucauTruckingNo) ?? new();
            var cus = await _context.Customer.FirstOrDefaultAsync(x => x.Customer_ID == hbl.CustomerID);

            var totalcuocvc = cuocvanchuyen.Sum(x => ConvertShippingAmount(x.thanhtien, x.tiente, x.tigiadebit));
            var phinang = credts.Where(x => x.itemid == idphinang && x.chiho == true).Sum(x => ConvertShippingAmount(x.thanhtiensauthue, x.tiente, x.tigiacredit));
            var phiha = credts.Where(x => x.itemid == idphiha && x.chiho == true).Sum(x => ConvertShippingAmount(x.thanhtiensauthue, x.tiente, x.tigiacredit));
            var sohdnang = allHoaDonDauRa.Where(x => debts.Where(d => d.itemid == idphinang && d.thuho == true).Select(h => h.sohoadondaura).Contains(x.sohoadonNoibo)).Select(x => x.sohoadonNoibo);
            var sohdha = allHoaDonDauRa.Where(x => debts.Where(d => d.itemid == idphinang && d.thuho == true).Select(h => h.sohoadondaura).Contains(x.sohoadonNoibo)).Select(x => x.sohoadonNoibo);
            var bocxep = credts.Where(x => idbocxep.Contains(x.itemid) && x.chiho == true).Sum(x => ConvertShippingAmount(x.thanhtiensauthue, x.tiente, x.tigiacredit));
            var phikhac = credts
                .Where(x => x.itemid != idphinang
                            && x.itemid != idphiha
                            && !idbocxep.Contains(x.itemid)
                            && x.chiho == true)
                .Sum(x => ConvertShippingAmount(x.thanhtiensauthue, x.tiente, x.tigiacredit));
            var phican = credts.Where(x => x.itemid == idphican && x.chiho == true).Sum(x => ConvertShippingAmount(x.thanhtiensauthue, x.tiente, x.tigiacredit));
            var tongTienHoaDon = cuocvanchuyen.Sum(x => ConvertShippingAmount(x.thanhtiensauthue, x.tiente, x.tigiadebit));
            var vat = tongTienHoaDon - totalcuocvc;

            return new ShippingStatisticGridItem
            {
                HblID = hbl.hblID,
                CustomerName = cus?.COMPANY ?? "",
                SoHD = "",
                NgayDukienLayHang = yctruck.NgayDukienLayHang,
                LoaiCont = yctruck.LoaiCont,
                XuatNhap = "",
                ContainerNos = string.Join(", ", conts.Select(x => x.CONTAINER_NO)),
                HBL = hbl.hbl ?? "",
                DiaDiemNhanHang = yctruck.DiaDiemNhanHang,
                ContainerLocationEmpty = yctruck.ContainerLocation_empty,
                DiaDiemTraHang = yctruck.DiaDiemTraHang,
                CuocVanChuyen = totalcuocvc,
                PhuThu = null,
                VAT = vat,
                TongTienHoaDon = tongTienHoaDon,
                PhiNang = phinang,
                SoHDNang = string.Join(", ", sohdnang),
                PhiHa = phiha,
                SoHDHa = string.Join(", ", sohdha),
                PhiCan = phican,
                BocXep = bocxep,
                PhiKhac = phikhac,
                TongPhi = phinang + phiha + bocxep + phican + phikhac
            };
        }

        public async Task<List<ShippingStatisticGridItem>> GetShippingStatisticGridItems(List<M_HBL> hbls)
        {
            var allHoaDonDauRa = await _context.HoaDonDauRa.ToListAsync();
            var items = new List<ShippingStatisticGridItem>();
            for (var i = 0; i < hbls.Count; i++)
            {
                var item = await BuildShippingStatisticGridItem(hbls[i], allHoaDonDauRa);
                item.STT = i + 1;
                items.Add(item);
            }
            return items;
        }

        public async Task<BoolandMessReponse> BangKeSanLuongVanChuyenExcel(List<M_HBL> hbls, DateRange dateRange)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                var templatePath = new FileInfo(Path.Combine("wwwroot", "Reports/ShippingStatisticReportSample.xlsx"));
                using var package = new ExcelPackage(templatePath);

                _context.ChangeTracker.Clear();
                var allHoaDonDauRa = _context.HoaDonDauRa.ToList();

                // 1. Lấy sheet template và đặt tên tạm
                var templateSheet = package.Workbook.Worksheets["Template"];
                if (templateSheet == null)
                    throw new InvalidOperationException("Không tìm thấy sheet 'Template' trong file mẫu.");

                // 2. Group theo CustomerID
                var groups = hbls.GroupBy(x => x.CustomerID);
                foreach (var grp in groups)
                {
                    var cusdetail = _context.Customer.FirstOrDefault(x => x.Customer_ID == grp.Key) ?? new();
                    // 3. Copy template thành sheet mới
                    var newSheetName = $"KH_{cusdetail.shortname}";
                    var worksheet = package.Workbook.Worksheets.Copy(templateSheet.Name, newSheetName);
                    int startRow = 13;
                    var listHbl = grp.ToList();
                    // chèn thêm dòng (nếu > 1 bản ghi)
                    if (listHbl.Count > 1)
                        worksheet.InsertRow(startRow + 1, listHbl.Count - 1, startRow);
                    int row = startRow;
                    foreach (var hbl in listHbl)
                    {
                        var idx = hbls.IndexOf(hbl);
                        var item = await BuildShippingStatisticGridItem(hbl, allHoaDonDauRa);
                        var i = 1;
                        worksheet.Cells[row, i++].Value = idx + 1;
                        worksheet.Cells[row, i++].Value = item.SoHD;
                        worksheet.Cells[row, i++].Value = item.NgayDukienLayHang;
                        worksheet.Cells[row, i++].Value = item.LoaiCont;
                        worksheet.Cells[row, i++].Value = item.XuatNhap;
                        worksheet.Cells[row, i++].Value = item.ContainerNos;
                        worksheet.Cells[row, i++].Value = item.HBL;
                        worksheet.Cells[row, i++].Value = item.DiaDiemNhanHang;
                        worksheet.Cells[row, i++].Value = item.ContainerLocationEmpty;
                        worksheet.Cells[row, i++].Value = item.DiaDiemTraHang;
                        worksheet.Cells[row, i++].Value = item.CuocVanChuyen;
                        worksheet.Cells[row, i++].Value = item.PhuThu;
                        worksheet.Cells[row, i++].Value = item.VAT;
                        worksheet.Cells[row, i++].Value = item.TongTienHoaDon;
                        worksheet.Cells[row, i++].Value = item.PhiNang;
                        worksheet.Cells[row, i++].Value = item.SoHDNang;
                        worksheet.Cells[row, i++].Value = item.PhiHa;
                        worksheet.Cells[row, i++].Value = item.SoHDHa;
                        worksheet.Cells[row, i++].Value = item.PhiCan;
                        worksheet.Cells[row, i++].Value = item.BocXep;
                        worksheet.Cells[row, i++].Value = item.PhiKhac;
                        worksheet.Cells[row, i++].Value = item.TongPhi;
                        row++;
                    }
                    var endMonthSanLuong = dateRange.End ?? DateTime.Today;
                    worksheet.Cells["A4"].Value = $"BẢNG KÊ SẢN LƯỢNG VẬN CHUYỂN THÁNG {endMonthSanLuong:MM.yyyy}";
                    worksheet.Cells["A5"].Value = $"Kính gửi: {cusdetail.EnglishName}";
                    worksheet.Cells["A6"].Value = $"Địa chỉ: {cusdetail.addresstiengviet}";
                    worksheet.Cells["A7"].Value = $"MST: {cusdetail.TaxCode}";
                }

                package.Workbook.Worksheets.Delete(templateSheet);
                // Lưu vào memory stream
                using var memoryStream = new MemoryStream();
                await package.SaveAsAsync(memoryStream);
                var excelData = memoryStream.ToArray();

                // Tên file xuất ra
                var endDateCongNo = dateRange.End ?? DateTime.Today;
                var fileName = $"BẢNG TỔNG HỢP CÔNG NỢ PHẢI THU ĐẾN NGÀY {endDateCongNo:dd.MM.yyyy}.xlsx";

                // Gọi JS tải xuống
                await JSRuntime.InvokeVoidAsync("saveAsFile", fileName, System.Convert.ToBase64String(excelData));

                return new BoolandMessReponse(true, "Export successfully");
            }
            catch (Exception)
            {
                return new BoolandMessReponse(false, "Export failed");
            }
        }

        public async Task<BoolandMessReponse> BangKeChiPhiVanChuyenExcel(List<M_HBL> hbls, DateRange dateRange)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                var templatePath = new FileInfo(Path.Combine("wwwroot", "Reports/TotalCostReportSample.xlsx"));
                using var package = new ExcelPackage(templatePath);

                // Null-safe currency conversion
                double? Convert(double? amount = 0, string? loaitien = "USD", double? tigia = 0) =>
                    string.Equals(loaitien, "USD", StringComparison.OrdinalIgnoreCase)
                        ? amount * tigia
                        : amount;

                //IDs
                Guid COMCarrier = Guid.Parse("47F2EA15-2BF8-4D4C-B3FC-7DC170016F20");
                Guid COMClient = Guid.Parse("C25047D1-4530-49D8-8926-DA6D0E270F89");
                var idphinang = Guid.Parse("A74FCB54-EDF8-4E58-B0A2-0CDD011EDA6F");// phi nang
                var idphiha = Guid.Parse("B7DBA4DF-1889-456A-BA83-46370D3FFA32");// phi ha
                List<Guid> idbocxep = new List<Guid> { Guid.Parse("341A1F85-C6AA-4F12-9A07-96726414E6EB"), Guid.Parse("38716C3B-F260-49EB-B032-A47F6CF2CC30") };
                var idphicauduong = Guid.Parse("4075B243-364D-4EC5-8FFC-901EA09FECCE");// phi cau duong
                var idphican = Guid.Parse("D66E656E-5CAB-498F-B0C9-BA953370BE57");// phi can
                var idphivesinh = Guid.Parse("517FC218-AEB5-4CC9-B4AA-17B1E7103B0F"); // phi vs
                var idphisuachua = Guid.Parse("D6FB8A86-65D6-40CF-B136-68A85282F0E2"); // phi sua chua
                var idphixangdau = Guid.Parse("AA1CF3F2-056B-4BD8-BE6A-BBF97E183178"); // phi xang dau
                var excludeIds = new List<Guid> { idphinang, idphiha, idphicauduong, idphican, idphivesinh, idphisuachua, idphixangdau };
                excludeIds.AddRange(idbocxep);

                //HOA DON
                _context.ChangeTracker.Clear();
                var allHoaDonDauRa = _context.HoaDonDauRa.ToList();
                var allHoaDonDauVao = _context.HoaDonDauVao.ToList();

                // 1. Lấy sheet template và đặt tên tạm
                var templateSheet = package.Workbook.Worksheets["Template"];
                if (templateSheet == null)
                    throw new InvalidOperationException("Không tìm thấy sheet 'Template' trong file mẫu.");

                // 2. Group theo CustomerID
                var groups = hbls.GroupBy(x => x.Truck_LenhDieuXeNo);
                foreach (var grp in groups)
                {
                    var xedetail = _context.LenhDieuXe.FirstOrDefault(x => x.Lenhdieuxeno == grp.Key) ?? new();
                    // 3. Copy template thành sheet mới
                    var newSheetName = $"Xe {xedetail.Soxe}";
                    var worksheet = package.Workbook.Worksheets.Copy(templateSheet.Name, newSheetName);
                    int startRow = 11;
                    var listHbl = grp.ToList();
                    // chèn thêm dòng (nếu > 1 bản ghi)
                    if (listHbl.Count > 1)
                        worksheet.InsertRow(startRow + 1, listHbl.Count - 1, startRow);
                    int row = startRow; // Ghi bắt đầu từ dòng 7
                    foreach (var hbl in listHbl)
                    {
                        var idx = hbls.IndexOf(hbl);
                        var type = await shipsv.GetTypeByMBLID(hbl!.mblid); // Sea Air Truck HQ & I or E
                        var conts = _context.Container.Where(h => h.hblid == hbl.hblID).ToList();
                        var debts = _context.Debit.Where(h => h.hblid == hbl.hblID).ToList();
                        var credts = _context.Credit.Where(h => h.hblid == hbl.hblID).ToList();
                        var income = debts.Where(x => x.itemid != COMCarrier && x.itemid != COMClient);
                        var cost = credts.Where(x => x.itemid != COMCarrier && x.itemid != COMClient);
                        var agent = _context.Customer.FirstOrDefault(x => x.Customer_ID == hbl.AgentID);
                        var cus = _context.Customer.FirstOrDefault(x => x.Customer_ID == hbl.CustomerID);
                        var yctruck = _context.YeuCauTrucking.FirstOrDefault(x => x.YeuCauTruckingNo == hbl.Truck_YeucauTruckingNo) ?? new();
                        var totalincome = income.Sum(x => Convert(x.thanhtien, x.tiente, x.tigiadebit));
                        var totalcost = cost.Sum(x => Convert(x.thanhtien, x.tiente, x.tigiacredit));
                        var phinang = credts.Where(x => x.itemid == idphinang && x.chiho == true).Sum(x => Convert(x.thanhtien, x.tiente, x.tigiacredit));
                        var phiha = credts.Where(x => x.itemid == idphiha && x.chiho == true).Sum(x => Convert(x.thanhtien, x.tiente, x.tigiacredit));
                        phinang += debts.Where(x => x.itemid == idphinang && x.thuho == true).Sum(x => Convert(x.thanhtien, x.tiente, x.tigiadebit));
                        phiha += debts.Where(x => x.itemid == idphiha && x.thuho == true).Sum(x => Convert(x.thanhtien, x.tiente, x.tigiadebit));
                        var sohdnang = allHoaDonDauRa.Where(x => debts.Where(x => x.itemid == idphinang).Select(h => h.sohoadondaura).Contains(x.sohoadonNoibo)).Select(x => x.sohoadonNoibo);
                        var sohdha = allHoaDonDauRa.Where(x => debts.Where(x => x.itemid == idphinang).Select(h => h.sohoadondaura).Contains(x.sohoadonNoibo)).Select(x => x.sohoadonNoibo);
                        //var bocxep = debts.Where(x => idbocxep.Contains(x.itemid) && x.thuho == true).Sum(x => Convert(x.thanhtien, x.tiente, x.tigiadebit));
                        var bocxep = credts.Where(x => idbocxep.Contains(x.itemid) && x.chiho == true).Sum(x => Convert(x.thanhtiensauthue, x.tiente, x.tigiacredit));
                        //var cauduong = debts.Where(x => x.itemid == idphicauduong && x.thuho == true).Sum(x => Convert(x.thanhtien, x.tiente, x.tigiadebit));
                        var cauduong = credts.Where(x => x.itemid == idphicauduong && x.chiho == true).Sum(x => Convert(x.thanhtiensauthue, x.tiente, x.tigiacredit));
                        //var phican = debts.Where(x => x.itemid == idphican && x.thuho == true).Sum(x => Convert(x.thanhtien, x.tiente, x.tigiadebit));
                        var phican = credts.Where(x => x.itemid == idphican && x.chiho == true).Sum(x => Convert(x.thanhtiensauthue, x.tiente, x.tigiacredit));
                        //var phivesinh = debts.Where(x => x.itemid == idphivesinh && x.thuho == true).Sum(x => Convert(x.thanhtien, x.tiente, x.tigiadebit));
                        var phivesinh = credts.Where(x => x.itemid == idphivesinh && x.chiho == true).Sum(x => Convert(x.thanhtiensauthue, x.tiente, x.tigiacredit));
                        //var phisuachua = debts.Where(x => x.itemid == idphisuachua && x.thuho == true).Sum(x => Convert(x.thanhtien, x.tiente, x.tigiadebit));
                        var phisuachua = credts.Where(x => x.itemid == idphisuachua && x.chiho == true).Sum(x => Convert(x.thanhtiensauthue, x.tiente, x.tigiacredit));
                        var sohdsuachua = allHoaDonDauRa.Where(x => debts.Where(x => x.itemid == idphisuachua).Select(h => h.sohoadondaura).Contains(x.sohoadonNoibo)).Select(x => x.sohoadonNoibo);
                        //var chiphikhac = debts.Where(x => !excludeIds.Contains(x.itemid) && x.thuho == true).Sum(x => Convert(x.thanhtien, x.tiente, x.tigiadebit));
                        var chiphikhac = credts.Where(x => !excludeIds.Contains(x.itemid) && x.chiho == true).Sum(x => Convert(x.thanhtiensauthue, x.tiente, x.tigiacredit));
                        //var phixangdau = debts.Where(x => x.itemid == idphixangdau && x.thuho == true).Sum(x => Convert(x.thanhtien, x.tiente, x.tigiadebit));
                        var phixangdau = credts.Where(x => x.itemid == idphixangdau && x.chiho == true).Sum(x => Convert(x.thanhtiensauthue, x.tiente, x.tigiacredit));
                        var sohdxangdau = allHoaDonDauRa.Where(x => debts.Where(x => x.itemid == idphixangdau).Select(h => h.sohoadondaura).Contains(x.sohoadonNoibo)).Select(x => x.sohoadonNoibo);
                        var i = 1;
                        worksheet.Cells[row, i++].Value = idx + 1;
                        worksheet.Cells[row, i++].Value = yctruck.NgayDukienLayHang;
                        worksheet.Cells[row, i++].Value = yctruck.LoaiCont;
                        worksheet.Cells[row, i++].Value = ""; // xuất nhập
                        worksheet.Cells[row, i++].Value = string.Join(", ", conts.Select(x => x.CONTAINER_NO).ToList());
                        worksheet.Cells[row, i++].Value = hbl.hbl;
                        worksheet.Cells[row, i++].Value = yctruck.ContainerLocation_empty;
                        worksheet.Cells[row, i++].Value = yctruck.DiaDiemNhanHang;
                        worksheet.Cells[row, i++].Value = yctruck.DiaDiemTraHang;
                        worksheet.Cells[row, i++].Value = bocxep;
                        worksheet.Cells[row, i++].Value = cauduong;
                        worksheet.Cells[row, i++].Value = phican;
                        worksheet.Cells[row, i++].Value = phivesinh;
                        worksheet.Cells[row, i++].Value = phisuachua;
                        worksheet.Cells[row, i++].Value = string.Join(", ", sohdsuachua);
                        worksheet.Cells[row, i++].Value = phinang;
                        worksheet.Cells[row, i++].Value = string.Join(", ", sohdnang);
                        worksheet.Cells[row, i++].Value = phiha;
                        worksheet.Cells[row, i++].Value = string.Join(", ", sohdha);
                        worksheet.Cells[row, i++].Value = chiphikhac; // các phi khác total ở đây
                        worksheet.Cells[row, i++].Value = phixangdau;
                        worksheet.Cells[row, i++].Value = string.Join(", ", sohdxangdau);
                        worksheet.Cells[row, i++].Value = bocxep + cauduong + phican + phivesinh + phisuachua + phinang + phiha + chiphikhac + phixangdau;
                        worksheet.Cells[row, i++].Value = xedetail.GiaCost;
                        worksheet.Cells[row, i++].Value = "";
                        worksheet.Cells[row, i++].Value = "";
                        worksheet.Cells[row, i++].Value = xedetail.GiaCost;
                        worksheet.Cells[row, i++].Value = xedetail.Tamung.HasValue ? xedetail.Ngaylap : "";
                        worksheet.Cells[row, i++].Value = xedetail.Tamung;
                        worksheet.Cells[row, i++].Value = "";
                        worksheet.Cells[row, i++].Value = "";
                        worksheet.Cells[row, i++].Value = "";
                        worksheet.Cells[row, i++].Value = cus?.shortname;
                        worksheet.Cells[row, i++].Value = "";
                        row++;
                    }
                    var endMonthChiPhi = dateRange.End ?? DateTime.Today;
                    worksheet.Cells["A4"].Value = $"BẢNG TỔNG HỢP CHI PHÍ THÁNG {endMonthChiPhi:MM.yyyy}";
                }

                package.Workbook.Worksheets.Delete(templateSheet);
                // Lưu vào memory stream
                using var memoryStream = new MemoryStream();
                await package.SaveAsAsync(memoryStream);
                var excelData = memoryStream.ToArray();

                // Tên file xuất ra
                var endDateChiPhi = dateRange.End ?? DateTime.Today;
                var fileName = $"BẢNG TỔNG HỢP CHI PHÍ ĐẾN NGÀY {endDateChiPhi:dd.MM.yyyy}.xlsx";

                // Gọi JS tải xuống
                await JSRuntime.InvokeVoidAsync("saveAsFile", fileName, System.Convert.ToBase64String(excelData));

                return new BoolandMessReponse(true, "Export successfully");
            }
            catch (Exception)
            {
                return new BoolandMessReponse(false, "Export failed");
            }
        }
        public async Task<BoolandMessReponse> ImportToExcel(List<M_Job> jobs, DateRange dateRange)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                var templatePath = new FileInfo(Path.Combine("wwwroot", "Reports/FormReportShipment.xlsx"));
                using var package = new ExcelPackage(templatePath);
                var worksheet = package.Workbook.Worksheets[0];
                _context.ChangeTracker.Clear();
                var allmbls = _context.MBL.ToList();
                var allhbls = _context.HBL.ToList();
                worksheet.InsertRow(8, jobs.Count - 1, 7);
                int row = 7; // Ghi bắt đầu từ dòng 7
                foreach (var job in jobs)
                {
                    var idx = jobs.IndexOf(job);
                    var mbls = allmbls.Where(x => x.Jobid == job.JobID).ToList();
                    var firstmbl = mbls.FirstOrDefault() ?? new();
                    var hbls = allhbls.Where(h => mbls.Select(x => x.MblID).Contains(h.mblid)).ToList();
                    hbls.AddRange(allhbls.Where(h => h.Jobid == job.JobID).ToList()); // truck
                    var agent = _context.Customer.FirstOrDefault(x => x.Customer_ID == firstmbl.AgentID);
                    var cus = _context.Customer.FirstOrDefault(x => x.Customer_ID == firstmbl.CustomerID);
                    var conts = _context.Container.ToList();
                    conts = conts.Where(h => hbls.Select(x => (Guid?)x.hblID).Contains(h.hblid)).ToList();
                    var debts = _context.Debit.ToList();
                    debts = debts.Where(h => hbls.Select(x => (Guid?)x.hblID).Contains(h.hblid)).ToList();
                    var credts = _context.Credit.ToList();
                    credts = credts.Where(h => hbls.Select(x => (Guid?)x.hblID).Contains(h.hblid)).ToList();

                    Guid COMCarrier = Guid.Parse("47F2EA15-2BF8-4D4C-B3FC-7DC170016F20");
                    Guid COMClient = Guid.Parse("C25047D1-4530-49D8-8926-DA6D0E270F89");
                    var income = debts.Where(x => x.itemid != COMCarrier && x.itemid != COMClient);
                    var cost = credts.Where(x => x.itemid != COMCarrier && x.itemid != COMClient);
                    // Null-safe currency conversion
                    double? Convert(double? amount = 0, string? loaitien = "USD", double? tigia = 0) =>
                        string.Equals(loaitien, "USD", StringComparison.OrdinalIgnoreCase)
                            ? amount * tigia
                            : amount;
                    worksheet.Cells[row, 1].Value = idx + 1;
                    worksheet.Cells[row, 2].Value = job.Loai == null ? "N/A" : (job.Loai.Contains("I") ? "Inbound" : "Outbound");
                    worksheet.Cells[row, 3].Value = job.FLC == null ? "N/A" : (job.FLC.Contains("S") ? "Sea Freight" : "Air Freight");
                    worksheet.Cells[row, 4].Value = "FCL";
                    worksheet.Cells[row, 5].Value = job.JobNo;
                    worksheet.Cells[row, 6].Value = string.Join(';', hbls.Select(h => h.hbl));
                    worksheet.Cells[row, 7].Value = string.Join(';', mbls.Select(h => h.Mbl));
                    worksheet.Cells[row, 8].Value = firstmbl.Shipper;
                    worksheet.Cells[row, 9].Value = firstmbl.Consignee;
                    worksheet.Cells[row, 11].Value = agent == null ? "" : agent.COMPANY;
                    worksheet.Cells[row, 12].Value = firstmbl.Notify1;
                    worksheet.Cells[row, 15].Value = cus == null ? "" : cus.COMPANY;
                    worksheet.Cells[row, 16].Value = firstmbl.Polcode;
                    worksheet.Cells[row, 17].Value = firstmbl.Podcode;
                    var pol = firstmbl.Polcode ?? string.Empty;
                    var pod = firstmbl.Podcode ?? string.Empty;
                    worksheet.Cells[row, 18].Value = pol.Length == 5 ? pol.Substring(0, 2) : pol;
                    worksheet.Cells[row, 19].Value = pod.Length == 5 ? pod.Substring(0, 2) : pod;
                    worksheet.Cells[row, 20].Value = firstmbl.ETD.HasValue ? firstmbl.ETD.Value.ToString("dd/MM/yyyy") : "";
                    worksheet.Cells[row, 21].Value = firstmbl.ETA.HasValue ? firstmbl.ETA.Value.ToString("dd/MM/yyyy") : "";
                    //worksheet.Cells[row, 22].Value = conts.Sum(x => x.cbm);
                    worksheet.Cells[row, 22].Value = conts
                     .Select(x => decimal.TryParse(x.cbm, out var v) ? v : 0)
                     .Sum();
                    worksheet.Cells[row, 24].Value = conts.Sum(x => x.pkgs);
                    worksheet.Cells[row, 25].Value = hbls.Count;
                    worksheet.Cells[row, 26].Value = conts.Select(x => x.CTN_SIZE_TYPE?.Contains("20")).Count();
                    worksheet.Cells[row, 27].Value = conts.Select(x => x.CTN_SIZE_TYPE?.Contains("40")).Count();
                    worksheet.Cells[row, 28].Value = conts.Select(x => x.CTN_SIZE_TYPE?.Contains("45")).Count();
                    worksheet.Cells[row, 29].Value = conts.Select(x => x.CTN_SIZE_TYPE?.Contains("60")).Count();
                    var totalincome = income.Sum(x => Convert(x.thanhtien, x.tiente, x.tigiadebit));
                    worksheet.Cells[row, 32].Value = totalincome;
                    worksheet.Cells[row, 33].Value = income.Sum(x => Convert(x.thanhtiensauthue, x.tiente, x.tigiadebit)) - totalincome;
                    var totalcost = cost.Sum(x => Convert(x.thanhtien, x.tiente, x.tigiacredit));
                    worksheet.Cells[row, 34].Value = totalcost;
                    worksheet.Cells[row, 35].Value = cost.Sum(x => Convert(x.thanhtiensauthue, x.tiente, x.tigiacredit)) - totalcost;
                    worksheet.Cells[row, 38].Value = credts.Where(x => x.itemid == COMClient).Sum(x => Convert(x.thanhtien, x.tiente, x.tigiacredit)); // com client
                    worksheet.Cells[row, 39].Value = credts.Where(x => x.itemid == COMCarrier).Sum(x => Convert(x.thanhtien, x.tiente, x.tigiacredit));  // com carrier
                    worksheet.Cells[row, 41].Value = job.Datecreate;
                    // 👉 Copy công thức từ dòng 7 sang dòng i
                    for (int col = 18; col <= 41; col++) // Tùy cột bạn muốn copy công thức (R đến AN)
                    {
                        var sourceFormula = worksheet.Cells[7, col].Formula;
                        if (!string.IsNullOrEmpty(sourceFormula))
                        {
                            worksheet.Cells[row, col].Formula = sourceFormula.Replace(7.ToString(), row.ToString());
                        }
                    }
                    row++;
                }
                worksheet.Cells["B2"].Value = $"Posting Date Fm/To, : {(dateRange.Start.HasValue ? dateRange.Start.Value.ToString("d/M") : string.Empty)}-{(dateRange.End.HasValue ? dateRange.End.Value.ToString("d/M") : string.Empty)}";
                //Posting Date Fm/To, : 1/9-30/9
                // Lưu vào memory stream
                using var memoryStream = new MemoryStream();
                await package.SaveAsAsync(memoryStream);
                var excelData = memoryStream.ToArray();

                // Tên file xuất ra
                var fileName = $"Báo cáo cuối tháng {DateTime.Now:yyyyMMddHHmmss}.xlsx";

                // Gọi JS tải xuống
                await JSRuntime.InvokeVoidAsync("saveAsFile", fileName, Convert.ToBase64String(excelData));

                return new BoolandMessReponse(true, "Export successfully");
            }
            catch (Exception)
            {
                return new BoolandMessReponse(false, "Export failed");
            }
        }
    }
}
