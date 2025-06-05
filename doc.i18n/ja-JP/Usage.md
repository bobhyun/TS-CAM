[English](../../Usage.md) | [한국어](../ko-KR/Usage.md) | 日本語 | [Tiếng Việt](../vi-VN/Usage.md)

## インストールと実行

##### 1. ファイル構成
1. Windows
   ```sh
    tscam.exe   # TS-CAM 実行ファイル
    .env        # 環境変数設定ファイル
   ```

2. Linux
   ```sh
    tscam       # TS-CAM 実行ファイル
    .env        # 環境変数設定ファイル
   ```

##### 2. 環境変数
   環境変数を設定することで `tscam.exe` の動作を設定できます。
   環境変数は、サービス登録時のスクリプトファイルで設定するか、単純に `.env` ファイルで設定できます。
   `.env` ファイルは常に `tscam.exe` ファイルと同じディレクトリに配置する必要があります。

   ```sh
     TSCAM_HTTP_PORT=10000                    # リッスンするTCPポート番号
     #TSCAM_DATA_DIR=C:\Users\bob\tscam\data  # スナップショット画像が保存されるディレクトリ
     #TSCAM_LOG_DIR=C:\Users\bob\tscam\log    # ログが保存されるディレクトリ

     # 時間表示をミリ秒（デフォルト）から秒に変更する場合に使用
     #TSCAM_NO_MILLISECONDS=1

     # スナップショット画像のURLパスプレフィックス
     #TSCAM_URI_DATA_PATH_PREFIX=/site1

     # ナンバープレート認識エンジンがtscam.exeと異なる場所にある場合に設定
     #TSANPR=C:\Program Files\TS-Solution\TS-ANPR\tsanpr-v3.0.0M\windows-x86_64\tsanpr.dll

     # ログファイル設定（デフォルト）
     #	maxSize: 単一ログファイルの最大サイズ
     # 	maxFiles: 保持するログファイル数（指定日数経過後、古いファイルは自動的に削除）
     # 	size: ログストレージ全体の最大サイズ（合計ログサイズがこれを超えると、最も古いファイルから削除）
     # TSCAM_LOG_CONFIG={"maxSize":"20m","maxFiles":"31d","size":"1024m"}

     # ファイル保存ログレベル（info、warn、errorのいずれかを設定）
     TSCAM_LOG_LEVEL_FILE=info        # すべてのログを保存
     # コンソール出力ログレベル（info、warn、errorのいずれかを設定）
     TSCAM_LOG_LEVEL_CONSOLE=info     # すべてのログを出力
   ```

##### 3. 開発環境
   開発環境では、コンソールにリアルタイムで動作状況が表示されるように設定すると便利です。

   - 環境変数の編集
     実行環境は環境変数で設定できるため、まず `.env` ファイルを開発環境に合わせて編集します。オペレーティングシステムの環境変数設定も使用できますが、開発用には単純に `.env` ファイルを使用することをお勧めします。
     ログは別途設定します。

     ```sh
     # ファイル保存ログレベル（info、warn、errorのいずれかを設定）
     TSCAM_LOG_LEVEL_FILE=info        # すべてのログを保存
     # コンソール出力ログレベル（info、warn、errorのいずれかを設定）
     TSCAM_LOG_LEVEL_CONSOLE=info     # すべてのログを出力
     ```

   - TS-CAMの実行
     コンソールウィンドウを開き、tscamを実行します。
     ```
     C:\Program Files\TS-Solution\TS-ANPR\tsanpr-v3.0.0M\windows-x86_64> tscam
     tscam v0.1.0
     log= C:\Users\bob\tscam\log
     data= C:\Users\bob\tscam\data
     2024-08-09 16:36:35.201 info: Process started { pid: 18764 }
     2024-08-09 16:36:35.446 info: os_name=win32, arch_name=x64
     2024-08-09 16:36:35.448 info: LIB_PATH= 'C:\Program Files\TS-Solution\TS-ANPR\tsanpr-v3.0.0M\windows-x86_64\tsanpr.dll'
     2024-08-09 16:36:36.547 info: TS-ANPR v2.4.0 is ready
     2024-08-09 16:36:36.570 info: listening 127.0.0.1:10000
     ```
   - TS-CAMの終了
     コンソールウィンドウで `Ctrl+C` を入力してプログラムを終了します。
     ```
     2024-08-09 16:37:02.240 info: SIGINT received. Shutting down gracefully
     2024-08-09 16:37:02.243 info: Closing server
     2024-08-09 16:37:03.245 info: Server closed
     2024-08-09 16:37:03.248 info: Socket.IO server closed
     2024-08-09 16:37:03.250 info: Process terminated { pid: 18764 }
     ```

##### 4. 本番環境
1. Windows
   耐障害性のため、本番環境ではシステムサービスとして実行します。

   - サービスの登録
     まず、`tscam` および `TS-ANPR` ディレクトリを目的の場所にコピーします。次に、システムサービスとして登録するために、`utils/windows-service/addsvc.bat` ファイルの必要な部分を修正します。
     例えば、`C:\Program Files\TS-Solution\TS-ANPR\` にコピーした場合、次のように修正できます。

     ```batch
     @echo off

     REM tscam サービスインストールスクリプト

     REM 管理者として実行してください。

     reg Query "HKLM\Hardware\Description\System\CentralProcessor\0" | find /i "x86" > NUL && set NSSM=win32\nssm.exe || set NSSM=win64\nssm.exe

     REM 実際の実行可能ファイルのパスに修正してください。
     %NSSM% install tscam "C:\Program Files\TS-Solution\TS-ANPR\tsanpr-v3.0.0M\windows-x86_64\tscam.exe"
     %NSSM% set tscam AppExit Default Restart
     %NSSM% set tscam AppRestartDelay 3000

     REM 必要な環境変数を設定
     REM HTTPポート番号
     REM %NSSM% set tscam AppEnvironment TSCAM_HTTP_PORT=10000

     REM データ保存ディレクトリ
     REM %NSSM% set tscam AppEnvironment "TSCAM_DATA_DIR=C:\ProgramData\TS-Solution\tscam\data"

     REM ログファイルディレクトリ
     REM %NSSM% set tscam AppEnvironment "TSCAM_LOG_DIR=C:\ProgramData\TS-Solution\tscam\log"

     REM ミリ秒単位の削除
     REM %NSSM% set tscam AppEnvironment TSCAM_NO_MILLISECONDS=1

     REM 保存画像ファイルのダウンロードパスプレフィックス
     REM %NSSM% set tscam AppEnvironment TSCAM_URI_DATA_PATH_PREFIX=/site1"

     REM コンソールログレベル (info, warn, error)
     REM 本番環境では、書き込み負荷を軽減するために warn または error に設定します。
     REM %NSSM% set tscam AppEnvironment TSCAM_LOG_LEVEL_CONSOLE=error

     REM ファイルログレベル (info, warn, error)
     REM 本番環境では、書き込み負荷を軽減するために warn または error に設定します。
     REM %NSSM% set tscam AppEnvironment TSCAM_LOG_LEVEL_FILE=error

     REM ログファイル設定 (二重引用符でエスケープ)
     REM maxSize: 単一ログファイルの最大サイズ
     REM maxFiles: 保持するログファイル数 (指定日数経過後、古いファイルは自動的に削除)
     REM size: ログストレージ全体の最大サイズ (合計ログサイズがこれを超えると、最も古いファイルから削除)
     REM %NSSM% set tscam AppEnvironment "TSCAM_LOG_CONFIG={{}}\"maxSize\":\"20m\",\"maxFiles\":\"31d\",\"size\":\"1024m\"{{}}}}"

     REM ナンバープレート認識エンジンがtscam.exeと異なる場所にある場合に設定
     REM %NSSM% set tscam AppEnvironment "TSANPR=C:\Program Files\TS-Solution\TS-ANPR\tsanpr-v3.0.0M\windows-x86_64\tsanpr.dll"

     %NSSM% start tscam
     %NSSM% status tscam

     ```

     修正した `addsvc.bat` ファイルを保存後、管理者として実行して登録を完了します。

   - サービスの削除
     登録された `tscam` サービスを削除するには、`utils/windows-service/rmsvc.bat` を管理者として実行します。

2. Linux

   - サービスの登録
     システムサービスとして登録するには、`utils/linux-service/addsvc.sh` ファイルの必要な部分を修正します。
     例えば、`/var/tsanpr/` にコピーした場合、次のように修正できます。

   ```sh
   #!/bin/bash

   # tscam サービス登録スクリプト

   echo "# tscam 設定

   [Unit]
   Description=tscam (ONVIF Camera Broker)
   After=network.target

   [Service]
   # HTTPポート番号
   # Environment=\"TSCAM_HTTP_PORT=10000\"

   # データ保存ディレクトリ
   # Environment=\"TSCAM_DATA_DIR=/var/tscam/data\"

   # ログファイルディレクトリ
   # Environment=\"TSCAM_LOG_DIR=/var/tscam/log\"

   # ミリ秒単位の削除
   # Environment=\"TSCAM_NO_MILLISECONDS=1\"

   # 保存画像ファイルのダウンロードパスプレフィックス
   # Environment=\"TSCAM_URI_DATA_PATH_PREFIX=/site1\"

   # コンソールログレベル (info, warn, error)
   # Environment=\"TSCAM_LOG_LEVEL_CONSOLE=error\"

   # ファイルログレベル (info, warn, error)
   # Environment=\"TSCAM_LOG_LEVEL_FILE=error\"

   # ログファイル設定
   # 	maxSize: 単一ログファイルの最大サイズ
   # 	maxFiles: 保持するログファイル数 (指定日数経過後、古いファイルは自動的に削除)
   # 	size: ログストレージ全体の最大サイズ (合計ログサイズがこれを超えると、最も古いファイルから削除)
   # Environment=\"TSCAM_LOG_CONFIG={\\\"maxSize\\\":\\\"20m\\\",\\\"maxFiles\\\":\\\"31d\\\",\\\"size\\\":\\\"1024m\\\"}\"

   # ナンバープレート認識エンジンがtscamと異なる場所にある場合に設定
   # Environment=\"TSANPR=/var/tsanpr/tsanpr-v3.0.0M/linux-x86_64/libtsanpr.so\"

   # TSANPR 初期化パラメータ
   # Environment=\"TSANPR_COUNTRY=KR\"
   # Environment=\"TSANPR_MIN_CHAR=4\"
   # Environment=\"TSANPR_SYMBOL=full\"

   # 必須項目
   WorkingDirectory=/var/tsanpr

   # tscam 実行ファイルパス
   ExecStart=/var/tsanpr/tsanpr-v3.0.0M/linux-x86_64/tscam

   # 停止した場合、3秒後に自動的に再起動
   Restart=always
   RestartSec=3
   LimitNOFILE=400000
   Type=simple

   [Install]
   WantedBy=multi-user.target
   " > /etc/systemd/system/tscam.service
   
   sudo systemctl daemon-reload
   sudo systemctl enable tscam
   sudo systemctl restart tscam
   ```

   - サービスの削除
     登録された `tscam` サービスを削除するには、`utils/linux-service/rmsvc.sh` を管理者として実行します。