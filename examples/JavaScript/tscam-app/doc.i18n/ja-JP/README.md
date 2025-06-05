[English](../../README.md) | [한국어](../ko-KR/README.md) | 日本語 | [Tiếng Việt](../vi-VN/README.md)

# TS-CAM Node.js Example

この例は、TS-CAM Socket.IO クライアントの Node.js 実装です。以下の機能を順番に実行します：

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
├── index.js
└── package.json
```

## 必要なライブラリ

- socket.io-client: ^4.7.5
- readline: ^1.3.0
- Node.js: 18 以上

## 実行方法

1. 依存関係のインストール:
   ```bash
   npm install
   ```
2. プログラムの実行:
   ```bash
   npm start
   ```
