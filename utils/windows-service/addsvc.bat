@echo off

REM tscam 서비스 설치 스크립트

REM 관리자 모드로 실행하세요.

reg Query "HKLM\Hardware\Description\System\CentralProcessor\0" | find /i "x86" > NUL && set NSSM=win32\nssm.exe || set NSSM=win64\nssm.exe

REM 실제 실행파일의 경로로 수정하세요.
%NSSM% install tscam "C:\Program Files\TS-Solution\TS-ANPR\tsanpr-KR-v2.4.0M\windowns-x86_64\tscam.exe"
%NSSM% set tscam AppExit Default Restart
%NSSM% set tscam AppRestartDelay 3000

REM 필요한 환경변수 설정
REM HTTP 포트 번호
REM %NSSM% set tscam AppEnvironment TSCAM_HTTP_PORT=10000

REM 데이터 저장 디렉토리
REM %NSSM% set tscam AppEnvironment "TSCAM_DATA_DIR=C:\ProgramData\TS-Solution\tscam\data"

REM 로그 파일 디렉토리
REM %NSSM% set tscam AppEnvironment "TSCAM_LOG_DIR=C:\ProgramData\TS-Solution\tscam\log"

REM 밀리초 단위 삭제
REM %NSSM% set tscam AppEnvironment TSCAM_NO_MILLISECONDS=1

REM 저장 이미지 파일 다운로드 경로 prefix
REM %NSSM% set tscam AppEnvironment TSCAM_URI_DATA_PATH_PREFIX=/site1"

REM 콘솔 로그 레벨 (info, warn, error)
REM 운영 환경에서는 쓰기 부하를 줄이려면 warm, error 수순으로 설정합니다.
REM %NSSM% set tscam AppEnvironment TSCAM_LOG_LEVEL_CONSOLE=error

REM 파일 로그 레벨 (info, warn, error)
REM 운영 환경에서는 쓰기 부하를 줄이려면 warm, error 수순으로 설정합니다.
REM %NSSM% set tscam AppEnvironment TSCAM_LOG_LEVEL_FILE=error

REM 로그 파일 설정 (따옴표 두개로 이스케이프 처리)
REM 	maxSize: 로그 파일 하나의 최대 크기
REM 	maxFiles: 보관할 로그 파일 수 (지정한 날짜가 지나면 자동 삭제됨)
REM 	size: 전체 로그 저장소의 최대 크기 (총 로그 크기가 설정한 값을 초과하면 가장 오래된 파일부터 삭제됨)  
REM %NSSM% set tscam AppEnvironment "TSCAM_LOG_CONFIG={""maxSize"":""20m"",""maxFiles"":""31d"",""size"":""1024m""}"

REM 차번인식엔진이 tscam.exe와 다른 위치에 있는 경우 설정합니다.
REM %NSSM% set tscam AppEnvironment "TSANPR=C:\Program Files\TS-Solution\TS-ANPR\tsanpr-KR-v2.4.0M\windows-x86_64\tsanpr.dll"

%NSSM% start tscam
%NSSM% status tscam

