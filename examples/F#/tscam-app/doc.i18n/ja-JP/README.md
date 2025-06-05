[English](../../README.md) | [한국어](../ko-KR/README.md) | 日本語 | [Tiếng Việt](../vi-VN/README.md)

# TS-CAM F# サンプル

このサンプルは、TS-CAM Socket.IO クライアントの F# 実装です。以下の機能を順次実行します：

1. 内部ネットワーク上のカメラの検索
2. 最初のカメラの情報の読み取り（カメラログインが必要）
3. 車両ナンバープレート認識オプションを含むスナップショット画像のリクエスト
4. リレーアウトプットの制御
5. イベントの待機
6. イベント監視リストの確認
7. イベント監視の停止

プログラムは各ステップでユーザー入力を待ち、Socket.IO サーバー通信を通じてリアルタイムのカメラ制御とイベント受信を行います。

## プロジェクト構造

```
tscam-app/
├── Program.fs
├── tscam-app.fsproj
└── tscam-app.sln
```

## 必要なライブラリ

- SocketIOClient
- Newtonsoft.Json
- .NET 6.0
