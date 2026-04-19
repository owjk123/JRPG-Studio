@echo off
REM ========================================
REM JRPG-Studio GitHub推送脚本 (Windows)
REM ========================================

echo ========================================
echo   JRPG-Studio GitHub推送工具
echo ========================================
echo.

REM GitHub配置
set GITHUB_USER=owjk123
set REPO_NAME=JRPG-Studio

REM 检查gh CLI
where gh >nul 2>nul
if %ERRORLEVEL% neq 0 (
    echo [!] 需要安装GitHub CLI
    echo.
    echo 请访问: https://cli.github.com/
    pause
    exit /b 1
)

REM 检查是否已登录
gh auth status >nul 2>nul
if %ERRORLEVEL% neq 0 (
    echo [!] 需要登录GitHub
    echo.
    gh auth login
)

REM 检查仓库
echo.
echo [检查仓库状态...]
gh repo view %GITHUB_USER%/%REPO_NAME% >nul 2>nul
if %ERRORLEVEL% neq 0 (
    echo [!] 仓库不存在，正在创建...
    gh repo create %REPO_NAME% --public --description "神界：混沌边境 - 日式回合制RPG手游"
)

REM 添加远程仓库
git remote remove origin 2>nul
git remote add origin https://github.com/%GITHUB_USER%/%REPO_NAME%.git

REM 推送
echo.
echo [推送代码到GitHub...]
git push -u origin main

if %ERRORLEVEL% equ 0 (
    echo.
    echo ========================================
    echo   推送成功！
    echo ========================================
    echo.
    echo 仓库地址: https://github.com/%GITHUB_USER%/%REPO_NAME%
    echo.
    start https://github.com/%GITHUB_USER%/%REPO_NAME%
) else (
    echo.
    echo [!] 推送失败
)

pause
