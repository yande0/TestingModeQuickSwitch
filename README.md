# Windows 测试模式切换工具 (TestingModeQuickSwitch)

Windows 测试模式（Test Mode）开关切换工具，支持 Xbox 手柄控制。

## 功能

- 一键开启/关闭 Windows 测试模式（`bcdedit /set testsigning`）
- 显示当前 Windows 版本和测试模式状态
- **Xbox 手柄支持**：
  - A 键 — 切换模式
  - B 键 — 退出
  - 方向键/左摇杆 — 按钮选择（对话框）

## 系统要求

- .NET Framework 4.7.2+
- Windows Vista 及以上（需管理员权限运行）
- Xbox 手柄（可选）

## 使用方法

1. **以管理员身份运行** `SwitchTestingMode.exe`
2. 点击 **开启/关闭测试模式** 按钮
3. 系统将自动重启生效

## 项目结构

```
SwitchTestingMode/
├── Program.cs              # 入口点
├── MainForm.cs             # 主窗口 UI 与交互逻辑
├── TestModeManager.cs      # 测试模式状态检测与设置（bcdedit）
├── XboxController.cs       # Xbox 手柄输入封装（XInput）
├── ControllerDialog.cs     # 支持手柄操作的对话框
├── app.manifest            # 应用程序清单
├── app.ico                 # 应用程序图标
└── Properties/
    └── AssemblyInfo.cs     # 程序集信息
```

## 技术细节

### 测试模式管理 (`TestModeManager.cs`)

- 调用 `bcdedit.exe /enum {current}` 解析当前测试模式状态
- 调用 `bcdedit /set testsigning on/off` 切换状态
- 通过 `shutdown /r /t 0` 重启系统
- 自动处理 64 位系统下 32 位进程的 SysNative 重定向

### Xbox 手柄 (`XboxController.cs`)

- 基于 XInput 1.4（`xinput1_4.dll`）
- 轮询方式检测最多 4 个手柄
- 支持按键：A、B、Start、方向键
- 左摇杆上下映射为方向键上下
- 连接状态变化事件通知

### 手柄对话框 (`ControllerDialog.cs`)

- 自定义对话框，支持手柄方向键切换按钮焦点
- A 键确认、B 键取消

## 编译

```bash
# Debug
msbuild SwitchTestingMode.csproj /p:Configuration=Debug

# Release
msbuild SwitchTestingMode.csproj /p:Configuration=Release
```

也可使用 Visual Studio 直接打开项目文件编译。

## 注意事项

- **必须使用管理员权限运行**，否则 bcdedit 调用会失败
- 切换测试模式后需要重启系统才能生效
- 测试模式会在系统水印区域显示"测试模式"文字
