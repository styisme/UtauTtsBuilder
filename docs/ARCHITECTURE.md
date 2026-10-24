# UTAU TTS Voicebank Builder 架构解析 (v1.2.0)

本项目是一个基于 **.NET 10 MAUI** 的 Android 应用，旨在将 Android 系统原生的 TTS（文本转语音）引擎与云端 TTS 服务，转换为 UTAU / OpenUtau 可用的声库格式。

为了保证在移动端的流畅度与稳定性，项目采用了**纯 C# 手写音频处理、声明式 UI 动态主题、以及基于罗马音的跨语种转换**。本文档将深度解析项目的核心架构与模块设计。

---

## 一、 整体架构设计

项目采用经典的分层架构设计，将 UI、业务逻辑与平台强依赖代码彻底解耦：

- **UI 层 (`*.xaml`)**：基于 .NET MAUI 的声明式 UI。彻底抛弃了系统默认的组件，实现了完全自定义的 MD3 / 鸿蒙 / UWP / Android4 四种风格切换。
- **服务层 (`Services/`)**：纯 C# 实现的业务核心，包括音频处理、声库构建编排、跨语种转换、日志系统等。这一层**不依赖任何 Android 原生 API**。
- **平台层 (`Platforms/Android/`)**：唯一与 Android 系统 API 交互的边界。负责枚举系统 TTS 引擎、调用底层 `TextToSpeech`、并处理 Android 特有权限。

---

## 二、 核心服务层 (Services)

### 1. 声库构建编排 (VoicebankBuilder)
这是整个生成流程的“总导演”类：
- **并发与缓存**：内部维护 `ttsCache` 字典，若发现当前音节的 TTS 文本与之前重复（如 CVVC 的过渡音），则直接复用已生成的 WAV 文件，节省大量合成时间。
- **分发机制**：自动判断当前引擎是本地引擎还是云端引擎（`__cloud.azure` 等虚拟包名），并调用对应的合成器。
- **容错机制**：支持“断点续传”（跳过已存在的 WAV），并在生成结束后输出详细的 `_log.txt` 和 `_reference.txt`。

### 2. 纯 C# 音频处理 (WavProcessor)
为了摆脱对 NAudio 等 Windows 专属库的依赖，底层完全手写了 WAV 文件的解析与处理：
- **RIFF 解析**：手动读取字节流，定位 `fmt ` 和 `data` 区块，提取采样率、声道数、位深。
- **重采样**：利用**线性插值算法**，将任意采样率（如 TTS 原生输出的 16kHz）统一转换为 UTAU 要求的 **44.1kHz**。
- **静音裁剪**：基于阈值 `0.005f`，自动寻边截掉音频头尾的无用静音，并在两端保留 8ms 的 Padding，极大提升了 `oto.ini` 参数估算的准确度。

### 3. 音素表与跨语种 (ReclistProvider & CrossLingual)
- **数据源**：内置了日（CV/CVVC/VCV）、中（CV/CVVC/VCV）、英（CV）的大量音素映射表。
- **跨语种桥梁**：利用**罗马音/拼音作为中间态**。例如，中→日 模式时，程序会提取中文的拼音（如 `a`），再反查日语 50 音表中的 `a`（あ），最后将“あ”送给只支持日语的 TTS 引擎，实现“曲线救国”。

### 4. 全局日志与异常捕获 (Logger)
- 采用单例模式，按日期滚动写入 `/files/logs/app_YYYYMMDD.log`。
- 在 `MauiProgram` 中挂载了 `AppDomain.UnhandledException` 与 `TaskScheduler.UnobservedTaskException`，确保任何未捕获的崩溃都会被记录在案，方便用户导出日志排查。

---

## 三、 平台层 (Platforms/Android)

### 1. TTS 封装 (TtsSynthesizer)
- **生命周期**：实现了 `TextToSpeech.IOnInitListener`，处理底层引擎的异步初始化。
- **强制制作**：当引擎检测到 `SetLanguage` 返回不支持时，若开启了 Force Build，则自动降级为 `Java.Util.Locale.Default` 继续合成。
- **回调委托**：将底层的 `UtteranceProgressListener` 包装为 C# 的 `TaskCompletionSource`，实现了 `await SynthesizeToFileAsync(...)` 的现代异步调用。

### 2. 接口助手 (TtsEngineHelper)
通过查询系统的 PackageManager，自动枚举出设备上所有安装的 TTS 引擎（如科大讯飞、Google、华为等），并剥离出它们的 Label 与 PackageName。

---

## 四、 UI 与主题系统

### 1. 动态主题引擎 (ThemeService & UiStyleService)
- 定义了四个枚举：`Md3`、`Harmony`、`Uwp`、`Android4`。
- 切换主题时，直接通过代码修改 `Application.Current.Resources` 中的 Color 字典（如 `Md3Primary`），所有通过 `DynamicResource` 绑定的 UI 元素都会**瞬间无缝变色**。
- **动态取色 (Material You)**：在 Android 12+ 上，读取系统壁纸提取的色调（`system_accent1_600` 等），直接覆盖 MD3 的基准色。

### 2. 自定义弹窗组件 (自定义选择器)
为了彻底解决 MAUI 原生 `Picker` / `ActionSheet` 在 Android 上风格割裂的问题，手写了：
- `SelectionPage`：全屏单选列表。
- `ActionSheetPage`：操作菜单列表。
- `AlertPage`：基于半透明遮罩的模态弹窗。
- **状态机修复**：这三者均采用先 `PopAsync` 完成动画后，再通过 `TaskCompletionSource` 回传结果的机制，彻底修复了 MAUI 的导航冲突 Bug。

---

## 五、 构建与 CI/CD

### 1. 版本号自动注入
项目根目录的 `.github/workflows/build-android.yml` 会在每次编译时，通过正则表达式提取 `CHANGELOG.md` 中最新的 `## [x.y.z]`。
- 将该版本号注入到 `.csproj` 的 `ApplicationDisplayVersion`。
- 使用 `YYYYMMDD` 注入到 `ApplicationVersion`（确保每次编译的整数版本号都在递增）。

### 2. 云端编译矩阵
利用 GitHub Actions 提供的免费 Ubuntu 服务器，执行 `dotnet workload install maui-android` 并打包 APK，最终将产物直接上传至 Artifacts 供用户下载，实现了零本地环境依赖的发布流程。