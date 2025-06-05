English | [한국어](doc.i18n/ko-KR/Usage.md) | [日本語](doc.i18n/ja-JP/Usage.md) | [Tiếng Việt](doc.i18n/vi-VN/Usage.md)

## Installation and Running

##### 1. File Structure
1. Windows
   ```sh
    tscam.exe   # TS-CAM executable file
    .env        # Environment variable configuration file
   ```

2. Linux
   ```sh
    tscam       # TS-CAM executable file
    .env        # Environment variable configuration file
   ```

##### 2. Environment Variables
   You can configure the behavior of `tscam.exe` by setting environment variables.
   Environment variables can be set in the script file when registering the service or simply in the `.env` file.
   The `.env` file must always be located in the same directory as the `tscam.exe` file.

   ```sh
     TSCAM_HTTP_PORT=10000                    # TCP port number for listening
     #TSCAM_DATA_DIR=C:\Users\bob\tscam\data  # Directory where snapshot images will be saved
     #TSCAM_LOG_DIR=C:\Users\bob\tscam\log    # Directory where logs will be saved

     # Use if you want to change time display from milliseconds (default) to seconds
     #TSCAM_NO_MILLISECONDS=1

     # URL path prefix for snapshot images
     #TSCAM_URI_DATA_PATH_PREFIX=/site1

     # Set if the license plate recognition engine is in a different location than tscam.exe
     # TSANPR=C:\Program Files\TS-Solution\TS-ANPR\tsanpr-v3.0.0M\windows-x86_64\tsanpr.dll

     #TSANPR initialization params
     #TSANPR_COUNTRY=KR
     #TSANPR_MIN_CHAR=4
     #TSANPR_SYMBOL=full

     # Log file settings (default)
     #	maxSize: Maximum size of a single log file
     # 	maxFiles: Number of log files to keep (older files are automatically deleted after the specified number of days)
     # 	size: Maximum size of the entire log storage (if total log size exceeds this, oldest files are deleted first)
     # TSCAM_LOG_CONFIG={"maxSize":"20m","maxFiles":"31d","size":"1024m"}

     # File save log level (set to one of: info, warn, error)
     TSCAM_LOG_LEVEL_FILE=info        # Save all logs
     # Console output log level (set to one of: info, warn, error)
     TSCAM_LOG_LEVEL_CONSOLE=info     # Output all logs
   ```

##### 3. Development Environment
   It is convenient to configure the development environment so that the operating status is displayed in real-time on the console.

   - Edit Environment Variables
     Since the execution environment can be configured using environment variables, first edit the `.env` file to suit your development environment. You can use the operating system's environment variable settings, but for development, it is recommended to simply use the `.env` file.
     Logs are separate.

     ```sh
     # File save log level (set to one of: info, warn, error)
     TSCAM_LOG_LEVEL_FILE=info        # Save all logs
     # Console output log level (set to one of: info, warn, error)
     TSCAM_LOG_LEVEL_CONSOLE=info     # Output all logs
     ```

   - Run TS-CAM
     Open a console window and run tscam.
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
   - Terminate TS-CAM
     Enter `Ctrl+C` in the console window to terminate the program.
     ```
     2024-08-09 16:37:02.240 info: SIGINT received. Shutting down gracefully
     2024-08-09 16:37:02.243 info: Closing server
     2024-08-09 16:37:03.245 info: Server closed
     2024-08-09 16:37:03.248 info: Socket.IO server closed
     2024-08-09 16:37:03.250 info: Process terminated { pid: 18764 }
     ```

##### 4. Production Environment
1. Windows
   For resilience, run as a system service in a production environment.

   - Register Service
     First, copy the `tscam` and `TS-ANPR` directories to your desired location. Then, to register it as a system service, modify the necessary parts of the `utils/windows-service/addsvc.bat` file.
     For example, if you copied it to `C:\Program Files\TS-Solution\TS-ANPR\`, you can modify it as follows:

     ```batch
     @echo off

     REM tscam service installation script

     REM Run as administrator.

     reg Query "HKLM\Hardware\Description\System\CentralProcessor\0" | find /i "x86" > NUL && set NSSM=win32\nssm.exe || set NSSM=win64\nssm.exe

     REM Modify to the actual executable path.
     %NSSM% install tscam "C:\Program Files\TS-Solution\TS-ANPR\tsanpr-v3.0.0M\windows-x86_64\tscam.exe"
     %NSSM% set tscam AppExit Default Restart
     %NSSM% set tscam AppRestartDelay 3000

     REM Set necessary environment variables
     REM HTTP port number
     REM %NSSM% set tscam AppEnvironment TSCAM_HTTP_PORT=10000

     REM Data storage directory
     REM %NSSM% set tscam AppEnvironment "TSCAM_DATA_DIR=C:\ProgramData\TS-Solution\tscam\data"

     REM Log file directory
     REM %NSSM% set tscam AppEnvironment "TSCAM_LOG_DIR=C:\ProgramData\TS-Solution\tscam\log"

     REM Delete milliseconds unit
     REM %NSSM% set tscam AppEnvironment TSCAM_NO_MILLISECONDS=1

     REM Saved image file download path prefix
     REM %NSSM% set tscam AppEnvironment TSCAM_URI_DATA_PATH_PREFIX=/site1"

     REM Console log level (info, warn, error)
     REM In a production environment, set to warn or error to reduce write load.
     REM %NSSM% set tscam AppEnvironment TSCAM_LOG_LEVEL_CONSOLE=error

     REM File log level (info, warn, error)
     REM In a production environment, set to warn or error to reduce write load.
     REM %NSSM% set tscam AppEnvironment TSCAM_LOG_LEVEL_FILE=error

     REM Log file settings (escape with double quotes)
     REM maxSize: Maximum size of a single log file
     REM maxFiles: Number of log files to keep (older files are automatically deleted after the specified number of days)
     REM size: Maximum size of the entire log storage (if total log size exceeds this, oldest files are deleted first)
     REM %NSSM% set tscam AppEnvironment "TSCAM_LOG_CONFIG={{""maxSize"":""20m"",""maxFiles"":""31d"",""size"":""1024m""}}"

     REM Set if the license plate recognition engine is in a different location than tscam.exe
     REM %NSSM% set tscam AppEnvironment "TSANPR=C:\Program Files\TS-Solution\TS-ANPR\tsanpr-v3.0.0M\windows-x86_64\tsanpr.dll"

     REM TSANPR initialization params
     REM %NSSM% set tscam AppEnvironment "TSANPR_COUNTRY=KR"
     REM %NSSM% set tscam AppEnvironment "TSANPR_MIN_CHAR=4"
     REM %NSSM% set tscam AppEnvironment "TSANPR_SYMBOL=full"

     %NSSM% start tscam
     %NSSM% status tscam

     ```

     After saving the modified `addsvc.bat` file, run it as an administrator to complete the registration.

   - Remove Service
     To remove the registered `tscam` service, run `utils/windows-service/rmsvc.bat` as an administrator.

2. Linux

   - Register Service
     To register as a system service, modify the necessary parts of the `utils/linux-service/addsvc.sh` file.
     For example, if you copied it to `/var/tsanpr/`, you can modify it as follows:

   ```sh
   #!/bin/bash

   # tscam service registration script

   echo "# tscam configuration

   [Unit]
   Description=tscam (ONVIF Camera Broker)
   After=network.target

   [Service]
   # HTTP port number
   # Environment=\"TSCAM_HTTP_PORT=10000\"

   # Data storage directory
   # Environment=\"TSCAM_DATA_DIR=/var/tscam/data\"

   # Log file directory
   # Environment=\"TSCAM_LOG_DIR=/var/tscam/log\"

   # Delete milliseconds unit
   # Environment=\"TSCAM_NO_MILLISECONDS=1\"

   # Saved image file download path prefix
   # Environment=\"TSCAM_URI_DATA_PATH_PREFIX=/site1\"

   # Console log level (info, warn, error)
   # Environment=\"TSCAM_LOG_LEVEL_CONSOLE=error\"

   # File log level (info, warn, error)
   # Environment=\"TSCAM_LOG_LEVEL_FILE=error\"

   # Log file settings
   # 	maxSize: Maximum size of a single log file
   # 	maxFiles: Number of log files to keep (older files are automatically deleted after the specified number of days)
   # 	size: Maximum size of the entire log storage (if total log size exceeds this, oldest files are deleted first)
   # Environment=\"TSCAM_LOG_CONFIG={\\\"maxSize\\\":\\\"20m\\\",\\\"maxFiles\\\":\\\"31d\\\",\\\"size\\\":\\\"1024m\\\"}\"

   # Set if the license plate recognition engine is in a different location than tscam
   # Environment=\"TSANPR=/var/tsanpr/tsanpr-v3.0.0M/linux-x86_64/libtsanpr.so\"

   # TSANPR initialization params
   # Environment=\"TSANPR_COUNTRY=KR\"
   # Environment=\"TSANPR_MIN_CHAR=4\"
   # Environment=\"TSANPR_SYMBOL=full\"

   # Required item
   WorkingDirectory=/var/tsanpr

   # tscam executable file path
   ExecStart=/var/tsanpr/tsanpr-v3.0.0M/linux-x86_64/tscam

   # Automatically restart after 3 seconds if it dies
   Restart=always
   RestartSec=3
   LimitNOFILE=400000
   Type=simple

   [Install]
   WantedBy=multi-user.target
   " > ~/.tmp.tscam.service
   sudo mv ~/.tmp.tscam.service /etc/systemd/system/tscam.service

   sudo systemctl daemon-reload
   sudo systemctl enable tscam
   sudo systemctl restart tscam
   ```

   - Remove Service
     To remove the registered `tscam` service, run `utils/linux-service/rmsvc.sh` as an administrator.
