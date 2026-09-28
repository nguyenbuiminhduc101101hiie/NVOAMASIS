using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class LeaveRequest
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public Guid EmployeeId { get; set; }
        
        // 1=Annual, 2=Sick, 3=Personal, 4=Unpaid
        public int LeaveType { get; set; }
        
        // 1=FullDay, 2=HalfDay, 3=Hours
        public int DurationType { get; set; }
        
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        
        [MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;
        
        // 1=Pending, 2=Approved, 3=Rejected
        public int Status { get; set; } = 1;
        
        /// <summary>
        /// Người được giao duyệt (quản lý trực tiếp theo HrEmployee.ManagerEmployeeId, lưu UserList.UsrId).
        /// Null = không có quản lý → người có quyền Approve trên "LeaveRequests" duyệt.
        /// Cột thêm bởi Scripts/CreateHrLeaveTimesheetTables.sql.
        /// </summary>
        public Guid? AssignedApproverId { get; set; }

        public Guid? ApproverId { get; set; }
        public DateTime? ApprovedDate { get; set; }
        
        [MaxLength(500)]
        public string? ApproverComment { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }
        
        public decimal TotalDays { get; set; }
        public decimal? TotalHours { get; set; }
    }
}
