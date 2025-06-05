[English](../../README.md) | 한국어 | [日本語](../ja-JP/README.md) | [Tiếng Việt](../vi-VN/README.md)

# TS-CAM Kotlin 예제

이 예제는 Kotlin으로 작성된 TS-CAM Socket.IO 클라이언트 예제입니다. 다음의 기능들을 순차적으로 실행합니다:

1. 내부망에 연결된 카메라 탐색
2. 첫번째 카메라 정보 읽기 (카메라 로그인 필요)
3. 스냅샷 이미지 요청 (차번인식 옵션 포함)
4. 릴레이 출력 제어
5. 이벤트 수신 대기
6. 이벤트 수신 대기 목록 확인
7. 이벤트 수신 종료

각 단계별로 사용자 입력을 기다리며 진행되며, Socket.IO 서버와의 통신을 통해 실시간으로 카메라 제어 및 이벤트 수신을 수행합니다.

## 프로젝트 구조

```
tscam-app/
├── src/
│   └── main/
│       └── kotlin/
│           └── com/example/tscam/
│               └── Main.kt
└── build.gradle.kts
```

## 필요한 라이브러리

- io.socket:socket.io-client:2.1.1
- org.json:json:20240303
- Kotlin 1.9.0

## 빌드 및 실행 방법

#### IntelliJ IDEA 사용 시
1. IntelliJ IDEA에서 프로젝트 열기
2. Gradle 프로젝트로 가져오기 (Groovy DSL 사용)
3. 프로젝트 빌드
4. 프로그램 실행

#### 명령행 사용 시
1. 프로젝트 디렉토리로 이동
2. 터미널 열기
3. 프로젝트 빌드
    ```bash
    # Windows
    gradlew build

    # Linux
    ./gradlew build
    ```
4. 프로그램 실행
    ```bash
    # Windows
    gradlew --console=plain run

    # Linux
    ./gradlew --console=plain run
    ```
