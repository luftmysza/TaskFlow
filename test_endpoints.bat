@echo off

:: === CONFIG (set BEFORE delayed expansion) ===
set BASE_URL=https://localhost:44346/taskflow
set USERNAME=GenericAdmin
call set PASSWORD=Password1!

:: === INIT (enable delayed expansion after sensitive vars) ===
setlocal ENABLEDELAYEDEXPANSION
set LOG_FILE=api_test_results.txt

:: === LOG HEADER ===
echo TaskFlow API Test Script > %LOG_FILE%
echo ============================ >> %LOG_FILE%

:: === 1. LOGIN ===
echo Logging in as %USERNAME%... >> %LOG_FILE%
curl -k -s -X POST "%BASE_URL%/auth/login" ^
  -H "Content-Type: application/json" ^
  -d "{\"userName\":\"%USERNAME%\", \"password\":\"%PASSWORD%\"}" > tmp_login.json

for /f "tokens=2 delims=:" %%a in ('findstr "bearer" tmp_login.json') do (
    set "RAW=%%a"
)

set "TOKEN=%RAW:~1,-2%"
del tmp_login.json

if "%TOKEN%"=="" (
    echo ❌ Failed to authenticate. >> %LOG_FILE%
    echo Script terminated. >> %LOG_FILE%
    exit /b 1
)

echo Token acquired >> %LOG_FILE%
echo Bearer token: %TOKEN% >> %LOG_FILE%
echo ============================ >> %LOG_FILE%

:: === 2. PROJECTS ===
echo [GET] /projects/list >> %LOG_FILE%
curl -k -s -w "\nStatus: %%{http_code}\n" "%BASE_URL%/projects/list" -H "Authorization: Bearer %TOKEN%" >> %LOG_FILE%
echo. >> %LOG_FILE%

echo [POST] /projects/create >> %LOG_FILE%
curl -k -s -w "\nStatus: %%{http_code}\n" -X POST "%BASE_URL%/projects/create" ^
  -H "Authorization: Bearer %TOKEN%" -H "Content-Type: application/json" ^
  -d "{\"projectKey\": \"testproj\", \"userNames\": [\"User1\"]}" >> %LOG_FILE%
echo. >> %LOG_FILE%

echo [GET] /projects/testproj >> %LOG_FILE%
curl -k -s -w "\nStatus: %%{http_code}\n" "%BASE_URL%/projects/testproj" -H "Authorization: Bearer %TOKEN%" >> %LOG_FILE%
echo. >> %LOG_FILE%

echo [POST] /projects/testproj/AddTask >> %LOG_FILE%
curl -k -s -w "\nStatus: %%{http_code}\n" -X POST "%BASE_URL%/projects/testproj/AddTask" ^
  -H "Authorization: Bearer %TOKEN%" -H "Content-Type: application/json" ^
  -d "{\"title\": \"New API Task\", \"description\": \"From curl\", \"assigneeUserName\": \"User1\"}" >> %LOG_FILE%
echo. >> %LOG_FILE%

:: === 3. TASKS ===
echo [GET] /tasks/mine >> %LOG_FILE%
curl -k -s -w "\nStatus: %%{http_code}\n" "%BASE_URL%/tasks/mine" -H "Authorization: Bearer %TOKEN%" >> %LOG_FILE%
echo. >> %LOG_FILE%

echo [GET] /tasks/DEMO-1/details >> %LOG_FILE%
curl -k -s -w "\nStatus: %%{http_code}\n" "%BASE_URL%/tasks/DEMO-1/details" -H "Authorization: Bearer %TOKEN%" >> %LOG_FILE%
echo. >> %LOG_FILE%

echo [PATCH] /tasks/DEMO-1/comment >> %LOG_FILE%
curl -k -s -w "\nStatus: %%{http_code}\n" -X PATCH "%BASE_URL%/tasks/DEMO-1/comment" ^
  -H "Authorization: Bearer %TOKEN%" -H "Content-Type: application/json" ^
  -d "\"Looks good from curl.\"" >> %LOG_FILE%
echo. >> %LOG_FILE%

:: === 4. CLOSE THE CREATED TASK ===
echo [PATCH] /tasks/testproj-1 (complete) >> %LOG_FILE%
curl -k -s -w "\nStatus: %%{http_code}\n" -X PATCH "%BASE_URL%/tasks/testproj-1" ^
  -H "Authorization: Bearer %TOKEN%" -H "Content-Type: application/json" ^
  -d "{\"operation\": \"complete\"}" >> %LOG_FILE%
echo. >> %LOG_FILE%

:: === 5. DELETE THE TEST PROJECT ===
echo [DELETE] /projects/testproj/delete >> %LOG_FILE%
curl -k -s -w "\nStatus: %%{http_code}\n" -X DELETE "%BASE_URL%/projects/testproj/delete" ^
  -H "Authorization: Bearer %TOKEN%" >> %LOG_FILE%
echo. >> %LOG_FILE%

:: === DONE ===
echo  All tests finished. See %LOG_FILE% for results.
pause
