# 神界：混沌边境 - 构建指南

本文档详细说明如何构建Android APK。

## 📋 前置要求

### 必需软件
- **Unity 2022.3 LTS** (推荐 2022.3.22f1 或更高版本)
- **Android Build Support** (通过Unity Hub安装)
- **Android SDK** (API 24-33)
- **Android NDK** (r25c)
- **JDK 11 或 JDK 17**

### 可选软件
- **Git** - 用于版本控制
- **GitHub账号** - 用于CI/CD自动构建

## 🚀 方法一：Unity编辑器构建（推荐新手）

### 步骤

1. **打开Unity Hub**
   - 确保已安装Unity 2022.3 LTS
   - 确保已安装Android Build Support模块

2. **添加项目**
   - 点击 "Add" 按钮
   - 选择 `JRPG-Studio` 文件夹
   - 选择Unity 2022.3 LTS版本

3. **打开项目**
   - 双击项目打开Unity编辑器
   - 首次打开会导入资源，需要几分钟

4. **构建设置**
   - 菜单：`File > Build Settings`
   - 确认Platform选中 `Android`
   - 点击 `Switch Platform`（如果需要）

5. **配置Player Settings**
   - 点击 `Player Settings`
   - 设置公司名称和产品名称
   - 配置Android设置：
     - Minimum API Level: Android 7.0 (API 24)
     - Target API Level: Android 13 (API 33)
     - Scripting Backend: IL2CPP
     - Target Architecture: ARM64 + ARMv7

6. **开始构建**
   - 点击 `Build` 或 `Build And Run`
   - 选择APK保存位置
   - 等待构建完成

### 快捷方式
- 菜单：`JRPG Studio > 构建 > 快速构建APK (Debug)`
- 自动完成所有配置并构建

## 🔧 方法二：命令行构建

### Windows

```batch
# 编辑 build-local.bat 中的Unity路径
# 然后双击运行
build-local.bat
```

### macOS/Linux

```bash
# 编辑 build-local.sh 中的Unity路径
chmod +x build-local.sh
./build-local.sh
```

### 直接命令

```bash
# Windows
"C:\Program Files\Unity\Hub\Editor\2022.3.22f1\Editor\Unity.exe" \
    -quit -batchmode -nographics \
    -projectPath . \
    -executeMethod JRPG.Editor.BuildTools.BuildAPK \
    -buildTarget Android

# macOS
/Applications/Unity/Hub/Editor/2022.3.22f1/Unity.app/Contents/MacOS/Unity \
    -quit -batchmode -nographics \
    -projectPath . \
    -executeMethod JRPG.Editor.BuildTools.BuildAPK \
    -buildTarget Android

# Linux
~/Unity/Hub/Editor/2022.3.22f1/Editor/Unity \
    -quit -batchmode -nographics \
    -projectPath . \
    -executeMethod JRPG.Editor.BuildTools.BuildAPK \
    -buildTarget Android
```

## 🔄 方法三：GitHub Actions自动构建

### 设置步骤

1. **Fork或推送代码到GitHub**
   ```bash
   git init
   git add .
   git commit -m "Initial commit"
   git branch -M main
   git remote add origin https://github.com/你的用户名/JRPG-Studio.git
   git push -u origin main
   ```

2. **配置GitHub Secrets**
   - 进入仓库 Settings > Secrets and variables > Actions
   - 添加以下secrets：
     - `UNITY_LICENSE`: Unity许可证文件内容
     - `UNITY_EMAIL`: Unity账号邮箱
     - `UNITY_PASSWORD`: Unity账号密码

3. **获取Unity许可证**
   - 在本地Unity编辑器中：`Help > Manage License > Create License Request`
   - 保存许可证文件内容

4. **触发构建**
   - 推送代码到main分支自动触发
   - 或在Actions页面手动触发

5. **下载APK**
   - 构建完成后在Actions页面下载Artifact
   - 或在Releases页面下载发布的APK

## 📱 安装测试

### 通过ADB安装

```bash
# 安装APK
adb install DivineChaos.apk

# 安装并启动
adb install -r DivineChaos.apk && adb shell am start -n com.jrpgstudio.divinechaos/com.unity3d.player.UnityPlayerActivity
```

### 直接安装

1. 将APK传输到Android手机
   - 通过USB数据线
   - 通过云盘分享
   - 通过电子邮件

2. 在手机上打开APK文件安装
   - 可能需要开启"允许安装未知来源应用"

## ⚠️ 常见问题

### Q: 构建失败 "Android SDK not found"
**A:** 确保在Unity Hub中安装了Android Build Support，或在Preferences中手动配置Android SDK路径。

### Q: 构建失败 "NDK not found"
**A:** 安装Android NDK r25c，并在Unity Preferences > External Tools中配置NDK路径。

### Q: 构建成功但APK无法安装
**A:** 检查手机Android版本是否 >= 7.0，并确保已开启"允许安装未知来源应用"。

### Q: 构建成功但游戏闪退
**A:** 
- 检查是否选择了正确的架构（ARM64 + ARMv7）
- 查看adb logcat日志定位错误
- 确保Scripting Backend使用IL2CPP

### Q: 首次打开项目报错
**A:** 
- 等待Unity完成资源导入
- 检查Console中的错误信息
- 尝试 `Assets > Reimport All`

## 📂 构建输出

构建完成后，输出文件位于：
```
Builds/
├── DivineChaos.apk          # Android安装包
├── DivineChaos.apk.id       # 构建ID
├── build.log                # 构建日志
└── Publishing/              # 发布配置（如果存在）
    └── PlayerSettings.keystore  # 签名文件
```

## 🔐 发布签名

发布版本需要签名：

1. **创建Keystore**
   - Unity: `Edit > Project Settings > Player > Publishing Settings`
   - 或使用keytool：
     ```bash
     keytool -genkey -v -keystore divinechaos.keystore -alias divinechaos -keyalg RSA -keysize 2048 -validity 10000
     ```

2. **配置签名**
   - 在Player Settings中设置Keystore路径和密码
   - Release构建会自动签名

3. **验证签名**
   ```bash
   # 查看APK签名信息
   keytool -printcert -jarfile DivineChaos.apk
   ```

---

**祝构建顺利！如有问题请查看构建日志或提交Issue。**
