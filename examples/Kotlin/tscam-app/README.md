English | [한국어](doc.i18n/ko-KR/README.md) | [日本語](doc.i18n/ja-JP/README.md) | [Tiếng Việt](doc.i18n/vi-VN/README.md)

# TS-CAM Kotlin Example

This example is a Kotlin implementation of the TS-CAM Socket.IO client. It sequentially performs the following functions:

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
├── src/
│   └── main/
│       └── kotlin/
│           └── com/example/tscam/
│               └── Main.kt
└── build.gradle.kts
```

## Required Libraries

- io.socket:socket.io-client:2.1.1
- org.json:json:20240303
- Kotlin 1.9.0

## How to Build and Run

#### Using IntelliJ IDEA
1. Open the project in IntelliJ IDEA
2. Import the Gradle project (using Groovy DSL)
3. Build the project
4. Run the program

#### Using Command Line
1. Open the project directory
2. Open the terminal
3. Build the project
    ```bash
    # Windows
    gradlew build

    # Linux
    gradlew build
    ```
4. Run the program
    ```bash
    # Windows
    gradlew --console=plain run

    # Linux
    ./gradlew --console=plain run
    ```