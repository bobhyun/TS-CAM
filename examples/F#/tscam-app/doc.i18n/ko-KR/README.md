[English](../../README.md) | 한국어 | [日本語](../ja-JP/README.md) | [Tiếng Việt](../vi-VN/README.md)

# TS-CAM F# 예제

이 예제는 TS-CAM Socket.IO 클라이언트의 F# 구현입니다. 다음 기능들을 순차적으로 수행합니다:

1. 내부 네트워크의 카메라 검색
2. 첫 번째 카메라 정보 읽기 (카메라 로그인 필요)
3. 차량 번호 인식 옵션을 포함한 스냅샷 이미지 요청
4. 릴레이 출력 제어
5. 이벤트 대기
6. 이벤트 감시 목록 확인
7. 이벤트 감시 중지

프로그램은 각 단계에서 사용자 입력을 기다리며, Socket.IO 서버 통신을 통해 실시간 카메라 제어와 이벤트 수신을 수행합니다.

## 프로젝트 구조

```
tscam-app/
├── Program.fs
├── tscam-app.fsproj
└── tscam-app.sln
```

## 필요한 라이브러리

- SocketIOClient
- Newtonsoft.Json
- .NET 6.0
