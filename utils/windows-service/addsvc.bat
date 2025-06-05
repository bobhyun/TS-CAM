@echo off

REM tscam Service Installation Script

REM Please run as Administrator.

reg Query "HKLM\Hardware\Description\System\CentralProcessor\0" | find /i "x86" > NUL && set NSSM=win32\nssm.exe || set NSSM=win64\nssm.exe

REM Modify to the actual executable path
%NSSM% install tscam "C:\Program Files\TS-Solution\TS-ANPR\tsanpr-v3.0.0M\windowns-x86_64\tscam.exe"
%NSSM% set tscam AppExit Default Restart
%NSSM% set tscam AppRestartDelay 3000

REM Environment variable settings
REM HTTP port number
REM %NSSM% set tscam AppEnvironment TSCAM_HTTP_PORT=10000

REM Data storage directory
REM %NSSM% set tscam AppEnvironment "TSCAM_DATA_DIR=C:\ProgramData\TS-Solution\tscam\data"

REM Log file directory
REM %NSSM% set tscam AppEnvironment "TSCAM_LOG_DIR=C:\ProgramData\TS-Solution\tscam\log"

REM Remove milliseconds
REM %NSSM% set tscam AppEnvironment TSCAM_NO_MILLISECONDS=1

REM URI prefix for saved image file downloads
REM %NSSM% set tscam AppEnvironment TSCAM_URI_DATA_PATH_PREFIX=/site1"

REM Console log level (info, warn, error)
REM In production, set to warn or error to reduce write load
REM %NSSM% set tscam AppEnvironment TSCAM_LOG_LEVEL_CONSOLE=error

REM File log level (info, warn, error)
REM In production, set to warn or error to reduce write load
REM %NSSM% set tscam AppEnvironment TSCAM_LOG_LEVEL_FILE=error

REM Log file configuration (escaped with double quotes)
REM     maxSize: Maximum size of a single log file
REM     maxFiles: Number of log files to keep (automatically deleted after specified days)
REM     size: Maximum size of total log storage (oldest files are deleted when total size exceeds this value)
REM %NSSM% set tscam AppEnvironment "TSCAM_LOG_CONFIG={""maxSize"":""20m"",""maxFiles"":""31d"",""size"":""1024m""}"

REM Set if the license plate recognition engine is in a different location than tscam.exe
REM %NSSM% set tscam AppEnvironment "TSANPR=C:\Program Files\TS-Solution\TS-ANPR\tsanpr-v3.0.0M\windows-x86_64\tsanpr.dll"

REM TSANPR initialization params
REM %NSSM% set tscam AppEnvironment "TSANPR_COUNTRY=KR"
REM %NSSM% set tscam AppEnvironment "TSANPR_MIN_CHAR=4"
REM %NSSM% set tscam AppEnvironment "TSANPR_SYMBOL=full"
     
%NSSM% start tscam
%NSSM% status tscam

