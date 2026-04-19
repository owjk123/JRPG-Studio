#!/bin/bash
# validate-commit.sh - 提交验证钩子
# 检查提交信息格式和代码质量

# 只在git commit时运行
if ! echo "$*" | grep -q "git commit"; then
    exit 0
fi

echo "🔍 验证提交..."

# 获取提交信息
COMMIT_MSG=$(git log -1 --pretty=%B 2>/dev/null)

if [ -z "$COMMIT_MSG" ]; then
    # 还没有提交，检查暂存区
    STAGED_FILES=$(git diff --cached --name-only 2>/dev/null)
    
    # 检查是否有TODO
    if echo "$STAGED_FILES" | grep -q "\.cs$"; then
        for file in $STAGED_FILES; do
            if [ -f "$file" ]; then
                if grep -q "TODO:" "$file" 2>/dev/null; then
                    echo "⚠️ $file 包含 TODO: 标记，请确保这是有意为之"
                fi
            fi
        done
    fi
    
    # 检查是否有硬编码数值（在战斗代码中）
    BATTLE_FILES=$(echo "$STAGED_FILES" | grep "src/Battle" 2>/dev/null)
    if [ -n "$BATTLE_FILES" ]; then
        for file in $BATTLE_FILES; do
            if [ -f "$file" ]; then
                # 检查魔法数字（排除0, 1, -1等常见值）
                if grep -E "[^0-9][0-9]{2,}[^0-9]" "$file" 2>/dev/null | grep -v "const\|readonly\|//"; then
                    echo "⚠️ $file 可能包含硬编码数值，建议使用ScriptableObject配置"
                fi
            fi
        done
    fi
fi

exit 0
