[English](../../Usage.md) | [한국어](../ko-KR/Usage.md) | [日本語](../ja-JP/Usage.md) | Tiếng Việt

## Cài đặt và Chạy

##### 1. Cấu trúc tệp
1. Windows
   ```sh
    tscam.exe   # Tệp thực thi TS-CAM
    .env        # Tệp cấu hình biến môi trường
   ```

2. Linux
   ```sh
    tscam       # Tệp thực thi TS-CAM
    .env        # Tệp cấu hình biến môi trường
   ```

##### 2. Biến môi trường
   Bạn có thể cấu hình hoạt động của `tscam.exe` bằng cách đặt các biến môi trường.
   Biến môi trường có thể được đặt trong tệp script khi đăng ký dịch vụ hoặc đơn giản là trong tệp `.env`.
   Tệp `.env` phải luôn nằm trong cùng thư mục với tệp `tscam.exe`.

   ```sh
     TSCAM_HTTP_PORT=10000                    # Số cổng TCP để lắng nghe
     #TSCAM_DATA_DIR=C:\Users\bob\tscam\data  # Thư mục lưu trữ hình ảnh chụp nhanh
     #TSCAM_LOG_DIR=C:\Users\bob\tscam\log    # Thư mục lưu trữ log

     # Sử dụng nếu bạn muốn thay đổi hiển thị thời gian từ mili giây (mặc định) sang giây
     #TSCAM_NO_MILLISECONDS=1

     # Tiền tố đường dẫn URL cho hình ảnh chụp nhanh
     #TSCAM_URI_DATA_PATH_PREFIX=/site1

     # Đặt nếu công cụ nhận dạng biển số xe ở vị trí khác với tscam.exe
     #TSANPR=C:\Program Files\TS-Solution\TS-ANPR\tsanpr-v3.0.0M\windows-x86_64\tsanpr.dll

     # Cài đặt tệp log (mặc định)
     #	maxSize: Kích thước tối đa của một tệp log đơn lẻ
     # 	maxFiles: Số lượng tệp log được giữ lại (các tệp cũ hơn sẽ tự động bị xóa sau số ngày quy định)
     # 	size: Kích thước tối đa của toàn bộ bộ nhớ log (nếu tổng kích thước log vượt quá mức này, các tệp cũ nhất sẽ bị xóa trước)
     # TSCAM_LOG_CONFIG={"maxSize":"20m","maxFiles":"31d","size":"1024m"}

     # Mức độ log lưu vào tệp (đặt một trong các giá trị: info, warn, error)
     TSCAM_LOG_LEVEL_FILE=info        # Lưu tất cả log
     # Mức độ log xuất ra console (đặt một trong các giá trị: info, warn, error)
     TSCAM_LOG_LEVEL_CONSOLE=info     # Xuất tất cả log
   ```

##### 3. Môi trường phát triển
   Trong môi trường phát triển, việc cấu hình để trạng thái hoạt động được hiển thị theo thời gian thực trên console rất tiện lợi.

   - Chỉnh sửa Biến môi trường
     Vì môi trường thực thi có thể được cấu hình bằng biến môi trường, trước tiên hãy chỉnh sửa tệp `.env` cho phù hợp với môi trường phát triển của bạn. Bạn có thể sử dụng cài đặt biến môi trường của hệ điều hành, nhưng đối với phát triển, nên sử dụng đơn giản tệp `.env`.
     Log được cấu hình riêng.

     ```sh
     # Mức độ log lưu vào tệp (đặt một trong các giá trị: info, warn, error)
     TSCAM_LOG_LEVEL_FILE=info        # Lưu tất cả log
     # Mức độ log xuất ra console (đặt một trong các giá trị: info, warn, error)
     TSCAM_LOG_LEVEL_CONSOLE=info     # Xuất tất cả log
     ```

   - Chạy TS-CAM
     Mở cửa sổ console và chạy tscam.
     ```
     C:\Program Files\TS-Solution\TS-ANPR\tsanpr-v3.0.0M\windows-x86_64> tscam
     tscam v0.1.0
     log= C:\Users\bob\tscam\log
     data= C:\Users\bob\tscam\data
     2024-08-09 16:36:35.201 info: Process started { pid: 18764 }
     2024-08-09 16:36:35.446 info: os_name=win32, arch_name=x64
     2024-08-09 16:36:35.448 info: LIB_PATH= 'C:\Program Files\TS-Solution\TS-ANPR\tsanpr-v3.0.0M\windows-x86_64\tsanpr.dll'
     2024-08-09 16:36:36.547 info: TS-ANPR v2.4.0 is ready
     2024-08-09 16:36:36.570 info: listening 127.0.0.1:10000
     ```
   - Dừng TS-CAM
     Nhập `Ctrl+C` trong cửa sổ console để dừng chương trình.
     ```
     2024-08-09 16:37:02.240 info: SIGINT received. Shutting down gracefully
     2024-08-09 16:37:02.243 info: Closing server
     2024-08-09 16:37:03.245 info: Server closed
     2024-08-09 16:37:03.248 info: Socket.IO server closed
     2024-08-09 16:37:03.250 info: Process terminated { pid: 18764 }
     ```

##### 4. Môi trường sản xuất
1. Windows
   Để đảm bảo khả năng phục hồi, hãy chạy dưới dạng dịch vụ hệ thống trong môi trường sản xuất.

   - Đăng ký Dịch vụ
     Đầu tiên, sao chép thư mục `tscam` và `TS-ANPR` đến vị trí mong muốn của bạn. Sau đó, để đăng ký nó như một dịch vụ hệ thống, hãy sửa đổi các phần cần thiết của tệp `utils/windows-service/addsvc.bat`.
     Ví dụ, nếu bạn đã sao chép nó vào `C:\Program Files\TS-Solution\TS-ANPR\`, bạn có thể sửa đổi nó như sau:

     ```batch
     @echo off

     REM Script cài đặt dịch vụ tscam

     REM Chạy với quyền quản trị viên.

     reg Query "HKLM\Hardware\Description\System\CentralProcessor\0" | find /i "x86" > NUL && set NSSM=win32\nssm.exe || set NSSM=win64\nssm.exe

     REM Sửa đổi thành đường dẫn thực thi thực tế.
     %NSSM% install tscam "C:\Program Files\TS-Solution\TS-ANPR\tsanpr-v3.0.0M\windows-x86_64\tscam.exe"
     %NSSM% set tscam AppExit Default Restart
     %NSSM% set tscam AppRestartDelay 3000

     REM Đặt các biến môi trường cần thiết
     REM Số cổng HTTP
     REM %NSSM% set tscam AppEnvironment TSCAM_HTTP_PORT=10000

     REM Thư mục lưu trữ dữ liệu
     REM %NSSM% set tscam AppEnvironment "TSCAM_DATA_DIR=C:\ProgramData\TS-Solution\tscam\data"

     REM Thư mục tệp log
     REM %NSSM% set tscam AppEnvironment "TSCAM_LOG_DIR=C:\ProgramData\TS-Solution\tscam\log"

     REM Xóa đơn vị mili giây
     REM %NSSM% set tscam AppEnvironment TSCAM_NO_MILLISECONDS=1

     REM Tiền tố đường dẫn tải xuống tệp hình ảnh đã lưu
     REM %NSSM% set tscam AppEnvironment TSCAM_URI_DATA_PATH_PREFIX=/site1"

     REM Mức độ log console (info, warn, error)
     REM Trong môi trường sản xuất, đặt thành warn hoặc error để giảm tải ghi.
     REM %NSSM% set tscam AppEnvironment TSCAM_LOG_LEVEL_CONSOLE=error

     REM Mức độ log tệp (info, warn, error)
     REM Trong môi trường sản xuất, đặt thành warn hoặc error để giảm tải ghi.
     REM %NSSM% set tscam AppEnvironment TSCAM_LOG_LEVEL_FILE=error

     REM Cài đặt tệp log (thoát bằng dấu ngoặc kép)
     REM maxSize: Kích thước tối đa của một tệp log đơn lẻ
     REM maxFiles: Số lượng tệp log được giữ lại (các tệp cũ hơn sẽ tự động bị xóa sau số ngày quy định)
     REM size: Kích thước tối đa của toàn bộ bộ nhớ log (nếu tổng kích thước log vượt quá mức này, các tệp cũ nhất sẽ bị xóa trước)
     REM %NSSM% set tscam AppEnvironment "TSCAM_LOG_CONFIG={{}}\"maxSize\":\"20m\",\"maxFiles\":\"31d\",\"size\":\"1024m\"{{}}}}"

     REM Đặt nếu công cụ nhận dạng biển số xe ở vị trí khác với tscam.exe
     REM %NSSM% set tscam AppEnvironment "TSANPR=C:\Program Files\TS-Solution\TS-ANPR\tsanpr-v3.0.0M\windows-x86_64\tsanpr.dll"

     %NSSM% start tscam
     %NSSM% status tscam

     ```

     Sau khi lưu tệp `addsvc.bat` đã sửa đổi, hãy chạy nó với quyền quản trị viên để hoàn tất đăng ký.

   - Xóa Dịch vụ
     Để xóa dịch vụ `tscam` đã đăng ký, hãy chạy `utils/windows-service/rmsvc.bat` với quyền quản trị viên.

2. Linux

   - Đăng ký Dịch vụ
     Để đăng ký dưới dạng dịch vụ hệ thống, hãy sửa đổi các phần cần thiết của tệp `utils/linux-service/addsvc.sh`.
     Ví dụ, nếu bạn đã sao chép nó vào `/var/tsanpr/`, bạn có thể sửa đổi nó như sau:

   ```sh
   #!/bin/bash

   # Script đăng ký dịch vụ tscam

   echo "# Cấu hình tscam

   [Unit]
   Description=tscam (ONVIF Camera Broker)
   After=network.target

   [Service]
   # Số cổng HTTP
   # Environment=\"TSCAM_HTTP_PORT=10000\"

   # Thư mục lưu trữ dữ liệu
   # Environment=\"TSCAM_DATA_DIR=/var/tscam/data\"

   # Thư mục tệp log
   # Environment=\"TSCAM_LOG_DIR=/var/tscam/log\"

   # Xóa đơn vị mili giây
   # Environment=\"TSCAM_NO_MILLISECONDS=1\"

   # Tiền tố đường dẫn tải xuống tệp hình ảnh đã lưu
   # Environment=\"TSCAM_URI_DATA_PATH_PREFIX=/site1\"

   # Mức độ log console (info, warn, error)
   # Environment=\"TSCAM_LOG_LEVEL_CONSOLE=error\"

   # Mức độ log tệp (info, warn, error)
   # Environment=\"TSCAM_LOG_LEVEL_FILE=error\"

   # Cài đặt tệp log
   # 	maxSize: Kích thước tối đa của một tệp log đơn lẻ
   # 	maxFiles: Số lượng tệp log được giữ lại (các tệp cũ hơn sẽ tự động bị xóa sau số ngày quy định)
   # 	size: Kích thước tối đa của toàn bộ bộ nhớ log (nếu tổng kích thước log vượt quá mức này, các tệp cũ nhất sẽ bị xóa trước)
   # Environment=\"TSCAM_LOG_CONFIG={\\\"maxSize\\\":\\\"20m\\\",\\\"maxFiles\\\":\\\"31d\\\",\\\"size\\\":\\\"1024m\\\"}\"

   # Đặt nếu công cụ nhận dạng biển số xe ở vị trí khác với tscam
   # Environment=\"TSANPR=/var/tsanpr/tsanpr-v3.0.0M/linux-x86_64/libtsanpr.so\"

   # Tham số khởi tạo TSANPR
   # Environment=\"TSANPR_COUNTRY=KR\"
   # Environment=\"TSANPR_MIN_CHAR=4\"
   # Environment=\"TSANPR_SYMBOL=full\"

   # Mục bắt buộc
   WorkingDirectory=/var/tsanpr

   # Đường dẫn tệp thực thi tscam
   ExecStart=/var/tsanpr/tsanpr-v3.0.0M/linux-x86_64/tscam

   # Tự động khởi động lại sau 3 giây nếu bị dừng
   Restart=always
   RestartSec=3
   LimitNOFILE=400000
   Type=simple

   [Install]
   WantedBy=multi-user.target
   " > ~/.tmp.tscam.service
   sudo mv ~/.tmp.tscam.service /etc/systemd/system/tscam.service
   
   sudo systemctl daemon-reload
   sudo systemctl enable tscam
   sudo systemctl restart tscam
   ```

   - Xóa Dịch vụ
     Để xóa dịch vụ `tscam` đã đăng ký, hãy chạy `utils/linux-service/rmsvc.sh` với quyền quản trị viên.