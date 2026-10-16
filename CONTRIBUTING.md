**🌐 语言 / Language / 言語：** 中文 | [English](CONTRIBUTING.en.md)

---
# 贡献指南

感谢参与 **UtauTtsBuilder** 的开发！本项目采用轻量但明确的源代码管理规范：
**Conventional Commits + SemVer 2.0.0 + CHANGELOG + CI 自动验证**。

---

## ⚠️ 关于 AI 生成代码

本项目的绝大多数代码由 **AI 辅助生成**，详见 [docs/AI_DISCLOSURE.md](docs/AI_DISCLOSURE.md)。提交 PR 时请：

- 说明改动是否由 AI 生成
- 人工验证过功能（至少在真机上跑过一次）
- 如果你也是用 AI 改的，请在 PR 描述里注明

---

## 本地开发

### 环境要求

| 组件 | 版本 |
|---|---|
| .NET SDK | 10.0 或更高 |
| .NET MAUI workload | `dotnet workload install maui-android` |
| JDK | 17 |
| Android SDK | API 21+ |

### 首次克隆

```bash
git clone https://github.com/styisme/UtauTtsBuilder.git
cd UtauTtsBuilder
dotnet workload install maui-android
dotnet restore
```

### 编译

```bash
# Debug（推荐开发时用）
dotnet build -f net10.0-android -c Debug

# Release
dotnet build -f net10.0-android -c Release
```

### 在设备上运行

```bash
dotnet build -t:Run -f net10.0-android
```

或直接安装 APK：

```bash
adb install bin/Debug/net10.0-android/com.example.utauttsbuilder-Signed.apk
```

### 云端构建

项目已配置 GitHub Actions：
- Push 到 `main` 或 `master` 自动编译
- 手动触发：Actions 页面 → **Run workflow**
- 编译产物在 **Artifacts** 区域下载

**没有本地开发环境？** 直接在 GitHub 网页上编辑文件，让 Actions 帮你编译。

---

## 项目结构

```
UtauTtsBuilder/
├── UtauTtsBuilder.csproj        # 项目文件
├── MauiProgram.cs               # MAUI 启动 + Handler 注册
├── App.xaml / App.xaml.cs       # 全局资源 + 生命周期
│
├── MainPage.xaml / .cs          # 主页（声库生成）
├── SettingsPage.xaml / .cs      # 设置页
├── AboutPage.xaml / .cs         # 关于页
├── CloudSettingsPage.xaml / .cs # 云端 TTS 配置
├── SelectionPage.xaml / .cs     # 自定义列表选择（替代 Picker）
├── ActionSheetPage.xaml / .cs   # 自定义操作菜单（替代 ActionSheet）
│
├── Platforms/Android/
│   ├── AndroidManifest.xml
│   ├── MainActivity.cs
│   ├── MainApplication.cs
│   ├── TtsEngineHelper.cs       # 枚举设备上的 TTS 引擎
│   └── TtsSynthesizer.cs        # 封装本地 TTS 合成到 WAV
│
├── Services/
│   ├── JapaneseCvReclist.cs     # 日语 CV 音节表（129 音节）
│   ├── ReclistProvider.cs       # 多语言 + CVVC/VCV + 自定义 reclist
│   ├── WavProcessor.cs          # 纯 C# WAV 解析 / 重采样 / 静音裁剪
│   ├── OtoGenerator.cs          # oto.ini / character.txt / character.yaml
│   ├── VoicebankBuilder.cs      # 主流程编排（本地 / 云端分发）
│   ├── CloudConfigService.cs    # 云端 API Key 存储
│   ├── CloudTtsEngine.cs        # Azure / Google Cloud TTS
│   ├── UstTemplateGenerator.cs  # 生成 test.ust
│   ├── ThemeService.cs          # 主题模式 + UI 风格统一入口
│   ├── ThemeHelper.cs           # 动态取色（Android 12+）
│   ├── UiStyleService.cs        # 四种 UI 风格色板
│   └── Localization.cs          # 三语字符串表 + Loc.T()
│
└── .github/workflows/
    └── build-android.yml        # CI 编译脚本
```

**分层原则**：

- **UI 层**（`*Page.xaml`）：只做界面与事件绑定
- **平台层**（`Platforms/Android/`）：所有 Android API 调用集中在此
- **服务层**（`Services/`）：纯 C# 业务逻辑，尽量不依赖 Android

---

## 代码风格

### C# 约定

- 使用 **4 空格缩进**（不用 Tab）
- 私有字段前缀 `_`（如 `_enginePackage`）
- 异步方法以 `Async` 结尾
- 优先使用 `string?` 而非无注解 `string`
- .NET 10 已开启 `ImplicitUsings`，不要写冗余 `using`

### 命名空间冲突（重要）

由于 `Platforms/Android/` 下的命名空间是 `UtauTtsBuilder.Platforms.Android`，**编译器会优先把它当根**。所有 Android API 调用必须用 `global::` 前缀：

```csharp
// ✅ 推荐
using AndroidTts = global::Android.Speech.Tts.TextToSpeech;

// ✅ 也可行
var locale = global::Java.Util.Locale.Japanese;

// ❌ 避免（会被解析为 UtauTtsBuilder.Platforms.Android.Speech）
using Android.Speech.Tts;
```

### XAML 约定

- `x:Name` 用 **PascalCase**，与后台代码一致
- 事件处理函数：`OnXxxClicked` / `OnXxxChanged`
- **颜色引用必须用 `DynamicResource`**（不能用 `StaticResource`，否则主题切换不生效）
- 样式集中在 `App.xaml` 的 `<Application.Resources>`
- 卡片用 `Border` + `StrokeShape="RoundRectangle 12"`，不要用已弃用的 `Frame`
- **不要用系统 `Picker` / `DisplayActionSheet`** —— 用 `SelectionPage` / `ActionSheetPage`

### 本地化约定

- **所有用户可见文本必须走 `Loc.T()`**，不要硬编码中文/英文
- 新增 key 时同步补 `Localization.cs` 里的中、英、日三语
- 带参数的用 `Loc.F("key_fmt", arg1, arg2)`

```csharp
// ✅
StatusLabel.Text = Loc.F("engines_found_fmt", count);

// ❌
StatusLabel.Text = $"已发现 {count} 个 TTS 引擎";
```

### 主题约定

- 颜色 key 统一前缀 `Md3*`（如 `Md3Primary` / `Md3OnSurface`）
- 尺寸 key 统一前缀 `Ui*`（如 `UiBtnCorner` / `UiBtnHeight`）
- 主题切换必须通过 `ThemeService.Apply()`，不要直接改 `Resources`

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
| `style` | 格式调整（空格、分号等） |
| `refactor` | 重构 |
| `perf` | 性能优化 |
| `test` | 测试相关 |
| `build` | 构建系统或依赖变更 |
| `ci` | CI 配置变更 |
| `chore` | 杂项（版本号、.gitignore） |
| `revert` | 回滚 |

### 常用 scope

| scope | 说明 |
|---|---|
| `tts` | TTS 合成相关 |
| `oto` | oto.ini 生成相关 |
| `wav` | 音频处理相关 |
| `reclist` | 音节表相关 |
| `ui` | 界面相关 |
| `i18n` | 本地化相关 |
| `theme` | 主题 / UI 风格相关 |
| `cloud` | 云端 TTS 相关 |
| `builder` | 声库构建流程 |
| `ci` | CI 配置 |

### 示例

```
feat(cloud): add Azure Speech support
fix(oto): correct cutoff sign for mobile UTAU
feat(reclist): expand Japanese CV to 129 syllables
docs(changelog): add v1.1.1 section
style(ui): migrate to Material Design 3
```

---

## 分支模型

| 分支 | 用途 |
|---|---|
| `master` | 稳定分支（当前默认） |
| `feat/xxx` | 功能分支 |
| `fix/xxx` | 修复分支 |
| `docs/xxx` | 文档分支 |

---

## Pull Request 流程

1. Fork 仓库（或直接编辑）
2. 新建分支：`git checkout -b feat/your-feature`
3. 修改代码并本地 / Actions 验证
4. 提交时用 Conventional Commits
5. Push 并发起 PR
6. 等 CI 通过 + Review

### PR 描述应包含

```markdown
## 变更内容
简述做了什么

## 变更类型
- [ ] feat / fix / refactor / docs / chore

## 关联 Issue
Closes #123

## 测试方式
- [ ] 本地编译通过
- [ ] 真机安装并测试
- [ ] Actions 编译通过

## 是否使用 AI 生成
- [ ] 是（请说明使用的模型和人工验证内容）
- [ ] 否
```

---

## Issue 提交

### Bug 报告

请提供：

- **设备型号** + **Android 版本**
- **TTS 引擎名称与版本**（如：科大讯飞语音引擎 3.0）
- **复现步骤**
- **期望行为** vs **实际行为**
- 相关日志（`_log.txt`）、截图或 `oto.ini` 片段

### 功能建议

- 描述使用场景
- 如果可能，给出参考项目或实现思路

---

## 版本号规则

遵循 [Semantic Versioning 2.0.0](https://semver.org/lang/zh-CN/)：

| 变更 | 版本号递增 |
|---|---|
| 不兼容变更 | **MAJOR** |
| 向后兼容的新功能 | **MINOR** |
| Bug 修复 | **PATCH** |

### ⚙️ 版本号自动注入（重要）

**你不需要手动改 `.csproj`！** GitHub Actions 会：

1. 从 `CHANGELOG.md` 提取最新的 `## [x.y.z]` 作为 `ApplicationDisplayVersion`
2. 用 `YYYYMMDD` 作为 `ApplicationVersion`（Android 内部整数）
3. APK 自动重命名为 `UtauTtsBuilder-v{x.y.z}.apk`

所以你**只要维护 `CHANGELOG.md`**，版本号就自动跟上了。

---

## CHANGELOG 维护

所有面向用户的变更都要在 `CHANGELOG.md` 的 `[Unreleased]` 段落中记录。

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
3. 在文件底部更新 compare 链接

---

## 发布流程

维护者发布新版本的步骤：

1. 确认所有代码已 push 到 `master`
2. 更新 `CHANGELOG.md`：把 `[Unreleased]` 内容固化为新版本号
3. Commit + Push
4. 等 GitHub Actions 编译成功（绿色对勾）
5. 从 Actions 的 **Artifacts** 下载 APK
6. 去仓库的 **Releases** 页面 → **Draft a new release**
   - Tag：`v{x.y.z}` → 点 **Create new tag on publish**
   - Title：`v{x.y.z}`
   - Description：粘贴 CHANGELOG 里对应段落 + 安装说明
   - 上传 APK
   - ✅ 勾选 **Set as the latest release**
7. **Publish release**

---

## 常见问题

### Q: 编译报 `Xamarin.Android 找不到 Java`

检查 JDK 17 是否安装、`JAVA_HOME` 是否指向 JDK 17。

### Q: Actions 编译失败，日志显示 `NETSDK1045`

`.NET 10` 可能还在预览期，确认 workflow 里的 `dotnet-version` 和 `.csproj` 的 `TargetFramework` 一致。

### Q: 生成声库时提示"该 TTS 引擎不支持日语"

设备上的 TTS 引擎没有日语语音包。可以：
- 安装 Google TTS 并下载日语语音包
- 换成其他支持日语的引擎

### Q: 生成的声库在 UTAU 里乱码

`oto.ini` 和 `character.txt` 必须使用 **Shift-JIS** 编码。本项目已处理，手动编辑时注意保存为 Shift-JIS。

### Q: 移动端 UTAU 无法发音

- 检查 `oto.ini` 里的文件名是否为**假名**（`あ.wav` 而不是 `a.wav`）
- 检查 `cutoff` 是否为**正数**
- 检查 `character.txt` 是否在声库根目录
- 音符歌词必须与别名完全一致（`ちゃ` 而不是 `cha`）

### Q: 想加一种新语言的 reclist

1. 在 `ReclistProvider.cs` 的 `VoicebankLanguage` 枚举里加一项
2. 新增 `BuildXxx()` 或 `ConvertCvDict()` 分支
3. 更新 `GetReclist` / `GetLanguageCode` / `GetDisplayName`
4. 在 `Localization.cs` 里加三语显示名
5. 在 `MainPage.xaml.cs` 的 `InitLanguageList()` 里加进列表

---

## 联系方式

- **Issue 区**：报告 bug、提出建议
- **Pull Request**：提交代码
- **@styisme**：其他联系

---

再次感谢你的贡献！🎉