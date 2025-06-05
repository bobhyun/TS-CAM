[English](../../README.md) | [한국어](../ko-KR/README.md) | [日本語](../ja-JP/README.md) | Tiếng Việt

# Ví dụ TS-CAM Java

Đây là ví dụ về việc triển khai Java cho TS-CAM Socket.IO client. Nó thực hiện các chức năng sau theo thứ tự:

1. Phát hiện camera trên mạng nội bộ
2. Đọc thông tin từ camera đầu tiên (yêu cầu đăng nhập camera)
3. Yêu cầu ảnh chụp màn hình với các tùy chọn nhận dạng biển số
4. Kiểm soát đầu ra relay
5. Chờ sự kiện
6. Kiểm tra danh sách theo dõi sự kiện
7. Dừng việc theo dõi sự kiện

Chương trình chờ đầu vào của người dùng ở mỗi bước và thực hiện kiểm soát camera và nhận sự kiện theo thời gian thực thông qua giao tiếp với máy chủ Socket.IO.

## Cấu trúc dự án

```
tscam-app/
├── src/
│   └── main/
│       └── java/
│           └── com/example/tscam/
│               └── Main.java
├── build.gradle
└── settings.gradle
```

## Thư viện cần thiết

- io.socket:socket.io-client:2.1.1
- org.json:json:20240303
- Java 17

## Cách xây dựng và chạy

#### Sử dụng IntelliJ IDEA
1. Mở dự án trong IntelliJ IDEA
2. Nhập dự án Gradle (sử dụng Groovy DSL)
3. Xây dựng dự án
4. Chạy chương trình

#### Sử dụng dòng lệnh
1. Mở thư mục dự án
2. Mở terminal
3. Xây dựng dự án
    ```bash
    # Windows
    gradlew build

    # Linux
    ./gradlew build
    ```
4. Chạy chương trình
    ```bash
    # Windows
    gradlew --console=plain run

    # Linux
    ./gradlew --console=plain run
    ```
