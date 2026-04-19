#!/bin/bash
# ========================================
# JRPG-Studio GitHub推送脚本
# ========================================

echo "========================================"
echo "  JRPG-Studio GitHub推送工具"
echo "========================================"
echo ""

# GitHub配置
GITHUB_USER="owjk123"
REPO_NAME="JRPG-Studio"

# 检查是否已登录
if gh auth status &>/dev/null; then
    echo "[✓] 已登录GitHub"
else
    echo "[!] 需要登录GitHub"
    echo ""
    echo "请选择登录方式："
    echo "  1. 使用Personal Access Token"
    echo "  2. 使用浏览器登录"
    echo ""
    read -p "请选择 (1/2): " choice
    
    if [ "$choice" = "1" ]; then
        echo ""
        echo "请输入GitHub Personal Access Token:"
        echo "(需要 repo, workflow 权限)"
        read -s TOKEN
        echo "$TOKEN" | gh auth login --with-token
    else
        gh auth login
    fi
fi

# 检查仓库是否存在
echo ""
echo "[检查仓库状态...]"
if gh repo view "$GITHUB_USER/$REPO_NAME" &>/dev/null; then
    echo "[✓] 仓库已存在: https://github.com/$GITHUB_USER/$REPO_NAME"
else
    echo "[!] 仓库不存在，正在创建..."
    gh repo create "$REPO_NAME" --public --description "神界：混沌边境 - 日式回合制RPG手游 | Unity 2022.3 LTS"
    echo "[✓] 仓库已创建: https://github.com/$GITHUB_USER/$REPO_NAME"
fi

# 添加远程仓库
cd "$(dirname "$0")"
git remote remove origin 2>/dev/null
git remote add origin "https://github.com/$GITHUB_USER/$REPO_NAME.git"

# 推送代码
echo ""
echo "[推送代码到GitHub...]"
git push -u origin main

if [ $? -eq 0 ]; then
    echo ""
    echo "========================================"
    echo "  推送成功！"
    echo "========================================"
    echo ""
    echo "仓库地址: https://github.com/$GITHUB_USER/$REPO_NAME"
    echo ""
    echo "后续操作："
    echo "  1. 配置GitHub Actions Secrets (UNITY_LICENSE, UNITY_EMAIL, UNITY_PASSWORD)"
    echo "  2. 推送代码会自动触发APK构建"
    echo "  3. 在Actions页面下载构建好的APK"
    echo ""
else
    echo ""
    echo "[!] 推送失败，请检查网络连接和认证信息"
fi
