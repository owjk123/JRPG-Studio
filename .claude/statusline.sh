#!/bin/bash
# statusline.sh - 状态栏显示脚本
# 显示项目上下文信息

# 项目名称
PROJECT_NAME="JRPG-Studio"

# 当前分支
BRANCH=$(git branch --show-current 2>/dev/null)
if [ -z "$BRANCH" ]; then
    BRANCH="no-branch"
fi

# 统计
CS_COUNT=$(find src -name "*.cs" 2>/dev/null | wc -l)
GDD_COUNT=$(find design/gdd -name "*.md" 2>/dev/null | wc -l)

# 当前阶段
STAGE="初始化"
if [ -f "production/project-stage.txt" ]; then
    STAGE=$(cat production/project-stage.txt)
fi

echo "🎮 $PROJECT_NAME | 分支:$BRANCH | 阶段:$STAGE | C#:$CS_COUNT | GDD:$GDD_COUNT"
