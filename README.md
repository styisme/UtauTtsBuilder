# UTAU TTS Voicebank Builder

> 从 Android 系统 TTS 引擎一键生成 UTAU 声库的 .NET MAUI 应用
> Generate UTAU voicebanks from Android system TTS engines with one tap.

[![Build Android APK](https://github.com/styisme/UtauTtsBuilder/actions/workflows/build-android.yml/badge.svg)](https://github.com/styisme/UtauTtsBuilder/actions/workflows/build-android.yml)
[![Version](https://img.shields.io/badge/version-1.0.0-512BD4)](CHANGELOG.md)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![Changelog](https://img.shields.io/badge/changelog-keep--a--changelog-orange)](CHANGELOG.md)
[![Contributing](https://img.shields.io/badge/contributing-welcome-brightgreen)](CONTRIBUTING.md)

---

## 📦 最新版本

当前版本：**v1.0.0**（2026-10-16）

完整版本历史请见 [CHANGELOG.md](CHANGELOG.md)。

---

## ✨ 功能特性 / Features

- 🗣️ **多 TTS 引擎** — 自动枚举设备上所有已安装的 TTS 引擎（小艺、科大讯飞、Google TTS 等）
- 🌏 **多语言声库** — 支持 **日语 CV (69)**、**中文 CV (78)**、**英语 CV (93)** 三种 reclist
- 🎨 **自定义图标与立绘** — 从相册选择图片，自动打包进声库
- 📝 **双配置输出** — 同时生成 `character.txt`（UTAU）和 `character.yaml`（OpenUtau）
- 🗜️ **自动 ZIP 打包** — 生成完一键压缩并分享到任意 App
- 🤖 **全自动 oto.ini** — 启发式参数估算，无需手动标注
- 💾 **本地处理** — 所有音频合成在设备上完成，不上传云端

---

## 📱 使用说明 / How to Use

1. 从 [Releases](../../releases/latest) 下载最新 APK 并安装
2. 首次启动时**允许「文件和媒体」访问权限**
3. 依次选择：
   - **声库语言**（日语 / 中文 / 英语）
   - **TTS 引擎**（选中后声库名会自动填入）
   - **声库名称**
   - 图标 / 立绘 / 作者（均可选）
4. 点击 **「开始生成声库」**，等待进度完成
5. 生成好的声库会保存在：
   ```
   内部存储/Download/UtauTtsBuilder/{声库名}/
   ```
6. 生成的 `.zip` 可通过系统分享面板保存到任意位置

---

## 📂 输出结构 / Output Structure

```
{声库名}/
├── a.wav            # あ
├── i.wav            # い
├── ...              # 所有音节
├── oto.ini          # UTAU 参数文件（Shift-JIS）
├── character.txt    # UTAU 声库描述（Shift-JIS）
├── character.yaml   # OpenUtau 声库描述（UTF-8）
├── icon.png         # 图标（可选）
└── portrait.png     # 立绘（可选）
```

---

## 🛠️ 自行编译 / Build from Source

### 前置要求
- **.NET 10 SDK**
- **.NET MAUI 工作负载** (`dotnet workload install maui-android`)
- **Android SDK** (API 21+)
- **JDK 17**

### 本地构建
```bash
git clone https://github.com/styisme/UtauTtsBuilder.git
cd UtauTtsBuilder
dotnet restore
dotnet build -f net10.0-android -c Release
```

### 使用 GitHub Actions 云端构建
本项目已配置好 `.github/workflows/build-android.yml`，每次 push 到 `main` 或手动触发都会自动编译 APK，编译产物在 Actions 页面的 **Artifacts** 区域下载。

> 💡 **没有本地开发环境？** 直接 Fork 仓库，在 GitHub 网页上修改代码，让 Actions 帮你编译 APK，从 Artifacts 下载到手机安装测试即可。

---

## ⚠️ 已知限制 / Known Limitations

- **TTS 质量决定一切**：系统 TTS 是为朗读设计的，缺乏歌唱所需的稳定元音延展性，生成的声库拼接痕迹会比较明显
- **国产手机 TTS 的日语支持有限**：部分设备可能没有日语语音包，程序会自动检测并提示
- **oto.ini 为启发式估算**：可用但不够精确，建议用 [vLabeler](https://github.com/sdercolin/vlabeler) 或 [setParam](https://github.com/LiCybora/setParam) 手动微调
- **图标建议用 BMP**：原版 UTAU 对 PNG 支持有限，OpenUtau 无此限制

---

## 🤝 贡献 / Contributing

欢迎提交 Issue 和 PR！**提交前请先阅读 [CONTRIBUTING.md](CONTRIBUTING.md)**，其中包含：

- 本地开发环境配置
- 代码风格与提交规范（Conventional Commits）
- 分支模型与 PR 流程
- 发布流程与版本号规则

**近期想优先做的事**：

- 更精确的 `oto.ini` 自动估算算法
- 更多语言 reclist（韩语、西班牙语等）
- UI/UX 改进

---

## 📜 协议 / License

本项目使用 [MIT License](LICENSE)。

---

## 🙏 致谢 / Credits

- [.NET MAUI](https://github.com/dotnet/maui) — 跨平台框架
- [UTAU](http://utau2008.xrea.jp/) — 经典歌声合成软件
- [OpenUtau](https://github.com/stakira/OpenUtau) — 现代 UTAU 替代方案
- [Keep a Changelog](https://keepachangelog.com/zh-CN/1.0.0/) — 变更日志规范
- [Conventional Commits](https://www.conventionalcommits.org/zh-hans/) — 提交规范

---

## 📚 相关文档

| 文档 | 说明 |
|---|---|
| [CHANGELOG.md](CHANGELOG.md) | 版本变更历史 |
| [CONTRIBUTING.md](CONTRIBUTING.md) | 贡献指南 |
| [LICENSE](LICENSE) | MIT 协议 |

---

**⚠️ 免责声明**：本项目生成的声库音频来自设备上的系统 TTS 引擎。请遵守所使用 TTS 引擎的服务条款，不要将生成的声库用于商业用途或侵犯他人权益的场景。