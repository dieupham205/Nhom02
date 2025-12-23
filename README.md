# MÔN PHÁT TRIỂN ỨNG DỤNG THƯƠNG MẠI ĐIỆN TỬ

## Tên đề tài: “Thiết kế và xây dựng website đặt tour du lịch VivaVivu”
Trong bối cảnh thương mại điện tử và du lịch trực tuyến ngày càng phát triển, việc xây dựng các nền tảng đặt tour du lịch trực tuyến đóng vai trò quan trọng trong việc nâng cao trải nghiệm người dùng và tối ưu hóa hoạt động kinh doanh dịch vụ du lịch. Các website đặt tour hiện đại không chỉ cung cấp thông tin tour mà còn tích hợp các chức năng đặt chỗ, thanh toán, quản lý lịch sử giao dịch và cá nhân hóa người dùng. Xuất phát từ nhu cầu thực tiễn đó, đề tài “Thiết kế và xây dựng website đặt tour du lịch VivaVivu” được thực hiện nhằm xây dựng một hệ thống website thương mại điện tử hoàn chỉnh, hỗ trợ người dùng tìm kiếm, đặt tour và thanh toán trực tuyến, đồng thời cung cấp công cụ quản lý hiệu quả cho phía quản trị viên.

## Mô tả và mục tiêu dự án
### Mô tả:
Dự án “Thiết kế và xây dựng website đặt tour du lịch VivaVivu” là một hệ thống website thương mại điện tử được phát triển trên nền tảng ASP.NET MVC, nhằm hỗ trợ hoạt động giới thiệu, đặt tour và quản lý dịch vụ du lịch trực tuyến. Website cho phép người dùng xem danh sách tour, xem chi tiết lịch trình, thực hiện đặt tour, lựa chọn phương thức thanh toán và theo dõi lịch sử các đơn đặt tour của mình. Hệ thống được thiết kế với giao diện trực quan, thân thiện, sử dụng các thành phần như Bootstrap Carousel, Accordion, và các hiệu ứng tương tác để nâng cao trải nghiệm người dùng. Ngoài ra, website còn hỗ trợ quản lý tài khoản cá nhân, cập nhật thông tin người dùng, quản lý booking và trạng thái thanh toán. Các chức năng được phân quyền rõ ràng giữa người dùng và hệ thống quản lý nhằm đảm bảo tính an toàn và hiệu quả trong vận hành.
### Mục tiêu:
- Xây dựng nền tảng đặt tour du lịch trực tuyến thân thiện với người dùng
- Hỗ trợ khách hàng tìm kiếm, xem chi tiết và đặt tour nhanh chóng
- Đơn giản hóa quy trình đặt tour và thanh toán trực tuyến
- Cung cấp kênh quảng bá và quản lý tour cho doanh nghiệp du lịch
- Góp phần thúc đẩy chuyển đổi số trong lĩnh vực thương mại điện tử du lịch
### Tác giả:
- Phạm Thị Ngọc Diệu (Nhóm trưởng)
- Nguyễn Liên Trường Thịnh
- Nguyễn Nhật Linh
- Đào Thanh Bảo Trâm
- Hồ Lê Hoài Thương
- Nguyễn Bảo Lâm
- Nguyễn Trần Thúy Vi

## Xây dựng website đặt tour du lịch
### Kiến thức áp dụng
- Phân quyền người dùng (Admin / User) trong quá trình truy cập và sử dụng chức năng hệ thống
- Sử dụng ASP.NET MVC để tổ chức source code theo mô hình phân tách rõ ràng giữa giao diện, xử lý nghiệp vụ và dữ liệu
- Thiết kế giao diện người dùng (UI) kết hợp HTML, CSS, Bootstrap nhằm đảm bảo tính trực quan, responsive và trải nghiệm người dùng tốt
- Xử lý dữ liệu động trên giao diện bằng Razor View Engine, vòng lặp, điều kiện và binding dữ liệu từ Model sang View
- Áp dụng các nguyên tắc CRUD (Create – Read – Update – Delete) trong quản lý tour và đơn đặt
- Tối ưu trải nghiệm người dùng trong quy trình đặt tour, thanh toán và xem lịch sử booking
- Quản lý trạng thái đặt tour và thanh toán thông qua các enum (BookingStatus, PaymentStatus)
- Kiến thức nền tảng về phát triển ứng dụng thương mại điện tử, đặc biệt trong lĩnh vực du lịch trực tuyến
- Áp dụng mô hình website đặt tour du lịch với các chức năng tìm kiếm, xem chi tiết, đặt tour và thanh toán
### Triển khai
#### Nền tảng xây dựng
Website đặt tour du lịch VivaVivu được xây dựng trên nền tảng ASP.NET MVC sử dụng ngôn ngữ lập trình C#, áp dụng mô hình kiến trúc Model – View – Controller nhằm tách biệt rõ ràng giữa xử lý nghiệp vụ, dữ liệu và giao diện người dùng. Cách tiếp cận này giúp hệ thống dễ dàng mở rộng, bảo trì và nâng cấp trong tương lai, đồng thời phù hợp với các dự án thương mại điện tử có quy mô vừa và lớn.

Website sử dụng HTML5, CSS3 và Bootstrap để xây dựng giao diện hiện đại, trực quan và tương thích đa thiết bị. Thiết kế responsive giúp người dùng có thể truy cập và đặt tour thuận tiện trên máy tính, máy tính bảng và điện thoại thông minh. Các thành phần giao diện như thẻ tour, carousel gợi ý tour và biểu mẫu đặt tour được thiết kế nhằm tối ưu trải nghiệm người dùng.

Bên cạnh đó, JavaScript và một số thư viện giao diện được sử dụng để xử lý các tương tác phía người dùng, hỗ trợ hiệu ứng chuyển động, điều hướng, xác nhận hành động và cải thiện tính trực quan trong quá trình tìm kiếm và đặt tour du lịch.

Về lưu trữ dữ liệu, hệ thống sử dụng Azure SQL Database trên nền tảng Microsoft Azure để quản lý các thông tin liên quan đến tour du lịch, điểm đến, người dùng, booking và thanh toán.

Việc triển khai cơ sở dữ liệu trên Azure giúp đảm bảo tính ổn định, bảo mật cao, khả năng sao lưu và mở rộng linh hoạt, đáp ứng nhu cầu vận hành website trong môi trường thực tế.

Công nghệ Entity Framework được sử dụng để kết nối và thao tác dữ liệu theo hướng đối tượng, giúp đơn giản hóa quá trình truy vấn cơ sở dữ liệu, giảm thiểu lỗi và tăng hiệu suất phát triển phần mềm. Điều này cho phép nhóm tập trung nhiều hơn vào logic nghiệp vụ và trải nghiệm người dùng.

Cuối cùng, dự án được quản lý bằng Git và GitHub, hỗ trợ làm việc nhóm hiệu quả, theo dõi lịch sử chỉnh sửa source code và đảm bảo quá trình phát triển diễn ra minh bạch, có kiểm soát.
#### Chuẩn Lập Trình
Kiến trúc ASP.NET MVC được áp dụng nghiêm ngặt nhằm tách biệt rõ ràng giữa ba thành phần: Model (xử lý dữ liệu và nghiệp vụ), View (giao diện người dùng) và Controller (điều hướng và xử lý logic). Cách tổ chức này giúp source code có cấu trúc rõ ràng, tránh tình trạng trộn lẫn xử lý nghiệp vụ vào giao diện, từ đó nâng cao khả năng tái sử dụng và bảo trì hệ thống.
Trong quá trình truy cập cơ sở dữ liệu, nhóm sử dụng Entity Framework theo hướng Code First/Database First (tùy giai đoạn), hạn chế viết truy vấn SQL trực tiếp trong Controller. Việc thao tác dữ liệu thông qua các lớp DbContext và entity giúp tăng tính an toàn, giảm lỗi SQL Injection và đảm bảo tính nhất quán của dữ liệu.

Đối với giao diện người dùng, code Razor View được tổ chức gọn gàng, hạn chế viết logic phức tạp trực tiếp trong View. Các khối xử lý điều kiện, vòng lặp chỉ được sử dụng ở mức hiển thị dữ liệu, trong khi các nghiệp vụ xử lý chính được thực hiện tại Controller hoặc Service. CSS được tách riêng và đặt tên class có quy ước rõ ràng để dễ dàng tái sử dụng và chỉnh sửa giao diện.

Ngoài ra, dự án tuân thủ các chuẩn về bảo mật và an toàn dữ liệu, bao gồm sử dụng Anti-Forgery Token cho các form quan trọng (đặt tour, hủy booking), kiểm tra dữ liệu đầu vào, phân quyền người dùng rõ ràng giữa Admin và User, cũng như hạn chế truy cập trái phép vào các chức năng quản trị hệ thống.

Cuối cùng, source code được quản lý bằng Git, tuân thủ quy trình commit rõ ràng với thông điệp mô tả ngắn gọn nội dung thay đổi. Việc này giúp dễ dàng theo dõi lịch sử phát triển, hỗ trợ làm việc nhóm hiệu quả và thuận tiện trong quá trình đánh giá, nghiệm thu và bảo trì dự án.

## Lời cảm ơn
Nhóm xin gửi lời cảm ơn chân thành và sâu sắc nhất đến thầy Nguyễn Mạnh Tuấn, giảng viên bộ môn Phát triển ứng dụng thương mại điện tử, người đã tận tâm giảng dạy và đồng hành cùng lớp trong suốt học kỳ vừa qua. Thầy không chỉ truyền đạt những kiến thức nền tảng và hiện đại về HTML, CSS, JavaScript, Advanced Front-end, mà còn mở rộng sang các công nghệ quan trọng như ASP.NET MVC, NodeJS và PHP, giúp nhóm có đủ kiến thức và tư duy để tiếp cận việc xây dựng một website hoàn chỉnh.

## Danh sách lỗi