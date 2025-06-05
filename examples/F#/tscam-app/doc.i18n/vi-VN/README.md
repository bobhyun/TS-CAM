[English](../../README.md) | [한국어](../ko-KR/README.md) | [日本語](../ja-JP/README.md) | Tiếng Việt

# TS-CAM F# Example

Ví dụ này là một triển khai của TS-CAM Socket.IO client bằng F#. Nó thực hiện các chức năng sau theo thứ tự:

1. Tìm kiếm camera trên mạng nội bộ
2. Đọc thông tin từ camera đầu tiên (yêu cầu đăng nhập camera)
3. Yêu cầu hình ảnh chụp màn hình với các tùy chọn nhận dạng biển số xe
4. Kiểm soát đầu ra relay
5. Chờ sự kiện
6. Kiểm tra danh sách theo dõi sự kiện
7. Dừng theo dõi sự kiện

Chương trình chờ đầu vào từ người dùng ở mỗi bước và thực hiện điều khiển camera và nhận sự kiện theo thời gian thực thông qua giao tiếp Socket.IO server.

## Cấu trúc dự án

```
tscam-app/
├── Program.fs
├── tscam-app.fsproj
└── tscam-app.sln
```

## Thư viện cần thiết

- SocketIOClient
- Newtonsoft.Json
- .NET 6.0
