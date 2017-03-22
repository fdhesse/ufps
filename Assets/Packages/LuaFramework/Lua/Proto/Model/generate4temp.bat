@ECHO OFF

SET PROTOC=%~dp0\..\..\..\..\tools\protobuf\protoc2
SET CURRDIR=%~dp0

IF NOT EXIST %CD%\..\..\ModelCsT MD %CD%\..\..\ModelCsT

FOR %%i in (*.proto) DO (
    ECHO generate cs and cpp for %%~nxi
    %PROTOC% -I=. -I=..\Common --csharp_out=..\..\ModelCsT %%~nxi
)
