@ECHO OFF

REM Get Path parameter
IF "%1" == "" (
  REM Report invalid invocation of this script.
  ECHO Invalid Syntax. The expected syntax is: 
  ECHO PathAccessCheckRunStep.cmd ^<path^>
  EXIT /B -1
)

SET PathToCheck="%1"
SET Result=-1

REM Check that Path exists and is writeable.
IF NOT EXIST %PathToCheck% (
  SET Result=2
) ELSE (
  REM Path exists and current user can read it, so create empty file to check if writeable.
  COPY /y NUL %1\pathaccess.check.txt.temporary >NUL
  IF NOT EXIST %1\pathaccess.check.txt.temporary (
    SET Result=1
  ) ELSE (
    REM File creation succeeded, now delete it.
    DEL %1\pathaccess.check.txt.temporary
    SET Result=0
  )
)

REM Print result to stdout.
IF %Result%==0 (
  ECHO Success
) ELSE IF %Result%==1 (
  ECHO Path Access Check failed: Path %PathToCheck% exists but the provided user credential does not have write permissions to it
) ELSE IF %Result%==2 (
  ECHO Path Access Check failed: Path %PathToCheck% does not exist or the provided user credential does not have access to it
) ELSE (
  ECHO Path Access Check failed: Unexpected error checking path %PathToCheck%
)

EXIT /B %Result%