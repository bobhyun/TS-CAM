[English](../../README.md) | [한국어](../ko-KR/README.md) | [日本語](../ja-JP/README.md) | Tiếng Việt

# Ví dụ TS-CAM C#

Đây là triển khai client Socket.IO TS-CAM bằng C#. Nó thực hiện tuần tự các chức năng sau:

1. Phát hiện camera trong mạng nội bộ
2. Đọc thông tin từ camera đầu tiên (yêu cầu đăng nhập camera)
3. Yêu cầu ảnh chụp với tùy chọn nhận dạng biển số xe
4. Điều khiển đầu ra rơ le
5. Chờ sự kiện
6. Kiểm tra danh sách theo dõi sự kiện
7. Dừng theo dõi sự kiện

Chương trình chờ đầu vào của người dùng ở mỗi bước và thực hiện điều khiển camera và nhận sự kiện theo thời gian thực thông qua giao tiếp máy chủ Socket.IO.

## Cấu trúc dự án

```
tscam-app/
├── Program.cs
├── tscam-app.csproj
└── tscam-app.sln
```

## Thư viện yêu cầu

- SocketIO4Net
- Newtonsoft.Json
- .NET 6.0
