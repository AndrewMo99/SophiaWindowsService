set SERVICE_NAME=SophiaWindowsService

sc stop %SERVICE_NAME%
sc delete %SERVICE_NAME%

installutil .\SophiaWindowsService\bin\Release\SophiaWindowsService.exe

sc config %SERVICE_NAME% start= demand