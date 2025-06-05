English | [한국어](doc.i18n/ko-KR/README.md) | [日本語](doc.i18n/ja-JP/README.md) | [Tiếng Việt](doc.i18n/vi-VN/README.md)

# TS-CAM VB.NET Example

This example is a VB.NET implementation of the TS-CAM Socket.IO client. It sequentially performs the following functions:

1. Discover cameras on the internal network
2. Read information from the first camera (camera login required)
3. Request snapshot image with license plate recognition options
4. Control relay output
5. Wait for events
6. Check event watch list
7. Stop event watching

The program waits for user input at each step and performs real-time camera control and event reception through Socket.IO server communication.

## Project Structure

```
tscam-app/
├── Program.vb
├── tscam-app.vbproj
└── tscam-app.sln
```

## Required Libraries

- SocketIoClientDotNet (or compatible .NET Socket.IO client)
- Newtonsoft.Json
- .NET 6.0 or later

## How to Run

1. Open the project in Visual Studio
2. Build the project
3. Run the program 