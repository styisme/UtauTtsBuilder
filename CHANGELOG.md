# Changelog

本项目的重要变更都记录在此文件。

格式基于 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.0.0/)，
版本号遵循 [Semantic Versioning](https://semver.org/lang/zh-CN/)。

> **版本号说明**：v1.0.0 之前的版本号未严格遵循 SemVer。

## [Unreleased]

### Planned
- 更精确的 oto.ini 自动估算算法（基于频谱分析）
- 支持韩语 / 西班牙语 reclist
- 声库生成完成后可直接调用 OpenUtau 试听
- 支持批量导入自定义 reclist（外部 txt 文件）

## [1.0.0] - 2026-10-16

首个正式版本，功能完整，可用于生成日语 / 中文 / 英语 UTAU CV 声库。

### Added
- 🗣️ **多 TTS 引擎支持**：自动枚举设备上已安装的 TTS 引擎（科大讯飞、Google TTS、华为等）
- 🌏 **多语言 reclist**：
  - 日语 CV（69 音节）
  - 中文 CV（78 音节）
  - 英语 CV（93 音节）
- 🎨 **自定义图标与立绘**：从相册选择图片，自动打包进声库目录
- 📝 **双配置输出**：同时生成 `character.txt`（原版 UTAU）和 `character.yaml`（OpenUtau）
- 🗜️ **自动 ZIP 打包**：生成完成后一键压缩
- 📤 **系统分享**：通过 Android 分享面板将 ZIP 保存到任意 App
- 🤖 **全自动 oto.ini 生成**：基于音频时长的启发式参数估算
- 🔊 **纯 C# 音频处理**：内置 WAV 解析、重采样（→44100Hz）、单声道混音、音量规范化
- 📁 **自定义输出目录**：支持默认路径（Download/UtauTtsBuilder/）或自定义相对路径
- ✍️ **自动填写声库名**：选中 TTS 引擎后自动填入引擎名
- 🎯 **现代化 UI**：卡片式布局、紫色主题、圆角设计

### Technical
- 基于 **.NET 10 MAUI**（`net10.0-android`）
- 使用 GitHub Actions 云端自动构建 APK
- 音频处理不依赖任何第三方库（NAudio 等）
- 生成 `oto.ini` / `character.txt` 时使用 **Shift-JIS** 编码并带替换回退，避免乱码崩溃

### Known Issues
- 系统 TTS 为朗读设计，生成的声库拼接痕迹明显
- 部分国产手机 TTS 引擎无日语语音包，程序会提示"不支持"
- 原版 UTAU 对 PNG 图标支持有限，建议使用 BMP

[Unreleased]: https://github.com/styisme/UtauTtsBuilder/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/styisme/UtauTtsBuilder/releases/tag/v1.0.0