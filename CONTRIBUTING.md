# 贡献指南

感谢参与 **UtauTtsBuilder** 的开发！本项目采用轻量但明确的源代码管理规范：
**Conventional Commits + SemVer 2.0.0 + CHANGELOG + CI 自动验证**。

无论你是想修一个小 bug、加一种新语言的 reclist、还是重构某个模块，都欢迎提交 Pull Request。本指南会告诉你如何开始。

---

## 目录

- [本地开发](#本地开发)
- [项目结构](#项目结构)
- [代码风格](#代码风格)
- [提交规范](#提交规范)
- [分支模型](#分支模型)
- [Pull Request 流程](#pull-request-流程)
- [Issue 提交](#issue-提交)
- [版本号规则](#版本号规则)
- [CHANGELOG 维护](#changelog-维护)
- [发布流程](#发布流程)
- [常见问题](#常见问题)
- [联系方式](#联系方式)

---

## 本地开发

### 环境要求

| 组件 | 版本 | 说明 |
|---|---|---|
| .NET SDK | 10.0 或更高 | [下载](https://dotnet.microsoft.com/download/dotnet/10.0) |
| .NET MAUI workload | — | `dotnet workload install maui-android` |
| JDK | 17 | 编译 Android 必需 |
| Android SDK | API 21+ | 用 Android Studio 或 `sdkmanager` 安装 |
| Git | 任意 | 版本控制 |

### 首次克隆

```bash
git clone https://github.com/styisme/UtauTtsBuilder.git
cd UtauTtsBuilder

# 安装 MAUI 工作负载（只需一次）
dotnet workload install maui-android

# 还原依赖
dotnet restore
```

### 编译

```bash
# Debug 模式（推荐开发时用）
dotnet build -f net10.0-android -c Debug

# Release 模式（发布前验证）
dotnet build -f net10.0-android -c Release
```

### 在设备上运行

```bash
# 需要开启 USB 调试并连接设备
dotnet build -t:Run -f net10.0-android
```

或直接安装编译好的 APK：

```bash
adb install bin/Debug/net10.0-android/com.example.utauttsbuilder-Signed.apk
```

### 云端构建

本项目已配置 GitHub Actions，任何 push 到 `main` 分支或手动触发的 workflow 都会自动编译 APK，产物在 Actions 页面的 **Artifacts** 区域。

**如果没有本地开发环境，也可以**：

1. 在 GitHub 网页上 Fork 本项目
2. 直接在网页编辑器里修改文件
3. Push 后让 Actions 自动编译
4. 从 Artifacts 下载 APK 到手机安装测试

---

## 项目结构

```
UtauTtsBuilder/
├── UtauTtsBuilder.csproj           # 项目文件
├── MauiProgram.cs                  # MAUI 启动入口
├── App.xaml / App.xaml.cs          # 应用级资源与生命周期
├── MainPage.xaml / .cs             # 主界面 UI + 交互
│
├── Platforms/Android/              # Android 平台特定代码
│   ├── AndroidManifest.xml         # 权限与服务声明
│   ├── MainActivity.cs             # Android 入口 Activity
│   ├── MainApplication.cs          # Android Application
│   ├── TtsEngineHelper.cs          # 枚举设备上的 TTS 引擎
│   └── TtsSynthesizer.cs           # 封装 TTS 合成到 WAV 文件
│
├── Services/                       # 纯逻辑层（尽量不依赖 Android）
│   ├── JapaneseCvReclist.cs        # 日语 CV 音节表
│   ├── ReclistProvider.cs          # 多语言 reclist 统一入口 + 语言枚举
│   ├── WavProcessor.cs             # 纯 C# WAV 解析 / 重采样 / 音量归一化
│   ├── OtoGenerator.cs             # oto.ini / character.txt / character.yaml
│   └── VoicebankBuilder.cs         # 主流程编排
│
├── Resources/
│   ├── AppIcon/                    # 应用图标（SVG）
│   └── Splash/                     # 启动画面（SVG）
│
└── .github/
    └── workflows/
        └── build-android.yml       # CI 编译脚本
```

**分层原则**：

- **UI 层**（`MainPage.xaml`）：只做界面与事件绑定，逻辑尽量下沉
- **平台层**（`Platforms/Android/`）：所有 Android API 调用集中在此
- **服务层**（`Services/`）：纯 C# 业务逻辑，方便单元测试与跨平台复用

---

## 代码风格

### C# 约定

- 遵循 [Microsoft C# 编码约定](https://learn.microsoft.com/zh-cn/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- 使用 **4 空格缩进**（不用 Tab）
- 私有字段前缀 `_`（如 `_enginePackage`）
- 使用表达式成员（`=>`）简化单行属性与方法
- 优先使用 `string?` 而非无注解的 `string`
- 异步方法以 `Async` 结尾
- **不要**在文件顶部堆无用的 `using`，.NET 10 已开启 `ImplicitUsings`

### 命名空间冲突

由于 Android 命名空间里有大量与 MAUI 同名的类型（例如 `TextToSpeech`、`Locale`、`Bundle`），**在 `Platforms/Android/` 下的代码请使用 `global::` 前缀或 `using` 别名**：

```csharp
// ✅ 推荐
using AndroidTts = global::Android.Speech.Tts.TextToSpeech;

// ✅ 也可行
var locale = global::Java.Util.Locale.Japanese;

// ❌ 避免（会和 MAUI 的 TextToSpeech 冲突）
using Android.Speech.Tts;
```

### XAML 约定

- `x:Name` 使用 **PascalCase**，且与后台代码中的字段一致
- 事件处理函数命名为 `OnXxxClicked` / `OnXxxChanged`
- 样式集中在 `<ContentPage.Resources>` 中定义，避免重复设置属性
- 尽量使用 `Border` + `Style` 实现卡片布局，不要滥用 `Frame`（已弃用）

### 文件组织

- 涉及 Android API 的类放到 `Platforms/Android/` 下
- 纯逻辑放到 `Services/` 下，尽量不依赖 Android
- 一个文件一个类（除非是紧密相关的内部类）

---

## 提交规范

使用 [Conventional Commits](https://www.conventionalcommits.org/zh-hans/)：

```
<type>(<scope>): <subject>
```

### 允许的 type

| type | 说明 |
|---|---|
| `feat` | 新功能 |
| `fix` | 修复 bug |
| `docs` | 仅文档变更 |
| `style` | 不影响代码含义的格式调整（空格、分号等） |
| `refactor` | 重构（非新功能、非修复） |
| `perf` | 性能优化 |
| `test` | 测试相关 |
| `build` | 构建系统或依赖变更 |
| `ci` | CI 配置变更 |
| `chore` | 其他杂项（版本号、.gitignore 等） |
| `revert` | 回滚某次提交 |

### 常用 scope

| scope | 说明 |
|---|---|
| `tts` | TTS 合成相关 |
| `oto` | oto.ini 生成相关 |
| `wav` | 音频处理相关 |
| `reclist` | 音节表相关 |
| `ui` | 界面相关 |
| `builder` | 声库构建流程相关 |
| `ci` | CI 配置 |
| `deps` | 依赖版本 |

### 示例

```
feat(tts): 支持西班牙语 reclist
fix(oto): 修复元音音节参数估算错误
docs(readme): 补充 Actions 编译说明
ci: 升级 setup-java 到 v5
chore: bump version to 1.0.1
refactor(wav): 提取重采样逻辑到独立方法
```

### 单次提交的原则

- 一次提交只做一件事（原子提交）
- subject 使用**祈使句、现在时**（`add` 而非 `added` 或 `adds`）
- subject 不超过 72 字符，不加句号
- 需要详细说明时，空一行后写 body

---

## 分支模型

| 分支 | 用途 |
|---|---|
| `main` | 稳定分支，所有发布的 tag 都从这里打 |
| `feat/xxx` | 功能分支，从 `main` 切出 |
| `fix/xxx` | 修复分支，从 `main` 切出 |
| `docs/xxx` | 文档分支 |
| `release/x.y.z` | 发布准备分支（可选） |

**开发流程**：

```bash
git checkout main
git pull
git checkout -b feat/spanish-reclist
# ... 修改代码 ...
git add .
git commit -m "feat(reclist): 添加西班牙语 CV 音节表"
git push origin feat/spanish-reclist
```

---

## Pull Request 流程

1. **Fork** 本仓库（或用 GitHub 网页直接编辑）
2. **新建分支**：`git checkout -b feat/your-feature`
3. **修改代码**并本地测试（或依赖 Actions 验证）
4. **提交**时使用 Conventional Commits 格式
5. **Push** 到你的 Fork
6. 向本仓库的 `main` 分支**发起 PR**
7. 等待 CI 通过 + 维护者 Review

### PR 描述模板

PR 描述应至少包含：

```markdown
## 变更内容

简述这个 PR 做了什么。

## 变更类型

- [ ] 新功能（feat）
- [ ] Bug 修复（fix）
- [ ] 重构（refactor）
- [ ] 文档（docs）
- [ ] 其他（chore / ci / build / perf / test）

## 关联 Issue

Closes #123

## 测试方式

- [ ] 本地编译通过（Debug / Release）
- [ ] 在真机上安装并测试
- [ ] Actions 编译通过

## 截图 / 录屏

（如果是 UI 相关改动）
```

### Review 关注点

- 是否遵循 [代码风格](#代码风格)
- 是否有对应的测试（如果能自动化）
- 是否会破坏现有功能
- 是否更新了 [CHANGELOG.md](CHANGELOG.md)（如果面向用户）
- 是否跨平台兼容（虽然目前只针对 Android）

---

## Issue 提交

### Bug 报告

请尽量提供：

- **设备型号** + **Android 版本**
- **TTS 引擎名称与版本**（例如：科大讯飞语音引擎 3.0）
- **复现步骤**（越详细越好）
- **期望行为** vs **实际行为**
- 相关日志、截图或生成的 `oto.ini` 片段

### 功能建议

- 描述使用场景（为什么需要这个功能）
- 如果可能，给出你设想的实现方式或参考项目
- 如果是 reclist 相关，请附上音节表来源

### 提问前

- 先搜索已有的 Issue 和 Closed Issue
- 查看 [README](README.md) 与 [CHANGELOG](CHANGELOG.md)
- 如果是编译问题，先确认已按 [本地开发](#本地开发) 配好环境

---

## 版本号规则

遵循 [Semantic Versioning 2.0.0](https://semver.org/lang/zh-CN/)：

| 变更 | 版本号递增 |
|---|---|
| 不兼容的 API 变更 | **MAJOR**（1.0.0 → 2.0.0） |
| 向后兼容的新功能 | **MINOR**（1.0.0 → 1.1.0） |
| 向后兼容的 bug 修复 | **PATCH**（1.0.0 → 1.0.1） |

版本号对应 `.csproj` 里的两个字段：

```xml
<ApplicationDisplayVersion>1.0.0</ApplicationDisplayVersion>  <!-- 用户可见，字符串 -->
<ApplicationVersion>1</ApplicationVersion>                      <!-- 内部整数，每次必须递增 -->
```

**注意**：`ApplicationVersion` 是 Android 系统用来判断"新版"的依据，每次发版**必须严格递增**（`1 → 2 → 3 ...`），否则用户无法覆盖安装。

---

## CHANGELOG 维护

所有面向用户的变更都要在 [CHANGELOG.md](CHANGELOG.md) 的 `[Unreleased]` 段落中记录。

**分类标签**：

- `Added` — 新功能
- `Changed` — 行为变更
- `Deprecated` — 已弃用的功能
- `Removed` — 移除的功能
- `Fixed` — Bug 修复
- `Security` — 安全相关

**发布新版本时**：

1. 把 `[Unreleased]` 改为 `[x.y.z] - YYYY-MM-DD`
2. 在下方补一个新的空 `[Unreleased]` 段落
3. 在文件底部更新对比链接

示例：

```markdown
## [Unreleased]

## [1.0.1] - 2026-10-20

### Fixed
- 修复某些设备上无法识别日语 TTS 的问题

[Unreleased]: https://github.com/styisme/UtauTtsBuilder/compare/v1.0.1...HEAD
[1.0.1]: https://github.com/styisme/UtauTtsBuilder/compare/v1.0.0...v1.0.1
[1.0.0]: https://github.com/styisme/UtauTtsBuilder/releases/tag/v1.0.0
```

---

## 发布流程

维护者发布新版本时的步骤：

1. **确认 `main` 分支最新**：`git checkout main && git pull`
2. **更新版本号**（`.csproj` 里的 `ApplicationDisplayVersion` 和 `ApplicationVersion`）
3. **整理 CHANGELOG**：把 `[Unreleased]` 内容固化到新版本号下
4. **提交**：`git commit -m "chore: bump version to x.y.z"`
5. **打 tag**：`git tag -a vx.y.z -m "Release vx.y.z"`
6. **推送**：`git push origin main && git push origin vx.y.z`
7. **等待 Actions 编译完成**
8. **创建 GitHub Release**：
   - Tag 选 `vx.y.z`
   - 标题填 `vx.y.z`
   - 描述里粘贴 CHANGELOG 里对应的段落
   - 上传 Actions Artifacts 里的 APK
9. **发布**

---

## 常见问题

### Q: 编译时报 `Xamarin.Android 找不到 Java`

检查 JDK 17 是否安装、`JAVA_HOME` 环境变量是否指向 JDK 17 目录。

### Q: Actions 编译失败，日志显示 `NETSDK1045`

`.NET 10` 可能还在预览期，确认 workflow 里的 `dotnet-version` 和 `.csproj` 里的 `TargetFramework` 一致。

### Q: 生成声库时提示"该 TTS 引擎不支持日语"

你的设备上安装的 TTS 引擎可能没有日语语音包。可以：
- 安装 Google TTS 并下载日语语音包
- 换成其他支持日语的引擎（如科大讯飞的部分版本）

### Q: 生成的声库在 UTAU 里乱码

`oto.ini` 和 `character.txt` 必须使用 **Shift-JIS** 编码。本项目已处理，如果你手动编辑过，注意保存为 Shift-JIS。

### Q: 图标不显示 / 显示为默认安卓机器人

- 部分启动器有图标缓存，卸载重装或重启设备
- 原版 UTAU 对 PNG 支持有限，建议用 BMP
- OpenUtau 无此限制

### Q: 想加一种新语言的 reclist

1. 在 `Services/ReclistProvider.cs` 里新增一个 `Dictionary<string, string>`
2. 在 `VoicebankLanguage` 枚举里加一项
3. 在 `GetReclist` / `GetLanguageCode` / `GetDisplayName` 里补上 `switch` 分支
4. 在 `MainPage.xaml.cs` 的 `InitLanguagePicker` 里加对应显示名
5. 更新 CHANGELOG

---

## 联系方式

- **Issue 区**：报告 bug、提出建议
- **Pull Request**：提交代码
- **@styisme**：其他直接联系

---

再次感谢你的贡献！🎉