# UTAU TTS Voicebank Builder 数据与格式规范 (v1.2.0)

本项目在生成声库时，遵循原版 UTAU 与 OpenUtau 的严格文件标准。同时，为了提供更高的扩展性，App 内部定义了自定义音素表与云端 TTS 的数据交互格式。

本文档将详细说明这些格式规范，供高级用户编写自定义脚本或进行深度调教时参考。

---

## 一、 自定义音素表导入规范 (Reclist `.txt`)

为了让 App 能够解析并交给 TTS 引擎合成，自定义音素表必须遵循以下纯文本格式（UTF-8 编码）：

### 1. 基础格式
每行代表一个音节，支持三种分隔符（`=`、`,` 或 `空格/Tab`）：
```text
# 格式 1：等号分隔
别名=交给TTS读取的文本
# 格式 2：逗号分隔
别名,交给TTS读取的文本
# 格式 3：空格分隔
别名 交给TTS读取的文本
```

**示例（日语 CV）：**
```text
あ=a
い,i
か ka
```

### 2. 特殊指令
支持在文件头部或任意位置使用 `#` 或 `//` 开头声明 TTS 发音语言，App 会自动识别：
```text
# language: ja-JP
# 可选值：ja-JP（日语）, zh-CN（中文）, en-US（英语）
```
如果没有声明该指令，App 会在导入时弹窗要求用户手动选择。

---

## 二、 `oto.ini` 生成参数规范 (Shift-JIS 编码)

原版 UTAU 的 `oto.ini` 包含 5 个核心参数。由于系统 TTS 生成的音频存在首尾静音和机械感，本项目的 `OtoGenerator` 服务采用了动态启发式估算策略，而非固定死值。

`oto.ini` 格式：
```text
文件名.wav=别名,左边界(Offset),固定(Consonant),右边界(Cutoff),先行发声(Preutterance),重叠(Overlap)
```

### 1. 参数定义与本项目策略
- **左边界 (Offset)**: 音频开始的无效部分。
  - *本项目策略*：常规音节约 `8–15ms`。
- **固定 (Consonant)**: 辅音结束、元音开始的位置。
  - *本项目策略*：基于音频总时长的 `15% - 30%` 进行估算。如果是元音单音（如“あ”），则提前固定点。
- **右边界 (Cutoff)**: 音频截断位置（**注意：本项目生成正数**，表示从固定点向后推算的毫秒数）。
  - *本项目策略*：常规音节约为时长 `30%` 的正数，确保切入元音部分。
- **先行发声 (Preutterance)**: 声音开始播放的位置，通常与固定点相差几十毫秒。
  - *本项目策略*：`固定点 - (15~20ms)`。
- **重叠 (Overlap)**: 两个音符重叠的长度。
  - *本项目策略*：`先行发声 × 0.5`。

### 2. 跨语种 (CVVC/VCV) 过渡音策略
对于 `a k` 这类过渡音，由于 TTS 实际合成的是一整段 `あか`，本项目的估算会将 `Cutoff` 设为音频总时长的一半左右，截取前半段作为过渡音。

---

## 三、 声库描述文件规范 (Meta)

本工具会自动生成两份描述文件，以兼容不同的歌声合成软件。

### 1. `character.txt` (原版 UTAU)
- **编码**: Shift-JIS
- **规范**: 仅包含纯文本的 `键=值` 结构。
- **支持字段**:
  ```text
  name=声库名称
  image=icon.png  # 仅支持 BMP/PNG
  author=作者名
  ```

### 2. `character.yaml` (OpenUtau)
- **编码**: UTF-8 (无 BOM)
- **规范**: 遵循 YAML 语法。支持更丰富的高级配置。
- **生成示例**:
  ```yaml
  name: 声库名称
  image: icon.png
  portrait: portrait.png
  portrait_opacity: 0.67
  portrait_height: 720
  author: 作者名
  text_file_encoding: shift_jis
  default_phonemizer: OpenUtau.Core.DefaultPhonemizer
  subbanks:
  - color: ''
    prefix: ''
    suffix: ''
    tone_ranges:
    - C1-B7
  ```

---

## 四、 日志与排错文件结构

每次生成结束后，声库目录下会自动生成以下辅助文件：

### 1. `_log.txt`
- **编码**: UTF-8
- **内容**: 记录了引擎初始化、每个音节的合成耗时（毫秒）、共用音频（共享）情况、失败原因，以及最终的耗时统计（最快/最慢/平均）。

### 2. `_reference.txt`
- **编码**: UTF-8
- **内容**: 发音参照表。清晰记录了 `UTAU别名`、`实际交给TTS的文本`、`对应的WAV文件名`。方便调教时查看跨语种映射的实际情况。

---

## 五、 云端 TTS 接口规范

如果你自己搭建了中转服务器，或者想要接入其他云服务，App 内部定义了一套标准。

### Azure Speech API 接入点
- **Endpoint**: `https://{region}.tts.speech.microsoft.com/cognitiveservices/v1`
- **Method**: `POST`
- **Auth**: 请求头携带 `Ocp-Apim-Subscription-Key`
- **Response**: `riff-16khz-16bit-mono-pcm` (App 收到此 WAV 流后，会自动由 `WavProcessor` 重采样为 44.1kHz)

### Google Cloud TTS 接入点
- **Endpoint**: `https://texttospeech.googleapis.com/v1/text:synthesize?key={API_KEY}`
- **Method**: `POST` (请求体 JSON)
- **Response**: 返回 Base64 编码的 `LINEAR16` PCM 裸流（App 内部会自动拼接 WAV 文件头并重采样）。