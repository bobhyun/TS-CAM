[English](../../README.md) | [한국어](../ko-KR/README.md) | 日本語 | [Tiếng Việt](../vi-VN/README.md)

# TS-CAM VB.NET サンプル

この例は、TS-CAM Socket.IOクライアントのVB.NET実装です。以下の機能を順番に実行します：

1. 内部ネットワーク上のカメラを検出
2. 最初のカメラから情報を読み取り（カメラへのログインが必要）
3. ナンバープレート認識オプション付きでスナップショット画像をリクエスト
4. リレー出力を制御
5. イベントを待機
6. イベントウォッチリストを確認
7. イベント監視を停止

プログラムは各ステップでユーザー入力を待機し、Socket.IOサーバー通信を通じてリアルタイムでカメラ制御とイベント受信を行います。

## プロジェクト構成

```
tscam-app/
├── Program.vb
├── tscam-app.vbproj
└── tscam-app.sln
```

## 必要なライブラリ

- SocketIoClientDotNet（または互換性のある .NET Socket.IO クライアント）
- Newtonsoft.Json
- .NET 6.0 以上 