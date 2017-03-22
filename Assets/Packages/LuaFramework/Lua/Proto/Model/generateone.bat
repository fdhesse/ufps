@ECHO OFF

SET PROTOC=%~dp0\..\..\..\..\tools\protobuf\protoc
SET CURRDIR=%~dp0

SET CSDIR=%~dp0\..\..\..\..\server\Game.KingsMan.Model\
SET LUADIR=%~dp0\..\..\..\..\client\Tactics\Data\Scripts\Generated\Model

IF NOT EXIST %CSDIR% MD %CSDIR%

FOR %%i in (%*) DO (
    ECHO generate cs and cpp for %%~nxi
    %PROTOC% -I=. -I=..\Common --csharp_out=%CSDIR% --lua_out=%LUADIR% %%~nxi
)

PAUSE