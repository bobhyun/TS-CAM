[English](../../README.md) | [한국어](../ko-KR/README.md) | [日本語](../ja-JP/README.md) | Tiếng Việt

# TS-CAM Node.js Example

Đây là ví dụ về việc triển khai Node.js cho TS-CAM Socket.IO client. Nó thực hiện các chức năng sau theo thứ tự:

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
├── index.js
└── package.json
```

## Thư viện cần thiết

- socket.io-client: ^4.7.5
- readline: ^1.3.0
- Node.js: 18 trở lên

## Cách chạy

1. Cài đặt các thư viện cần thiết:
   ```bash
   npm install
   ```
2. Chạy chương trình:
   ```bash
   npm start
   ```
