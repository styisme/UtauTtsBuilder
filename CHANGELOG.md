# Changelog

本项目的重要变更都记录在此文件。

格式基于 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.0.0/)，
版本号遵循 [Semantic Versioning](https://semver.org/lang/zh-CN/)。

> **版本号说明**：v1.0.0 之前的版本未正式发布。

---

## [Unreleased]

### Planned
- 更精确的 oto.ini 自动估算算法（基于频谱分析 / 共振峰检测）
- 支持韩语、西班牙语、法语 reclist
- 更多云端 TTS（讯飞在线、阿里云、腾讯云）
- 内置波形编辑器（手动微调 oto 参数）
- 图标 / 立绘裁剪工具（内置方形裁剪 + 缩放）
- 批量生成（一次选多个引擎生成多个声库）
- 导出为 DiffSinger / NNSVS / ENUNU 格式
- 试听播放器（内置音频播放界面）
- 支持 Web（Blazor）

---

## [1.1.0] - 2026-10-16

本次是项目自 v1.0.0 以来的**首次重大更新**，新增云端 TTS、多语言 CVVC / VCV、四种 UI 风格、深色模式等大量功能。

### Added

#### 🗣️ TTS 引擎

- **云端 TTS 支持**
  - 新增 `CloudTtsEngine`，支持通过 HTTP API 调用云端语音合成
  - **Azure Speech** — 支持自定义 Region、Voice（如 `ja-JP-NanamiNeural`）
  - **Google Cloud TTS** — 支持 Neural2 / Wavenet 音色
  - 云端引擎自动附加到主页引擎列表，标记为 `☁ Azure (...)`
  - 云端返回的裸 PCM 自动加 WAV 头
  - 云端密钥通过 `CloudConfigService` 加密存于 `Preferences`
- **新增 `CloudSettingsPage`**
  - 独立的云端 TTS 配置页面
  - 支持 Azure API Key / Region / Voice
  - 支持 Google API Key
  - 输入框带密码遮罩

#### 📚 音素表

- **日语 CVVC (133 音节)**
  - 68 CV + 20 VV 过渡 + 45 VC 过渡
  - 过渡别名格式：`a k` → TTS 读取 `あか`
  - 使用 `ttsCache` 实现相同 TTS 文本的音频共享
- **日语 VCV (105 音节)**
  - 5 单独元音 + 6 ん相关 + 20 VV + 70 辅音过渡
  - 简化版 VCV（辅音行只取 a 元音音节）
- **中文 CVVC (203 音节)**
  - 78 CV + 20 VV 过渡 + 105 VC 过渡
  - 21 个声母 × 5 个基础韵母
- **中文 VCV (130 音节)**
  - 5 元音 + 20 VV + 105 声母行过渡
  - 声母行取 `b p m f d t n l g k h j q x zh ch sh r z c s` 的 a 元音
- **自定义音素表导入**
  - 支持 `.txt` 文件导入
  - 兼容多种分隔符：`别名=文件名` / `别名,文件名` / `别名 文件名`（空格 / Tab）
  - 支持注释行（`#` / `//`）
  - 支持 `# language: ja-JP` 指定 TTS 语言
  - 通过 `Preferences` 持久化，重启后保留
  - 主页新增「清除自定义音素表」按钮

#### 🎨 UI 与体验

- **四种 UI 风格**（设置页可切换，默认 MD3）
  - **Material 3** — MD3 官方配色（种子色 `#6750A4`）
  - **鸿蒙 HarmonyOS** — 蓝调 `#007DFF` + 白色卡片 + 中圆角
  - **Windows UWP** — Windows 蓝 `#0078D4` + 直角 + 扁平
  - **Android 4 Holo** — 深灰 `#000000` + 青 `#33B5E5` + 小圆角
- **深色模式**
  - 三种模式：跟随系统 / 浅色 / 深色
  - 通过 `ThemeService` 统一管理
  - 深色模式色板遵循 MD3 官方规范
- **动态取色（Material You）**
  - Android 12+ 从壁纸提取色调
  - 13 个 MD3 token 映射到系统动态色资源
  - 设置页可关闭（关闭后使用基线紫）
  - 深浅模式下分别使用不同的系统资源（如 `system_accent1_600` / `system_accent1_200`）
- **三语 UI**
  - 中文 / English / 日本語
  - 默认跟随系统语言（`zh-*` → 中文、`ja-*` → 日语、其他 → 英文）
  - 设置页可手动切换
  - 覆盖所有界面文本、进度消息、弹窗、reclist 显示名
  - `Loc.T()` / `Loc.F()` 统一本地化 API
- **完全自定义选择器**
  - 新增 `SelectionPage` 替代 MAUI `Picker`
  - 新增 `ActionSheetPage` 替代 `DisplayActionSheet`
  - 两种页面完全跟随当前 UI 风格
  - 选中项用主色高亮 + `●` 圆点标记
  - 支持系统返回键取消
- **设置页面重构**
  - 隐藏系统导航栏，改用页面内自定义 TopBar
  - 分区：外观 / UI 风格 / 语言 / 动态取色 / 云端 / 关于
  - 主题切换带淡入淡出动画（90ms 淡出 + 180ms 淡入）
- **关于页面**
  - 应用图标 + 名称 + 版本号
  - 项目描述
  - GitHub 仓库链接（点击打开浏览器）
  - MIT 协议链接
  - 版权年份自动更新

#### 🎵 音频处理

- **首尾静音裁剪**
  - `WavProcessor.TrimSilence()` 阈值 `0.005f`（约 -46dB）
  - 两端各保留 8ms padding，避免削去起音
  - 全静音音频返回短静音数组，避免空数组崩溃
  - 大幅改善 oto.ini 参数估算精度
- **语速 / 音高调节**
  - 主页两个滑块：语速 0.5–1.5× / 音高 0.5–2.0×
  - 实时显示数值
  - 传递给 `TtsSynthesizer.SetSpeechRate()` / `SetPitch()`
  - 云端 TTS 的语速 / 音高暂未支持（后续版本）
- **断点续传**
  - 主页新增「跳过已存在的音频」复选框（默认开启）
  - 合成前检查 `{音节}.wav` 是否存在，存在则跳过
  - 取消勾选时全量重新生成
  - 适用于中断后继续 / 增量修改场景

#### 📄 声库输出

- **UST 模板生成**
  - `UstTemplateGenerator` 自动生成 `test.ust`
  - 3 个音符（a / ka / a），Tempo 120
  - Shift-JIS 编码，直接拖入 UTAU 即可试听
- **生成日志**
  - 每个声库目录写入 `_log.txt`
  - 记录引擎、语言、语速、音高、跳过策略
  - 逐条记录每个音节的状态：`OK` / `SHARE` / `SKIP` / `EMPTY` / `FAIL`
  - 记录合成耗时（毫秒）
  - 末尾统计总数、成功、共享、跳过、失败
  - UTF-8 无 BOM 编码
- **完成后操作菜单**
  - 生成完成后弹出 `ActionSheetPage`
  - 选项：
    - **编辑 oto.ini** — 通过系统分享面板选择文本编辑器
    - **生成 UST 模板** — 写入 `test.ust`
    - **分享 ZIP** — 系统分享面板
  - 取代原来的单一「分享 ZIP」弹窗

#### 🖼️ 图片预览

- **图标缩略图** — 选中后 64×64 预览
- **立绘缩略图** — 选中后 64×96 预览
- 存储路径使用应用缓存目录，避免 `content://` 路径问题

#### ⚙️ 构建与发布

- **版本号自动注入**
  - GitHub Actions 从 `CHANGELOG.md` 提取最新的 `## [x.y.z]` 标题
  - 用作 `ApplicationDisplayVersion`
  - `ApplicationVersion` 使用 `YYYYMMDD`（每次递增）
  - APK 自动重命名：`UtauTtsBuilder-v1.1.0.apk`
- **CI 优化**
  - `setup-java@v5`（修复 Node.js 20 弃用警告）
  - 手动指定 Android SDK 环境变量，避免 `setup-android` 失败
  - `dotnet workload install maui-android --source https://api.nuget.org/v3/index.json`

### Changed

- **引擎列表**
  - 从 `Picker` 改为自定义 `SelectionPage`
  - 支持本地 + 云端混合显示
  - 选中引擎后自动填入声库名（去版本号）
  - 每次返回主页自动刷新（云端配置可能变化）
- **主题系统重构**
  - 色板从 `MainPage.Resources` 移到 `App.xaml` 全局资源
  - 所有颜色引用从 `StaticResource` 改为 `DynamicResource`
  - `ThemeService.Apply()` 统一入口
- **`MauiProgram.cs`**
  - 新增 Entry / Picker 去下划线 Handler
  - 新增 Switch / RadioButton 去白底 Handler
  - Switch 统一 thumb 白色
- **导航栏**
  - `SettingsPage` / `AboutPage` / `CloudSettingsPage` 隐藏系统导航栏
  - 改用页面内自定义 TopBar（← 返回按钮 + 标题）

### Fixed

- **启动器图标**
  - `appicon.svg` 改为**完整图标**（紫色渐变 + 白色声波）
  - `MauiIcon` 加 `BaseSize="456,456"` 强制尺寸
  - `appiconfg.svg` 作为自适应图标前景层
- **设置页 Switch 白底**
  - 通过 Android Handler 设置 `BackgroundTintList = Transparent`
- **RadioButton 白底**
  - 同 Switch，用 Handler 处理
- **导航栏风格分裂**
  - 之前 `SettingsPage` 用系统默认深色导航栏，与 MD3 主题不搭
  - 现隐藏系统导航栏，改用页面内 TopBar
- **`OtoGenerator.cs` 里未使用的变量 `q`**
  - 清理无用声明
- **`ThemeHelper.cs` 的 `Color?` 用法**
  - 从 `Color?` + `.HasValue` 改为 `TryReadColor(out Color)` 模式
  - 修复 `CS1061: 'Color' does not contain a definition for 'HasValue'`
- **`FilePickerFileTypes` 笔误**
  - 修正为 `FilePickerFileType`（单数）
- **`TtsSynthesizer.OnDone / OnError / OnStart` 签名**
  - .NET 10 Android 绑定层参数类型从 `Java.Lang.String` 改为 `string`
- **`ReclistProvider` 缺失导致的编译错误**
  - 合并 `VoicebankLanguage` 枚举进 `JapaneseCvReclist.cs`

### Deprecated

- `MainPage.xaml` 里的 `Picker` 控件（改用 `SelectionPage`）
- `DisplayActionSheet` 调用（改用 `ActionSheetPage`）

### Security

- 云端 API Key 存储于 Android `Preferences`（未额外加密，请自行评估风险）
- 未收集任何用户数据
- 所有本地处理，不上传云端（云端 TTS 除外，由用户主动启用）

### Technical Notes

- **目标框架** — `net10.0-android`（.NET 10 MAUI）
- **最低 Android 版本** — API 21 (Android 5.0)
- **音频格式** — 强制 44.1kHz / 16-bit PCM / Mono
- **编码**
  - `oto.ini` / `character.txt` — Shift-JIS（带 `EncoderReplacementFallback("?")`）
  - `character.yaml` / `_log.txt` — UTF-8 无 BOM
- **依赖**
  - `Microsoft.Maui.Controls` 10.0.110
  - 无第三方音频库（`WavProcessor` 为纯 C# 实现）
- **代码量**（约）
  - C# 后端：~4500 行
  - XAML 前端：~1200 行
  - 本地化字符串：~150 条 × 3 语言
- **生成声明** — 本项目由 AI 辅助生成，详见 [docs/AI_DISCLOSURE.md](docs/AI_DISCLOSURE.md)

---

## [1.0.0] - 2026-10-16

首个正式版本，实现从 Android 系统 TTS 引擎一键生成 UTAU 声库的基础功能。

### Added

- **多 TTS 引擎** — 自动枚举设备上所有已安装的 TTS 引擎（`TtsEngineHelper`）
- **日语 CV (69 音节)** — 标准单独音 reclist
- **中文 CV (78 音节)** — 覆盖基础音节
- **英语 CV (93 音节)** — 元音 + 辅音组合
- **自定义图标与立绘** — 从相册选择，自动打包
- **双配置输出**
  - `character.txt`（原版 UTAU，Shift-JIS）
  - `character.yaml`（OpenUtau，UTF-8，含立绘字段）
- **自动 ZIP 打包** — 生成完一键压缩
- **系统分享** — 通过 Android 分享面板保存 ZIP
- **全自动 oto.ini** — 基于音频时长的启发式参数估算
- **纯 C# 音频处理** — `WavProcessor` 内置 WAV 解析、重采样、单声道混音、音量规范化
- **自定义输出目录** — 默认 `Download/UtauTtsBuilder/`，可自定义相对路径
- **自动填写声库名** — 选中 TTS 引擎后自动填入引擎名
- **Material Design 3 UI** — 卡片式布局、紫色主题、圆角设计

### Known Issues

- 系统 TTS 为朗读设计，生成的声库拼接痕迹明显
- 部分国产手机 TTS 引擎无日语语音包
- `oto.ini` 为启发式估算，需要手动微调
- 原版 UTAU 对 PNG 图标支持有限

---

## 版本对比链接

[Unreleased]: https://github.com/styisme/UtauTtsBuilder/compare/v1.1.0...HEAD
[1.1.0]: https://github.com/styisme/UtauTtsBuilder/compare/v1.0.0...v1.1.0
[1.0.0]: https://github.com/styisme/UtauTtsBuilder/releases/tag/v1.0.0