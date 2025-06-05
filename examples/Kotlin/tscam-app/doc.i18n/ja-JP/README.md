[English](../../README.md) | [한국어](../ko-KR/README.md) | 日本語 | [Tiếng Việt](../vi-VN/README.md)

# TS-CAM Kotlin サンプル

この例は、Kotlinで書かれたTS-CAM Socket.IOクライアントの例です。以下の機能を順番に実行します：

1. 内部ネットワーク上のカメラを検出
2. 最初のカメラから情報を読み取り（カメラログインが必要）
3. ナンバープレート認識オプション付きのスナップショット画像をリクエスト
4. リレー出力を制御
5. イベントを待機
6. イベントウォッチリストを確認
7. イベントの監視を停止

プログラムは各ステップでユーザー入力を待機し、Socket.IOサーバーとの通信を通じてリアルタイムでカメラ制御とイベント受信を実行します。

## プロジェクト構造

```
tscam-app/
├── src/
│   └── main/
│       └── kotlin/
│           └── com/example/tscam/
│               └── Main.kt
└── build.gradle.kts
```

## 必要なライブラリ

- io.socket:socket.io-client:2.1.1
- org.json:json:20240303
- Kotlin 1.9.0

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
