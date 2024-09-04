@echo off

REM tscam 서비스 삭제 스크립트

REM 관리자 모드로 실행하세요.

reg Query "HKLM\Hardware\Description\System\CentralProcessor\0" | find /i "x86" > NUL && set NSSM=win32\nssm.exe || set NSSM=win64\nssm.exe

%NSSM% stop tscam
%NSSM% remove tscam confirm
