# Viva Vivu Travel Booking

Đồ án cuối học phần **Hệ thống thanh toán điện tử**  
Giảng viên hướng dẫn: **ThS. Lê Hữu Thanh Tùng**

## Giới thiệu

**Viva Vivu Travel Booking** là website đặt tour du lịch được xây dựng bằng ASP.NET Core MVC. Dự án mô phỏng quy trình đặt tour trực tuyến từ lúc người dùng xem tour, tạo booking, áp dụng ưu đãi, chọn phương thức thanh toán đến khi hệ thống ghi nhận kết quả giao dịch.

Trọng tâm của đồ án là triển khai luồng thanh toán điện tử trong một hệ thống thương mại dịch vụ, bao gồm tích hợp cổng thanh toán, xử lý callback/return URL, cập nhật trạng thái đơn đặt tour và quản trị dữ liệu giao dịch.

## Chức năng chính

### Người dùng

- Đăng ký, đăng nhập và đăng xuất tài khoản.
- Cập nhật thông tin cá nhân và ảnh đại diện.
- Xem danh sách tour du lịch đang hoạt động.
- Lọc tour theo điểm đến, tháng khởi hành và khoảng giá.
- Xem chi tiết tour, điểm đến và lịch trình.
- Đặt tour với số lượng hành khách mong muốn.
- Áp dụng voucher giảm giá khi đặt tour.
- Thanh toán booking qua nhiều phương thức:
  - VNPay
  - PayPal
  - MoMo
  - Thanh toán tiền mặt
- Xem lịch sử đặt tour và trạng thái thanh toán.
- Hủy booking khi booking chưa thanh toán và tour chưa khởi hành.
- Xuất danh sách booking cá nhân ra PDF.

### Quản trị viên

- Quản lý tài khoản người dùng.
- Phân quyền Admin/User.
- Khóa và mở khóa tài khoản.
- Quản lý tour du lịch.
- Quản lý điểm đến.
- Quản lý booking.
- Quản lý voucher.
- Quản lý lịch trình tour.
- Theo dõi dashboard thống kê booking, doanh thu và lợi nhuận ước tính.
- Xuất dữ liệu tour, booking, điểm đến ra PDF hoặc Excel.

## Luồng thanh toán điện tử

1. Người dùng chọn tour và nhập thông tin đặt tour.
2. Hệ thống tính tổng tiền dựa trên giá tour, số lượng hành khách, hạng thành viên và voucher.
3. Booking được tạo với trạng thái chờ thanh toán.
4. Người dùng chọn phương thức thanh toán.
5. Hệ thống chuyển người dùng sang cổng thanh toán tương ứng.
6. Sau khi giao dịch hoàn tất, cổng thanh toán chuyển về return URL của hệ thống.
7. Hệ thống kiểm tra kết quả giao dịch và cập nhật:
   - trạng thái thanh toán,
   - trạng thái booking,
   - mã giao dịch,
   - cổng thanh toán,
   - thời gian hoàn tất thanh toán.

## Cổng thanh toán đã tích hợp

### VNPay

- Tạo URL thanh toán sandbox.
- Truyền thông tin số tiền, mã booking, thời gian tạo giao dịch và return URL.
- Ký dữ liệu bằng HMAC-SHA512.
- Kiểm tra chữ ký khi nhận kết quả trả về.
- Cập nhật booking thành công khi `vnp_ResponseCode = 00`.

### PayPal

- Lấy access token từ PayPal sandbox.
- Tạo order với intent `CAPTURE`.
- Chuyển người dùng sang trang approve của PayPal.
- Capture order sau khi thanh toán thành công.
- Cập nhật trạng thái booking và lưu mã giao dịch.

### MoMo

- Tạo yêu cầu thanh toán tới MoMo test gateway.
- Ký dữ liệu bằng HMAC-SHA256.
- Nhận `payUrl` để chuyển người dùng sang màn hình thanh toán.
- Có endpoint nhận dữ liệu trả về từ MoMo.

### Tiền mặt

- Cho phép tạo booking với trạng thái chờ xử lý.
- Phù hợp với trường hợp khách hàng thanh toán trực tiếp.

## Công nghệ sử dụng

- ASP.NET Core MVC (.NET 9)
- C#
- Entity Framework Core
- PostgreSQL
- ASP.NET Core Identity
- Razor Views
- Bootstrap, CSS, JavaScript
- VNPay Sandbox
- PayPal Sandbox API
- MoMo Test Gateway
- QuestPDF
- ClosedXML
- MailKit
- Serilog
- Bogus

## Cấu trúc thư mục

```text
Controllers/        Xử lý request, điều hướng nghiệp vụ và view
Models/             Các entity chính như Tour, Booking, Voucher, User
ViewModels/         Dữ liệu trung gian cho giao diện
Views/              Razor views cho người dùng và quản trị viên
Data/               DbContext, seeder và dữ liệu mẫu
Services/           Dịch vụ thanh toán, email, PDF, Excel
Repositories/       Repository pattern và Unit of Work
Enums/              Trạng thái booking, thanh toán, phương thức thanh toán
Middlewares/        Middleware xử lý lỗi
wwwroot/            CSS, JavaScript, hình ảnh, font và file upload
Migrations/         Migration của Entity Framework Core
```

## Yêu cầu cài đặt

- .NET SDK 9.0 trở lên
- PostgreSQL
- Visual Studio 2022 hoặc Visual Studio Code
- Tài khoản sandbox/test cho VNPay, PayPal và MoMo nếu muốn chạy đầy đủ luồng thanh toán

## Cấu hình

Cập nhật chuỗi kết nối và thông tin sandbox trong `appsettings.json` hoặc dùng biến môi trường/user secrets.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=...;Username=...;Password=..."
  },
  "VNPay": {
    "TmnCode": "...",
    "HashSecret": "...",
    "BaseUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
    "ReturnUrl": "https://localhost:xxxx/Payment/VNPayReturn"
  },
  "PayPal": {
    "ClientId": "...",
    "ClientSecret": "...",
    "BaseUrl": "https://api-m.sandbox.paypal.com"
  },
  "MoMo": {
    "PartnerCode": "...",
    "AccessKey": "...",
    "SecretKey": "...",
    "Endpoint": "https://test-payment.momo.vn/v2/gateway/api/create",
    "ReturnUrl": "https://localhost:xxxx/Payment/MoMoReturn",
    "NotifyUrl": "https://localhost:xxxx/Payment/MoMoNotify"
  }
}
```

> Lưu ý: Khi chạy local, cần thay các return URL bằng địa chỉ local hoặc public URL được tunnel qua công cụ như ngrok để cổng thanh toán có thể gọi lại hệ thống.

## Cách chạy dự án

1. Clone repository:

```bash
git clone https://github.com/dieupham205/Nhom02.git
cd Nhom02
```

2. Restore package:

```bash
dotnet restore
```

3. Cập nhật database:

```bash
dotnet ef database update
```

4. Chạy ứng dụng:

```bash
dotnet run
```

5. Mở trình duyệt theo URL hiển thị trong terminal, ví dụ:

```text
https://localhost:xxxx
```

## Tài khoản quản trị mặc định

Khi ứng dụng khởi chạy, hệ thống tự seed tài khoản quản trị:

```text
Email: admin@tour.com
Password: Admin@123
```

Tài khoản này dùng để truy cập các chức năng quản trị như quản lý tour, booking, voucher, người dùng và dashboard.

## Một số màn hình/chức năng tiêu biểu

- Trang chủ giới thiệu website du lịch.
- Danh sách tour nội địa và quốc tế.
- Trang chi tiết tour và lịch trình.
- Form đặt tour.
- Trang thanh toán qua VNPay, PayPal, MoMo.
- Trang kết quả thanh toán thành công/thất bại.
- Trang lịch sử đặt tour của người dùng.
- Trang quản trị booking, tour, điểm đến, voucher.
- Dashboard thống kê doanh thu và trạng thái booking.

## Ý nghĩa học phần

Thông qua đồ án, nhóm thực hành các nội dung chính của môn Hệ thống thanh toán điện tử:

- Xây dựng quy trình đặt hàng/đặt dịch vụ trực tuyến.
- Tạo giao dịch thanh toán qua cổng thanh toán trung gian.
- Truyền dữ liệu giao dịch giữa website và payment gateway.
- Ký và kiểm tra chữ ký để đảm bảo tính toàn vẹn dữ liệu.
- Xử lý phản hồi thanh toán thành công/thất bại.
- Quản lý trạng thái đơn hàng sau giao dịch.
- Ghi nhận mã giao dịch, phương thức thanh toán và thời gian thanh toán.
- Kết hợp thanh toán điện tử với nghiệp vụ quản trị hệ thống.

## Thành viên thực hiện

Nhóm 02

## Ghi chú

Dự án phục vụ mục đích học tập và mô phỏng nghiệp vụ thanh toán điện tử. Các thông tin cấu hình cổng thanh toán nên sử dụng môi trường sandbox/test, không dùng trực tiếp thông tin production trong mã nguồn công khai.
