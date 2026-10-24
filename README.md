**🌐 语言 / Language / 言語：** 中文 | [English](README.en.md) | [日本語](README.ja.md)

---

# UTAU TTS Voicebank Builder

> 从 Android 系统 TTS 引擎一键生成 UTAU 声库的 .NET MAUI 应用
> Generate UTAU voicebanks from Android system TTS engines with one tap.

[![Build Android APK](https://github.com/styisme/UtauTtsBuilder/actions/workflows/build-android.yml/badge.svg)](https://github.com/styisme/UtauTtsBuilder/actions/workflows/build-android.yml)
[![Version](https://img.shields.io/badge/version-1.2.0-512BD4)](CHANGELOG.md)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![Made with AI](https://img.shields.io/badge/made%20with-AI-FF6B6B)](docs/AI_DISCLOSURE.md)
[![Platform](https://img.shields.io/badge/platform-Android%205.0%2B-brightgreen)]()

---

## ⚠️ AI 生成声明

> **本项目由 AI 辅助生成。**
>
> 绝大多数代码（包括架构、业务逻辑、UI 布局、文档）由 **大型语言模型（LLM）** 在开发者指导下生成，并经过人工审阅、测试与修正。开发者负责需求定义、功能验收、bug 定位与最终发布决策。
>
> **请注意**：
> - 代码可能包含未预料的边缘情况或潜在 bug
> - 不保证在生产环境的稳定性或安全性
> - 请自行评估风险后再用于重要场景
> - 欢迎提交 Issue 报告问题，但**不承诺响应时效**
>
> 详细说明请见 [docs/AI_DISCLOSURE.md](docs/AI_DISCLOSURE.md)。

---

## 📦 最新版本

当前版本：**v1.2.0**

完整版本历史请见 [CHANGELOG.md](CHANGELOG.md)。

---

## ✨ 功能特性

### 声库生成与高级玩法
- 🗣️ **多 TTS 引擎** — 自动枚举设备上所有已安装的本地 TTS 引擎
- ☁️ **云端 TTS** — 支持 Azure Speech 和 Google Cloud TTS（需自备 API Key）
- 🌏 **多语言声库** — 日语、中文、英语
  - 日语：CV (129) / CVVC (194) / VCV (105)
  - 中文：CV (78) / CVVC (203) / VCV (130)
  - 英语：CV (93)
- 🌍 **跨语种合成** — 支持中→日、日→中、英→日、英→中等 7 种转换模式，支持智能匹配，用不支持目标语言的引擎也能生成（基于罗马音/拼音桥梁）
- 🛡️ **强制制作** — 开启后，目标 TTS 引擎若不支持所选语言，自动降级使用系统默认语言合成，防止中断
- ⚡ **多引擎批量生成** — 一键为选中的多个 TTS 引擎分别生成不同的声库
- ⏱️ **生成时间预估** — 点下生成前，提前展示预计耗时和音节总数
- 📥 **自定义音素表** — 导入任意 `.txt` 音素表，导入前提供预览确认

### 生产力与诊断工具
- 📂 **声库管理列表** — 扫描并列出所有已生成的声库，支持一键分享 ZIP、编辑 `oto.ini`、生成 UST、删除等操作
- ⚖️ **引擎音质对比** — 快速调用所有可用引擎朗读同一段音节，方便 A/B 试听对比
- 📊 **全局运行日志** — 记录所有异常、菜单操作、网络请求结果。支持实时查看、清空与分享日志文件
- 📝 **构建追踪** — 每个声库自动生成 `_log.txt`（记录每个音节耗时）和 `_reference.txt`（发音参考表）
- 🖼️ **波形与预览图** — 支持查看单个音节波形，生成完成后自动绘制一张声库预览图（自适应深色模式）

### 音频处理
- 🔊 **纯 C# 音频处理** — 不依赖任何第三方库
- ✂️ **首尾静音自动裁剪** — 阈值 0.005f，两端各留 8ms padding
- 🎚️ **语速 / 音高调节** — 主页实时滑块（0.5–1.5× / 0.5–2.0×）
- 🔄 **断点续传** — 跳过已存在的音频（可关闭）

### 界面与体验
- 🎨 **四种 UI 风格** — Material 3 / 鸿蒙 HarmonyOS / Windows UWP / Android 4 Holo
- 🌗 **深色模式** — 跟随系统 / 浅色 / 深色
- 🎨 **动态取色** — Android 12+ 壁纸动态取色（可关闭）
- 🌍 **三语 UI** — 中文 / English / 日本語（跟随系统或手动切换）
- 🔀 **完全自定义组件** — 抛弃系统 Picker / ActionSheet / Alert，所有弹窗完全跟随当前 UI 风格
- ⚙️ **二级菜单与设置** — 主页右上角 `⋯` 更多菜单统一收纳高级功能，设置页集中管理外观与引擎

---

## 📱 使用说明

### 基础流程
1. 从 [Releases](../../releases/latest) 下载最新 APK 并安装
2. 首次启动时**允许「文件和媒体」和「网络」访问权限**
3. 依次选择：
   - **声库语言**（日语 / 中文 / 英语，CV / CVVC / VCV）
   - **跨语种模式**（默认原生，可让不支持日语的引擎读中文映射出的日语音节）
   - **TTS 引擎**（选中后声库名会自动填入）
   - **语速 / 音高**（建议先用默认值）
   - **声库名称**
   - 图标 / 立绘 / 作者（均可选）
4. 点击 **「开始生成声库」**，确认预估时间
5. 生成完成后弹出操作菜单：
   - **查看波形/试听** → 打开详情页
   - **编辑 oto.ini** → 通过分享面板选择文本编辑器
   - **生成 UST 模板** → 写入 `test.ust` 到声库目录
   - **分享 ZIP** → 系统分享面板

### 高级功能（右上角 `⋯` 菜单）
- **已生成的声库**：管理历史生成产物
- **多引擎批量生成**：一键跑完所有引擎
- **引擎音质对比**：快速试听各引擎音色
- **检查更新**：手动拉取 GitHub Releases
- **运行日志**：排查错误必备
- **清除缓存**：释放临时空间

### 使用云端 TTS（可选）
1. 设置页 → **云端 TTS 配置**
2. 填入 Azure Speech 或 Google Cloud 的 API Key
3. 返回主页，引擎列表会多出 `☁ Azure (...)` 或 `☁ Google Cloud (...)`

**Azure 配置**：
- 在 [Azure Portal](https://portal.azure.com/) 创建 Speech 服务
- 复制 **Key** 和 **Region**（如 `eastasia`）
- 语音名（如 `ja-JP-NanamiNeural`）可留空使用默认

**Google 配置**：
- 在 [Google Cloud Console](https://console.cloud.google.com/) 启用 **Text-to-Speech API**
- 创建 API Key 并填入

> ⚠️ 云端 TTS 需要网络。国内网络可能无法访问 Google。

### 输出位置
```
内部存储/Download/UtauTtsBuilder/{声库名}/
```

---

## 📂 输出结构

```
{声库名}/
├── あ.wav             # 音节音频（44.1kHz / 16-bit / Mono）
├── か.wav
├── ちゃ.wav           # 拗音
├── a_k.wav            # CVVC 过渡音
├── ...
├── oto.ini            # UTAU 参数文件（Shift-JIS）
├── character.txt      # UTAU 声库描述（Shift-JIS）
├── character.yaml     # OpenUtau 声库描述（UTF-8，含立绘）
├── preview.png        # 声库预览图（自动生成）
├── icon.png           # 图标（可选）
├── portrait.png       # 立绘（可选）
├── test.ust           # UST 模板（可选生成）
├── _log.txt           # 生成日志（耗时统计）
└── _reference.txt     # 语音参考表（别名与 TTS 文本映射）
```

---

## 🛠️ 自行编译

### 前置要求
| 组件 | 版本 |
|---|---|
| .NET SDK | 10.0 或更高 |
| .NET MAUI workload | `dotnet workload install maui-android` |
| JDK | 17 |
| Android SDK | API 21+ |

### 本地构建
```bash
git clone https://github.com/styisme/UtauTtsBuilder.git
cd UtauTtsBuilder
dotnet restore
dotnet build -f net10.0-android -c Release
```

### GitHub Actions 云端构建
项目已配置 `.github/workflows/build-android.yml`：
- 每次 push 到 `main` 自动编译
- 手动触发：Actions 页面 → **Run workflow**
- 编译产物在 **Artifacts** 区域下载

**版本号自动读取**：Actions 会从 `CHANGELOG.md` 提取最新的 `## [x.y.z]` 作为应用版本号。

> 💡 **没有本地开发环境？** 直接 Fork 仓库，在 GitHub 网页上改代码，让 Actions 帮你编译。

---

## ⚠️ 已知限制

- **TTS 质量决定一切** — 系统 TTS 为朗读设计，缺乏歌唱所需的稳定元音延展性，拼接痕迹明显
- **CVVC / VCV 需要手动微调** — 生成的 oto.ini 参数为估算值，强烈建议用 [vLabeler](https://github.com/sdercolin/vlabeler) 或 [setParam](https://github.com/LiCybora/setParam) 手动调整
- **跨语种可能存在口音** — 用罗马音桥梁转换出的发音可能带有原语言的腔调
- **云端 TTS 依赖网络** — 且国内访问 Google 可能受限
- **图标建议用 BMP** — 原版 UTAU 对 PNG 支持有限，OpenUtau 无此限制
- **AI 生成代码可能含未知 bug** — 详见 AI 生成声明

---

## 🤝 贡献

欢迎提交 Issue 和 PR！**提交前请先阅读 [CONTRIBUTING.md](CONTRIBUTING.md)**，其中包含：
- 本地开发环境配置
- 代码风格与提交规范（Conventional Commits）
- 分支模型与 PR 流程
- 发布流程与版本号规则

---

## 📜 协议

本项目使用 [MIT License](LICENSE)。

**特别声明**：
- 生成的声库音频版权归所使用的 TTS 引擎服务提供方所有
- 请遵守对应 TTS 引擎的服务条款
- **不要**将生成的声库用于商业用途或侵犯他人权益的场景
- **不要**模仿现实中的真人声音

---

## 🙏 致谢

- [.NET MAUI](https://github.com/dotnet/maui) — 跨平台框架
- [UTAU](http://utau2008.xrea.jp/) — 经典歌声合成软件
- [OpenUtau](https://github.com/stakira/OpenUtau) — 现代 UTAU 替代方案
- [Keep a Changelog](https://keepachangelog.com/zh-CN/1.0.0/) — 变更日志规范
- [Conventional Commits](https://www.conventionalcommits.org/zh-hans/) — 提交规范
- 所有参与 AI 生成与人工审阅的贡献者

---

## 📚 相关文档

| 文档 | 说明 |
|---|---|
| [CHANGELOG.md](CHANGELOG.md) | 版本变更历史 |
| [CONTRIBUTING.md](CONTRIBUTING.md) | 贡献指南 |
| [docs/AI_DISCLOSURE.md](docs/AI_DISCLOSURE.md) | AI 生成详细说明 |
| [LICENSE](LICENSE) | MIT 协议 |

---

**⚠️ 免责声明**：本项目为实验性工具，代码主要由 AI 生成。使用本工具产生的任何后果由使用者自行承担。生成的声库音频来自设备上的系统 TTS 引擎或第三方云端服务，请遵守对应服务条款。