@echo off
REM ========================================
REM 神界：混沌边境 - Windows构建脚本
REM ========================================

echo ========================================
echo   神界：混沌边境 APK构建脚本
echo ========================================
echo.

REM 设置Unity路径（请根据实际安装路径修改）
set UNITY_PATH="C:\Program Files\Unity\Hub\Editor\2022.3.22f1\Editor\Unity.exe"

REM 检查Unity是否存在
if not exist %UNITY_PATH% (
    echo [错误] 未找到Unity编辑器！
    echo 请修改脚本中的UNITY_PATH变量为您的Unity安装路径
    echo.
    echo 常见路径：
    echo   C:\Program Files\Unity\Hub\Editor\2022.3.x\Editor\Unity.exe
    echo   C:\Program Files (x86)\Unity\Editor\Unity.exe
    pause
    exit /b 1
)

REM 设置项目路径
set PROJECT_PATH=%~dp0

REM 设置输出路径
set BUILD_PATH=%PROJECT_PATH%Builds

REM 创建输出目录
if not exist "%BUILD_PATH%" mkdir "%BUILD_PATH%"

echo [信息] 项目路径: %PROJECT_PATH%
echo [信息] 输出路径: %BUILD_PATH%
echo.

REM 构建APK
echo [开始构建] 正在构建Android APK...
echo.

%UNITY_PATH% ^
    -quit ^
    -batchmode ^
    -nographics ^
    -projectPath "%PROJECT_PATH%" ^
    -executeMethod JRPG.Editor.BuildTools.BuildAPK ^
    -buildTarget Android ^
    -logFile "%BUILD_PATH%\build.log"

REM 检查构建结果
if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo   构建成功！
    echo ========================================
    echo.
    echo APK位置: %BUILD_PATH%\DivineChaos.apk
    echo.
    
    REM 打开输出目录
    explorer "%BUILD_PATH%"
) else (
    echo.
    echo ========================================
    echo   构建失败！
    echo ========================================
    echo.
    echo 请查看日志: %BUILD_PATH%\build.log
    echo.
)

pause
