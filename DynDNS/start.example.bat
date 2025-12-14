@echo off
REM Example start script for DynDNS application
REM Copy this file to start.bat and fill in your credentials

REM ====================================================================
REM REQUIRED PARAMETERS
REM ====================================================================

REM Your Netcup API Key (generate in CCP under "Stammdaten/API")
set API_KEY=your_api_key_here

REM Your Netcup API Password
set API_PASSWORD=your_api_password_here

REM Your Netcup Customer Number
set CUSTOMER_NUMBER=123456

REM Your domain name
set DOMAIN=example.com


REM ====================================================================
REM OPTIONAL PARAMETERS
REM ====================================================================

REM Execution interval in minutes (default: 60)
set EXECUTION_INTERVAL=60

REM Hostnames to ignore (space-separated, wrap each with -i)
REM Leave empty to update ALL hosts (default behavior)
REM Example: set IGNORED_HOSTS=-i * -i @ -i autoconfig -i mail -i webmail
set IGNORED_HOSTS=

REM Enable ticking clock (true/false)
set TICKING_CLOCK=false

REM Save configuration to file (true/false)
REM WARNING: Stores credentials in plain text!
set SAVE_CONFIG=false


REM ====================================================================
REM START APPLICATION
REM ====================================================================

echo Starting DynDNS application...
echo.

set CMD=DynDNS.exe --apiKey "%API_KEY%" --api-password "%API_PASSWORD%" --customer-number %CUSTOMER_NUMBER% --domain "%DOMAIN%" --execution-interval-in-minutes %EXECUTION_INTERVAL% %IGNORED_HOSTS%

if /i "%TICKING_CLOCK%"=="true" set CMD=%CMD% --ticking-clock
if /i "%SAVE_CONFIG%"=="true" set CMD=%CMD% --save-config

%CMD%

pause
