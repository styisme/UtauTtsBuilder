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
- 导出为 DiffSinger / NNSVS / ENUNU 格式
- 支持 Web（Blazor）

---

## [1.2.0] - 2026-10-24

本次更新是项目在**生产力与可用性**上的重大飞跃，新增了跨语种、批量处理、声库管理、运行时日志及全局异常捕获等核心功能。

### Added

#### 🌍 跨语种与语言强制

- **跨语种转换（Cross-Lingual）**
  - 主页新增「跨语种转换」下拉框
  - 支持模式：原生、中→日、日→中、英→日、英→中、日→英、中→英
  - **智能匹配**：根据当前选择的语言自动推断转换方向
  - 底层基于罗马音/拼音映射表，将源语言文本转为目标 TTS 支持的字符再合成
- **强制制作（Force Build）**
  - 设置页新增「高级设置 → 强制制作」开关
  - 当目标 TTS 引擎不支持所选语言时，自动回退到系统默认语言强制生成（不再报错中断）

#### 🛠️ 生产力工具

- **多引擎批量生成**
  - 通过主页「⋯」菜单进入
  - 勾选多个可用引擎后，一键为每个引擎生成对应的声库（名称自动附加引擎后缀）
- **声库管理列表（Voicebank List）**
  - 扫描默认输出目录，列表化展示所有已生成的声库
  - 显示音节数与占用空间
  - 支持快捷操作：查看详情、分享 ZIP、编辑 `oto.ini`、生成 UST、删除
- **音质对比（Quick Compare）**
  - 快速使用所有可用引擎合成同一个音节，供 A/B 试听对比
  - 自动处理不支持目标语言的引擎（自动跳过或降级）
- **生成时长预估**
  - 点击生成前，弹窗显示当前配置下的音节数与预估耗时
- **音素表预览**
  - 导入自定义 `reclist` 时，先展示前 20 条解析结果供确认

#### 📊 日志与诊断

- **全局日志系统（Logger）**
  - 新增 `Services/Logger.cs`，按日期滚动记录，保留 7 天
  - 全局捕获 `AppDomain.UnhandledException` 与 `TaskScheduler.UnobservedTaskException`
  - 主页「⋯」菜单新增「运行日志」入口
  - 日志页面支持实时刷新、分享文件、清空日志
- **详细的构建日志**
  - `_log.txt` 增强：记录每个音节的合成耗时，输出慢/快/平均耗时统计
  - 附带 `_reference.txt` 导出，列出所有别名对应的 TTS 文本与文件名

#### 🎨 交互与展示

- **自定义弹窗（AlertPage）**
  - 彻底抛弃系统原生 `DisplayAlert`，改用 MD3 风格的自定义弹窗页面
  - 支持单按钮、双按钮模式，自动跟随主题与深浅色
- **波形查看器（WaveformView）**
  - 声库预览页支持展示单音节音频波形图（基于 C# 纯代码 `GraphicsView` 绘制）
- **声库预览图生成**
  - 生成完成后自动在声库目录下创建 `preview.png`
  - 包含声库名、音节数、日期，自适应深浅色模式
- **更新检查（Update Service）**
  - 启动时静默检查 GitHub Releases
  - 手动触发检查时，弹窗提示当前与最新版本号，支持一键跳转下载

### Fixed

- **导航冲突修复**：`ActionSheetPage` / `SelectionPage` / `AlertPage` 统一改为先 `Pop` 完成动画后再返回结果，解决了"弹窗闪退/卡死"问题。
- **缺失网络权限**：`AndroidManifest.xml` 补充 `INTERNET` 与 `ACCESS_NETWORK_STATE` 权限，解决更新检查与云端 TTS 失败。
- **UI 割裂修复**：全局移除了系统 `DisplayActionSheet`、`DisplayAlert`，统一使用 MD3 组件。
- **本地化文本缺失**：修复了 `Localization.cs` 缺失 key 导致界面上直接显示变量名的问题（如 `list_title`）。
- **拼接与调用错误**：修复了 `VoicebankBuilder` 中的各种字符串拼接和异步调用冲突。

### Changed

- **多语言与主题检测**：`TtsSynthesizer` 初始化时增加 `forceBuild` 参数。
- **UI 重构**：主页右上角新增「⋯」更多菜单，集中收纳列表、批量、对比、日志等二级功能。
- **依赖变更**：清理了部分未使用的库，补充了 `System.Text.Json` 用于更新检查。

### Technical Notes

- 跨语种映射基于 `ReclistProvider` 中的 `ChineseCvList` / `EnglishCvList` / `JapaneseCvReclist` 进行反向查找。
- 强制制作模式将传递 `Java.Util.Locale.Default` 给底层 TTS 引擎。
- 日志文件位于 `/data/user/0/com.example.utauttsbuilder/files/logs/`。

---

## [1.1.1] - 2026-10-16

修复移动端 UTAU 无法发音的问题，扩展日语 CV 音节表。

### Fixed

- **拗音无法发音** —— `ちゃ` / `しゃ` / `きゅ` 等拗音不在 reclist 里导致 UTAU 静音
- **文件名格式不匹配移动端 UTAU** —— 文件名改用假名（`あ.wav` 而非 `a.wav`）
- **cutoff 参数符号错误** —— 从负数改为正数，匹配常见声库格式
- **参数范围过大** —— `offset` 从 20–40 缩小到 8–15

### Changed

- **日语 CV 从 69 → 129 音节**
  - 新增拗音清 21：きゃ / きゅ / きょ / しゃ / しゅ / しょ / ちゃ / ちゅ / ちょ / にゃ / にゅ / にょ / ひゃ / ひゅ / ひょ / みゃ / みゅ / みょ / りゃ / りゅ / りょ
  - 新增拗音浊 12：ぎゃ / ぎゅ / ぎょ / じゃ / じゅ / じょ / ぢゃ / ぢゅ / ぢょ / びゃ / びゅ / びょ
  - 新增拗音半浊 3：ぴゃ / ぴゅ / ぴょ
  - 新增外来语音节 22：ふぁ / ふぃ / ふぇ / ふぉ / てぃ / でぃ / うぃ / うぇ / うぉ / しぇ / じぇ / ちぇ / つぁ / つぃ / つぇ / つぉ / とぅ / どぅ / ヴぁ / ヴぃ / ヴぇ / ヴぉ
- **日语 CVVC 从 133 → 194 音节**（随 CV 表扩展自动增长）
- **UI 显示名**：三语都同步更新（中 / 英 / 日）

### Technical Notes

- `OtoGenerator.GenerateEntry()` 参数策略调整：
  - `offset`：8–15ms
  - `cutoff`：正数（从固定点往后算）
  - `preutterance`：15–20ms
  - `overlap`：10–15ms
- `ReclistProvider` 的 `FileName` 改为 `Alias`（假名），不再用罗马音

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
  - 云端密钥通过 `CloudConfigService` 存于 `Preferences`
- **新增 `CloudSettingsPage`**
  - 独立的云端 TTS 配置页面
  - 支持 Azure API Key / Region / Voice
  - 支持 Google API Key
  - 输入框带密码遮罩

#### 📚 音素表

- **日语 CVVC (133 音节)** —— 68 CV + 20 VV 过渡 + 45 VC 过渡
- **日语 VCV (105 音节)** —— 5 单独元音 + 6 ん相关 + 20 VV + 70 辅音过渡
- **中文 CVVC (203 音节)** —— 78 CV + 20 VV 过渡 + 105 VC 过渡
- **中文 VCV (130 音节)** —— 5 元音 + 20 VV + 105 声母行过渡
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
- **三语 UI**
  - 中文 / English / 日本語
  - 默认跟随系统语言
  - 设置页可手动切换
  - 覆盖所有界面文本、进度消息、弹窗、reclist 显示名
- **完全自定义选择器**
  - 新增 `SelectionPage` 替代 MAUI `Picker`
  - 新增 `ActionSheetPage` 替代 `DisplayActionSheet`
  - 两种页面完全跟随当前 UI 风格
- **设置页面重构**
  - 隐藏系统导航栏，改用页面内自定义 TopBar
  - 分区：外观 / UI 风格 / 语言 / 动态取色 / 云端 / 关于
  - 主题切换带淡入淡出动画（90ms 淡出 + 180ms 淡入）
- **关于页面**
  - 应用图标 + 名称 + 版本号
  - GitHub 仓库链接
  - MIT 协议链接

#### 🎵 音频处理

- **首尾静音裁剪** —— `WavProcessor.TrimSilence()` 阈值 0.005f，两端各留 8ms
- **语速 / 音高调节** —— 主页两个滑块（0.5–1.5× / 0.5–2.0×）
- **断点续传** —— 主页「跳过已存在的音频」复选框（默认开启）

#### 📄 声库输出

- **UST 模板生成** —— 自动生成 `test.ust`（3 个音符，Tempo 120）
- **生成日志** —— 每个声库目录写入 `_log.txt`
- **完成后操作菜单** —— 编辑 oto.ini / 生成 UST / 分享 ZIP

#### 🖼️ 图片预览

- **图标缩略图** —— 选中后 64×64 预览
- **立绘缩略图** —— 选中后 64×96 预览

#### ⚙️ 构建与发布

- **版本号自动注入** —— GitHub Actions 从 `CHANGELOG.md` 提取版本号
- **APK 自动重命名** —— `UtauTtsBuilder-v1.1.0.apk`
- **CI 优化** —— `setup-java@v5` + 手动指定 Android SDK 环境变量

### Changed

- **引擎列表** — 从 `Picker` 改为 `SelectionPage`；支持本地 + 云端混合
- **主题系统重构** — 色板移到 `App.xaml`；`StaticResource` → `DynamicResource`
- **`MauiProgram.cs`** — 新增 Entry / Picker / Switch / RadioButton Handler
- **导航栏** — `SettingsPage` / `AboutPage` / `CloudSettingsPage` 隐藏系统导航栏

### Fixed

- **启动器图标** — `appicon.svg` 改为完整图标 + `BaseSize="456,456"`
- **设置页 Switch 白底** — Android Handler 设置 `BackgroundTintList = Transparent`
- **导航栏风格分裂** — 改用页面内 TopBar
- **`ThemeHelper.cs` 的 `Color?` 用法** — 改为 `TryReadColor(out Color)`
- **`FilePickerFileTypes` 笔误** — 改为 `FilePickerFileType`
- **`TtsSynthesizer` 回调签名** — .NET 10 Android 绑定层参数类型改为 `string`

### Deprecated

- `MainPage.xaml` 里的 `Picker` 控件（改用 `SelectionPage`）
- `DisplayActionSheet` 调用（改用 `ActionSheetPage`）

### Security

- 云端 API Key 存储于 Android `Preferences`（未额外加密）
- 未收集任何用户数据
- 所有本地处理，不上传云端（云端 TTS 除外）

### Technical Notes

- **目标框架** — `net10.0-android`
- **最低 Android 版本** — API 21 (Android 5.0)
- **音频格式** — 强制 44.1kHz / 16-bit PCM / Mono
- **编码**
  - `oto.ini` / `character.txt` — Shift-JIS（带替换回退）
  - `character.yaml` / `_log.txt` — UTF-8 无 BOM
- **依赖**
  - `Microsoft.Maui.Controls` 10.0.110
  - 无第三方音频库
- **生成声明** — 本项目由 AI 辅助生成，详见 [docs/AI_DISCLOSURE.md](docs/AI_DISCLOSURE.md)

---

## [1.0.0] - 2026-10-16

首个正式版本，实现从 Android 系统 TTS 引擎一键生成 UTAU 声库的基础功能。

### Added

- **多 TTS 引擎** — 自动枚举设备上所有已安装的 TTS 引擎
- **日语 CV (69 音节)** — 标准单独音 reclist
- **中文 CV (78 音节)** — 覆盖基础音节
- **英语 CV (93 音节)** — 元音 + 辅音组合
- **自定义图标与立绘** — 从相册选择，自动打包
- **双配置输出** —— `character.txt`（UTAU）+ `character.yaml`（OpenUtau）
- **自动 ZIP 打包** — 生成完一键压缩
- **系统分享** — 通过 Android 分享面板保存 ZIP
- **全自动 oto.ini** — 基于音频时长的启发式参数估算
- **纯 C# 音频处理** — `WavProcessor` 内置 WAV 解析、重采样、单声道混音
- **自定义输出目录** — 默认 `Download/UtauTtsBuilder/`
- **自动填写声库名** — 选中 TTS 引擎后自动填入引擎名
- **Material Design 3 UI** — 卡片式布局、紫色主题、圆角设计

### Known Issues

- 系统 TTS 为朗读设计，生成的声库拼接痕迹明显
- 部分国产手机 TTS 引擎无日语语音包
- `oto.ini` 为启发式估算，需要手动微调
- 原版 UTAU 对 PNG 图标支持有限

---

## 版本对比链接

[Unreleased]: https://github.com/styisme/UtauTtsBuilder/compare/v1.2.0...HEAD
[1.2.0]: https://github.com/styisme/UtauTtsBuilder/compare/v1.1.1...v1.2.0
[1.1.1]: https://github.com/styisme/UtauTtsBuilder/compare/v1.1.0...v1.1.1
[1.1.0]: https://github.com/styisme/UtauTtsBuilder/compare/v1.0.0...v1.1.0
[1.0.0]: https://github.com/styisme/UtauTtsBuilder/releases/tag/v1.0.0