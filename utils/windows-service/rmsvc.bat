@echo off

REM tscam Service Removal Script

REM Please run as Administrator.

reg Query "HKLM\Hardware\Description\System\CentralProcessor\0" | find /i "x86" > NUL && set NSSM=win32\nssm.exe || set NSSM=win64\nssm.exe

%NSSM% stop tscam
%NSSM% remove tscam confirm
