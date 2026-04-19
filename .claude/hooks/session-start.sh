#!/bin/bash
# session-start.sh - 会话启动钩子
# 显示当前项目状态和最近的活动

echo "🎮 JRPG-Studio 会话启动"
echo "========================"

# 检查是否在项目根目录
if [ ! -f "CLAUDE.md" ]; then
    echo "⚠️ 未在项目根目录"
    exit 0
fi

# 显示当前分支
if command -v git &> /dev/null; then
    BRANCH=$(git branch --show-current 2>/dev/null)
    if [ -n "$BRANCH" ]; then
        echo "📍 当前分支: $BRANCH"
    fi
fi

# 显示最近的提交
if command -v git &> /dev/null; then
    echo ""
    echo "📝 最近提交:"
    git log --oneline -5 2>/dev/null | while read line; do
        echo "   $line"
    done
fi

# 显示项目状态
echo ""
echo "📊 项目状态:"

# 统计设计文档
GDD_COUNT=$(find design/gdd -name "*.md" 2>/dev/null | wc -l)
echo "   设计文档: $GDD_COUNT 份"

# 统计代码文件
CS_COUNT=$(find src -name "*.cs" 2>/dev/null | wc -l)
echo "   C#文件: $CS_COUNT 个"

# 检查是否有active.md
if [ -f "production/session-state/active.md" ]; then
    echo ""
    echo "🔄 上次会话进度:"
    head -10 production/session-state/active.md 2>/dev/null | while read line; do
        echo "   $line"
    done
fi

echo ""
echo "💡 运行 /help 查看可用命令"
echo ""
