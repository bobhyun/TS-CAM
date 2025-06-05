[English](../../README.md)Tiếng Anh | [한국어](../ko-KR/README.md) | [日本語](../ja-JP/README.md) | Tiếng Việt

# Ví dụ TS-CAM Kotlin

Ví dụ này là một triển khai Kotlin của máy khách TS-CAM Socket.IO. Nó thực hiện tuần tự các chức năng sau:

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
│       └── kotlin/
│           └── com/example/tscam/
│               └── Main.kt
└── build.gradle.kts
```

## Thư viện cần thiết

- io.socket:socket.io-client:2.1.1
- org.json:json:20240303
- Kotlin 1.9.0

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
