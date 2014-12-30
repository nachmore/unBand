@set /p input=Enter Private Key cert password:

"C:\Program Files (x86)\Microsoft SDKs\Windows\v7.1A\Bin\signtool" sign /t http://timestamp.digicert.com /f %1 /p "%input%" %2

@IF NOT ERRORLEVEL 1 goto Exit

@pause

:Exit
pause
