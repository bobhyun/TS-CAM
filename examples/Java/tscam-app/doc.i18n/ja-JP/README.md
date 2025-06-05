[English](../../README.md) | [한국어](../ko-KR/README.md) | 日本語 | [Tiếng Việt](../vi-VN/README.md)

# TS-CAM Java サンプル

この例は、TS-CAM Socket.IO クライアントの Java 実装です。以下の機能を順番に実行します：

1. 内部ネットワークに接続されたカメラの検出
2. 1番目のカメラの情報の読み取り（カメラログインが必要）
3. 車両ナンバー認識オプション付きスナップショット画像のリクエスト
4. リレーアウトプットの制御
5. イベントの待機
6. イベント監視リストの確認
7. イベント監視の停止

このプログラムは、各ステップでユーザーの入力を待ち、Socket.IO サーバーとの通信を通じてリアルタイムのカメラ制御とイベント受信を行います。

## プロジェクト構造

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

## 必要なライブラリ

- io.socket:socket.io-client:2.1.1
- org.json:json:20240303
- Java 17

## ビルドと実行方法

#### IntelliJ IDEA を使用する場合
1. IntelliJ IDEA でプロジェクトを開く
2. Gradle プロジェクトをインポート（Groovy DSL を使用）
3. プロジェクトをビルド
4. プログラムを実行

#### コマンドラインを使用する場合
1. プロジェクトディレクトリを開く
2. ターミナルを開く
3. プロジェクトをビルド
    ```bash
    # Windows
    gradlew build

    # Linux
    ./gradlew build
    ```
4. プログラムを実行
    ```bash
    # Windows
    gradlew --console=plain run

    # Linux
    ./gradlew --console=plain run
    ```
