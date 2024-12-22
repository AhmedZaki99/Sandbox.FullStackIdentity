@echo off
setlocal EnableDelayedExpansion

REM Check if OpenSSL is available
where openssl >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo Error: OpenSSL is not installed or not in PATH
    exit /b 1
)

echo Generating self-signed certificate...
openssl req -newkey rsa:2048 -noenc -keyout data-protection.key -x509 -days 3650 -out data-protection.crt -subj "/CN=DataProtection"

echo Converting to PFX format...
openssl pkcs12 -inkey data-protection.key -in data-protection.crt -export -out data-protection.pfx -password pass:

echo Cleaning up temporary files...
del data-protection.key data-protection.crt

echo Certificate generation complete. Output file: data-protection.pfx
exit /b 0