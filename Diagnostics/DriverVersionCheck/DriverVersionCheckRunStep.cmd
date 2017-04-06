@ECHO off

:: Get parameter
IF "%1" == "" GOTO :InvalidSyntax
SET Param=""
SET Value=""
for /f "delims=: tokens=1-2" %%i IN ("%1") DO (
  SET Param=%%i
  SET Value=%%j
)
IF NOT %Param% == -Network GOTO :InvalidSyntax
IF %Value% == "" GOTO :InvalidSyntax

:: Call DriverVersionCheck.ps1
powershell.exe -Command "& {Add-PsSnapIn Microsoft.HPC; Set-ExecutionPolicy Bypass; \\%CCP_SCHEDULER%\c$\Diag\DriverVersionCheck\DriverVersionCheck.ps1 %1}"
GOTO :eof

:InvalidSyntax
ECHO Invalid Syntax. The expected syntax is: 
ECHO DriverVersionCheckRunStep.cmd -Network:^<name^>

:eof