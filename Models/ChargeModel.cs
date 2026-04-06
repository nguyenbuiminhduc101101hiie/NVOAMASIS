using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
	public class ChargeModel
	{
		[Key]
		public Guid CHARGE_ID {get;set;}
		public int? stt {get;set;}
		public string? CHARGE_CODE {get;set;}
		public bool? Show {get;set;}
		public bool? show1 {get;set;}
		public string? CHARGE {get;set;}
		public string? DVT {get;set;}
		public string? tariffFCL {get;set;}
		public string? tarifffclib {get;set;}
		public string? tarifflcl {get;set;}
		public string? tarifflclib {get;set;}
		public bool? APPROVE {get;set;}
		public bool? CONTINUED {get;set;}
		public bool? EDITABLE {get;set;}
		public string? USERID {get;set;}
		public DateTime? UPDATETIME {get;set;}
		public string? tiengtrung {get;set;}
		public string? unit {get;set;}
		public bool? other {get;set;}
		public string? tk1 {get;set;}
		public string? tk2 {get;set;}
		public string? tk3 {get;set;}
		public string? loaidichvu_charge {get;set;}
		public string? accselling {get;set;}
		public string? accbuying {get;set;}
	}
}
