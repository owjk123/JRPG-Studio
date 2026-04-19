#!/bin/bash
# ========================================
# 神界：混沌边境 - macOS/Linux构建脚本
# ========================================

echo "========================================"
echo "  神界：混沌边境 APK构建脚本"
echo "========================================"
echo ""

# 设置Unity路径（请根据实际安装路径修改）
if [[ "$OSTYPE" == "darwin"* ]]; then
    # macOS
    UNITY_PATH="/Applications/Unity/Hub/Editor/2022.3.22f1/Unity.app/Contents/MacOS/Unity"
    if [ ! -f "$UNITY_PATH" ]; then
        UNITY_PATH="/Applications/Unity/Unity.app/Contents/MacOS/Unity"
    fi
else
    # Linux
    UNITY_PATH="$HOME/Unity/Hub/Editor/2022.3.22f1/Editor/Unity"
    if [ ! -f "$UNITY_PATH" ]; then
        UNITY_PATH="/opt/unity/Editor/Unity"
    fi
fi

# 检查Unity是否存在
if [ ! -f "$UNITY_PATH" ]; then
    echo "[错误] 未找到Unity编辑器！"
    echo "请修改脚本中的UNITY_PATH变量为您的Unity安装路径"
    echo ""
    echo "macOS常见路径："
    echo "  /Applications/Unity/Hub/Editor/2022.3.x/Unity.app/Contents/MacOS/Unity"
    echo ""
    echo "Linux常见路径："
    echo "  ~/Unity/Hub/Editor/2022.3.x/Editor/Unity"
    echo "  /opt/unity/Editor/Unity"
    exit 1
fi

# 设置项目路径
PROJECT_PATH="$(cd "$(dirname "$0")" && pwd)"

# 设置输出路径
BUILD_PATH="$PROJECT_PATH/Builds"

# 创建输出目录
mkdir -p "$BUILD_PATH"

echo "[信息] 项目路径: $PROJECT_PATH"
echo "[信息] 输出路径: $BUILD_PATH"
echo ""

# 构建APK
echo "[开始构建] 正在构建Android APK..."
echo ""

"$UNITY_PATH" \
    -quit \
    -batchmode \
    -nographics \
    -projectPath "$PROJECT_PATH" \
    -executeMethod JRPG.Editor.BuildTools.BuildAPK \
    -buildTarget Android \
    -logFile "$BUILD_PATH/build.log"

# 检查构建结果
if [ $? -eq 0 ]; then
    echo ""
    echo "========================================"
    echo "  构建成功！"
    echo "========================================"
    echo ""
    echo "APK位置: $BUILD_PATH/DivineChaos.apk"
    echo ""
    
    # 打开输出目录
    if [[ "$OSTYPE" == "darwin"* ]]; then
        open "$BUILD_PATH"
    else
        xdg-open "$BUILD_PATH"
    fi
else
    echo ""
    echo "========================================"
    echo "  构建失败！"
    echo "========================================"
    echo ""
    echo "请查看日志: $BUILD_PATH/build.log"
    echo ""
    cat "$BUILD_PATH/build.log" | tail -50
fi
