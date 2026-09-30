# 10.18 Kho vật tư, hàng hóa (TK 151–156): hướng dẫn

## Cài đặt (1 lần cho mỗi database tenant)

1. Chạy `Scripts/CreateKhoTables.sql`. Script này nằm sẵn trong `TongHop_ThayDoi_Database_20260929.sql`, mục 5. Script tạo 4 bảng: `KhoVatTu`, `KhoHang`, `PhieuKho`, `PhieuKhoChiTiet`. Nó cũng tạo mã quyền `KHO_*`, cấp quyền cho user phòng ADMIN và tạo 1 kho mẫu `KHO01`.
2. Cấp quyền cho kế toán tại **1.6 Phân quyền**:

   | Mã quyền | Màn hình | Quyền dùng |
   |---|---|---|
   | `KHO_DanhMuc` | 10.18.1 Danh mục vật tư, kho | Xem / Thêm / Sửa / Xóa |
   | `KHO_Phieu` | 10.18.2 Phiếu nhập, xuất kho | Xem / Thêm / Sửa, Xóa (phiếu nháp) / **Duyệt = Ghi sổ, Bỏ ghi sổ, Tính lại giá xuất** |
   | `KHO_BaoCao` | 10.18.3 Tổng hợp NXT, 10.18.4 Sổ chi tiết | Xem |

3. Người dùng bấm F5 để thấy nhóm menu **10.18**.
4. Trong danh mục tài khoản phải có các TK sẽ dùng, gồm TK kho (1521, 1531, 1561…) và TK đối ứng (331, 1111, 632, 621, 642…). Nếu chưa có thì phần mềm báo lỗi khi lưu.

## Quy trình

1. **10.18.1**: khai kho, rồi khai vật tư (mã, tên, ĐVT, TK kho 151–156). Có thể nhập danh mục từ Excel: bấm "Tải file mẫu" để lấy mẫu.
2. **10.18.2 → Tồn đầu kỳ**: nhập số lượng và giá trị tồn lúc bắt đầu dùng phần mềm, rồi bấm **Ghi sổ**. Phiếu tồn đầu **không** sinh bút toán, vì số dư đầu TK 15x đã nhập ở 10.9.
3. **Nhập kho**: chọn lý do, phần mềm tự điền TK đối ứng (mua chưa trả → 331, tiền mặt → 1111…). Nhập SL, đơn giá và % thuế. Bấm **Lưu & ghi sổ**, phần mềm sinh bút toán:
   - Nợ 15x / Có 331
   - Nợ 1331 / Có 331 (tiền thuế)
4. **Xuất kho**: chỉ nhập số lượng. Đơn giá do phần mềm tính theo **bình quân gia quyền tức thời** khi ghi sổ (trên màn hình hiện số tạm tính màu xanh). Bút toán: Nợ 632/621/627/641/642/242… / Có 15x.
5. **Chuyển kho**: hàng chuyển giữa 2 kho theo giá bình quân của kho xuất. Không sinh bút toán vì vẫn cùng TK.
6. **In phiếu (Excel)**: in phiếu nhập kho, phiếu xuất kho có đủ chữ ký.

## Giá xuất bình quân tức thời

- Mỗi vật tư tại mỗi kho được tính riêng: giá xuất = giá trị tồn / số lượng tồn ngay trước lần xuất, rồi làm tròn đồng. Khi xuất hết thì lấy toàn bộ giá trị còn lại, không để lại số lẻ.
- Thứ tự tính: theo ngày chứng từ. Trong cùng một ngày thì theo thứ tự tồn đầu → nhập → chuyển → xuất, sau đó theo giờ lập phiếu.
- Khi ghi sổ hoặc bỏ ghi sổ một phiếu **lùi ngày**, phần mềm tự tính lại giá các phiếu xuất sau ngày đó và **sửa luôn bút toán sổ cái** của các phiếu ấy. Thông báo sẽ liệt kê các phiếu đã được tính lại.
- Nếu ghi sổ làm tồn kho bị âm ở bất kỳ thời điểm nào thì phần mềm chặn lại và báo rõ phiếu nào, mã nào, thiếu bao nhiêu.
- Kỳ đã khóa sổ (10.8) thì không lập, sửa, ghi sổ hay bỏ ghi sổ được. Phần mềm cũng không tự sửa giá của phiếu thuộc kỳ đã khóa.

## Sửa phiếu đã ghi sổ

Bấm **Bỏ ghi sổ** (cần quyền Duyệt) → phiếu về nháp và bút toán của phiếu bị xóa → sửa → ghi sổ lại.
Chứng từ do phiếu kho sinh ra (xem ở 10.4.1, nguồn `INVENTORY`) **không sửa, xóa hay bỏ ghi sổ ở 10.4.1 được**. Như vậy tồn kho và sổ cái luôn khớp nhau.

## Báo cáo

- **10.18.3 Tổng hợp nhập xuất tồn** (mẫu S11-DN) gồm tồn đầu, nhập, xuất, tồn cuối theo SL và giá trị, nhóm theo TK. Lọc được theo kho và theo TK. Dòng có tồn thấp hơn "tồn tối thiểu" được tô cam. Bấm vào 1 dòng để mở sổ chi tiết.
  Mục **Đối chiếu sổ cái** so giá trị tồn kho với Nợ − Có của TK trong sổ cái:
  - Nếu chênh lệch khác 0, nghĩa là có bút toán TK 15x được ghi ngoài phiếu kho, ví dụ nhập tay ở 10.4.1.
  - Hoặc có phiếu kho chưa ghi sổ.
- **10.18.4 Sổ chi tiết vật tư / thẻ kho** (mẫu S10-DN) liệt kê từng lần nhập, xuất kèm tồn lũy kế. Xuất Excel được.
