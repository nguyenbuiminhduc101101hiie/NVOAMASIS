using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NVOAMASIS.Migrations
{
    /// <summary>
    /// Inserts SharedResource keys for ChargeType UI (en-US, vi-VN, zh-CN).
    /// </summary>
    public partial class AddChargeTypeLocalizationResources : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                INSERT INTO LocalizationResources (ResourceKey, Culture, Value) VALUES
                (N'ChargeType', N'en-US', N'Charge type'),
                (N'ChargeType', N'vi-VN', N'Loại phí'),
                (N'ChargeType', N'zh-CN', N'费用类型'),
                (N'ChargeType_Code', N'en-US', N'Code'),
                (N'ChargeType_Code', N'vi-VN', N'Mã'),
                (N'ChargeType_Code', N'zh-CN', N'代码'),
                (N'ChargeType_Description', N'en-US', N'Description'),
                (N'ChargeType_Description', N'vi-VN', N'Mô tả'),
                (N'ChargeType_Description', N'zh-CN', N'说明'),
                (N'ChargeType_Active', N'en-US', N'Active'),
                (N'ChargeType_Active', N'vi-VN', N'Kích hoạt'),
                (N'ChargeType_Active', N'zh-CN', N'启用'),
                (N'ChargeType_Yes', N'en-US', N'Yes'),
                (N'ChargeType_Yes', N'vi-VN', N'Có'),
                (N'ChargeType_Yes', N'zh-CN', N'是'),
                (N'ChargeType_No', N'en-US', N'No'),
                (N'ChargeType_No', N'vi-VN', N'Không'),
                (N'ChargeType_No', N'zh-CN', N'否'),
                (N'ChargeType_NewDialogTitle', N'en-US', N'New charge type'),
                (N'ChargeType_NewDialogTitle', N'vi-VN', N'Thêm loại phí'),
                (N'ChargeType_NewDialogTitle', N'zh-CN', N'新建费用类型'),
                (N'ChargeType_EditDialogTitle', N'en-US', N'Edit charge type'),
                (N'ChargeType_EditDialogTitle', N'vi-VN', N'Sửa loại phí'),
                (N'ChargeType_EditDialogTitle', N'zh-CN', N'编辑费用类型'),
                (N'ChargeType_DeleteConfirmBody', N'en-US', N'Delete this charge type? This cannot be undone.'),
                (N'ChargeType_DeleteConfirmBody', N'vi-VN', N'Xóa loại phí này? Thao tác không hoàn tác.'),
                (N'ChargeType_DeleteConfirmBody', N'zh-CN', N'确定删除此费用类型？此操作不可撤销。'),
                (N'ChargeType_CannotDeleteReferenced', N'en-US', N'Cannot delete: this charge type is used by tariff or shipment charge context.'),
                (N'ChargeType_CannotDeleteReferenced', N'vi-VN', N'Không xóa được: loại phí đang được dùng bởi biểu phí hoặc ngữ cảnh tính phí lô hàng.'),
                (N'ChargeType_CannotDeleteReferenced', N'zh-CN', N'无法删除：该费用类型已被运价或货运计费引用。'),
                (N'ChargeType_CodeAlreadyExists', N'en-US', N'Code already exists.'),
                (N'ChargeType_CodeAlreadyExists', N'vi-VN', N'Mã đã tồn tại.'),
                (N'ChargeType_CodeAlreadyExists', N'zh-CN', N'代码已存在。'),
                (N'ChargeType_RecordNotFound', N'en-US', N'Record not found.'),
                (N'ChargeType_RecordNotFound', N'vi-VN', N'Không tìm thấy bản ghi.'),
                (N'ChargeType_RecordNotFound', N'zh-CN', N'未找到记录。'),
                (N'ChargeType_Saved', N'en-US', N'Saved.'),
                (N'ChargeType_Saved', N'vi-VN', N'Đã lưu.'),
                (N'ChargeType_Saved', N'zh-CN', N'已保存。'),
                (N'ChargeType_Deleted', N'en-US', N'Deleted.'),
                (N'ChargeType_Deleted', N'vi-VN', N'Đã xóa.'),
                (N'ChargeType_Deleted', N'zh-CN', N'已删除。'),
                (N'ChargeType_CannotLoad', N'en-US', N'Cannot load: {0}'),
                (N'ChargeType_CannotLoad', N'vi-VN', N'Không tải được: {0}'),
                (N'ChargeType_CannotLoad', N'zh-CN', N'无法加载：{0}'),
                (N'ChargeType_CannotSave', N'en-US', N'Cannot save: {0}'),
                (N'ChargeType_CannotSave', N'vi-VN', N'Không lưu được: {0}'),
                (N'ChargeType_CannotSave', N'zh-CN', N'无法保存：{0}'),
                (N'ChargeType_CannotDelete', N'en-US', N'Cannot delete: {0}'),
                (N'ChargeType_CannotDelete', N'vi-VN', N'Không xóa được: {0}'),
                (N'ChargeType_CannotDelete', N'zh-CN', N'无法删除：{0}'),
                (N'ChargeType_CodeRequired', N'en-US', N'Code is required.'),
                (N'ChargeType_CodeRequired', N'vi-VN', N'Bắt buộc nhập mã.'),
                (N'ChargeType_CodeRequired', N'zh-CN', N'代码为必填项。'),
                (N'ChargeType_NameRequired', N'en-US', N'Name is required.'),
                (N'ChargeType_NameRequired', N'vi-VN', N'Bắt buộc nhập tên.'),
                (N'ChargeType_NameRequired', N'zh-CN', N'名称为必填项。'),
                (N'ChargeType_Warning', N'en-US', N'Warning'),
                (N'ChargeType_Warning', N'vi-VN', N'Cảnh báo'),
                (N'ChargeType_Warning', N'zh-CN', N'警告');
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM LocalizationResources
                WHERE ResourceKey LIKE N'ChargeType%';
                """);
        }
    }
}
