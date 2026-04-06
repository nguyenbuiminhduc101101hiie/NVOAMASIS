using Microsoft.EntityFrameworkCore;

namespace NVOAMASIS.Models
{
    [Keyless]
    public class ListDebit
    {
        public string? company {get;set;}
	public string? dept {get;set;}
	public string? item {get;set;}
	public string? currency {get;set;}
	public string? quantity {get;set;}
	public string? containertype {get;set;}
	public double? unitprice_ {get;set;}
	public double? taxprice {get;set;}
	public double? price_ {get;set;}
	public double? unitprice {get;set;}
	public double? price {get;set;}
	public double? thanhtiensauthueVND {get;set;}
	public string? tigia {get;set;}
	public string? note {get;set;}
	public Guid itemid {get;set;}
	public string? no_ {get;set;}
	public Guid inboundid {get;set;}
	public Guid inboundfreightid {get;set;}
	public Guid customerid {get;set;}
	public string? container {get;set;}
	public bool? boss {get;set;}
	public bool? ktt {get;set;}
	public int? freedem {get;set;}
	public int? freedet {get;set;}
	public double? pricetruocthue {get;set;}
	public double? pricenotaxvnd {get;set;}
	public double? pricethue {get;set;}
	public bool? paycheck {get;set;}
	public bool? os {get;set;}
	public string? ngay {get;set;}
	public string? ngayhoadon {get;set;}
	public double? dongiatruocthueVND {get;set;}
	public double? thanhtientruocthueVND {get;set;}
	public double? tienthueVND {get;set;}
	public string? eraseno {get;set;}
	public string? bkno {get;set;}
	public string? userupdate {get;set;}
	public string? dateupdate {get;set;}
	public bool? approve {get;set;}
	public bool? showarrival {get;set;}
	public string? soNgayCongNo {get;set;}
	public bool? daily {get;set;}
	public string? housebill_debitcredit {get;set;}
	public string? ref_debitcredit {get;set;}
	public int? stt {get;set;}
	public bool? showvnd {get;set;}
	public int? f1 {get;set;}
	public int? f2 {get;set;}
	public int? t1 {get;set;}
	public int? t2 {get;set;}
	public double? p1 {get;set;}
	public double? p2 {get;set;}
	public int? level {get;set;}
	public DateTime? freightdatereport {get;set;}
	public string? quyenbaocao {get;set;}
    }
}
