**🌐 言語 / Language / 语言:** [中文](README.md) | [English](README.en.md) | 日本語

---

# UTAU TTS Voicebank Builder

> Android のシステム TTS エンジンから UTAU 音源ライブラリをワンタップで生成する .NET MAUI アプリ

[![Build Android APK](https://github.com/styisme/UtauTtsBuilder/actions/workflows/build-android.yml/badge.svg)](https://github.com/styisme/UtauTtsBuilder/actions/workflows/build-android.yml)
[![Version](https://img.shields.io/badge/version-1.2.0-512BD4)](CHANGELOG.md)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![Made with AI](https://img.shields.io/badge/made%20with-AI-FF6B6B)](docs/AI_DISCLOSURE.md)
[![Platform](https://img.shields.io/badge/platform-Android%205.0%2B-brightgreen)]()

---

## ⚠️ AI 生成に関する声明

> **本プロジェクトは AI の支援を受けて生成されています。**
>
> コードの大部分（アーキテクチャ、ビジネスロジック、UI レイアウト、ドキュメント）は **大規模言語モデル（LLM）** によって開発者の指示のもとで生成され、その後、人間によるレビュー・テスト・修正が行われています。要件定義・機能検収・バグ特定・リリース判断は開発者が担当します。
>
> **ご注意ください**：
> - 予期しないエッジケースやバグが含まれている可能性があります
> - 本番環境での安定性・安全性は保証されません
> - 重要な用途での使用は自己責任でお願いします
> - Issue は歓迎しますが、**対応時期は保証できません**
>
> 詳細は [docs/AI_DISCLOSURE.md](docs/AI_DISCLOSURE.md) をご覧ください。

---

## 📦 最新バージョン

現在のバージョン：**v1.2.0**

完全な履歴は [CHANGELOG.md](CHANGELOG.md) をご覧ください。

---

## ✨ 機能

### 音源生成と高度な合成
- 🗣️ **複数の TTS エンジン** — デバイスにインストール済みのローカル TTS エンジンを自動列挙
- ☁️ **クラウド TTS** — Azure Speech と Google Cloud TTS（API キーは各自で用意）
- 🌏 **多言語音源** — 日本語、中国語、英語
  - 日本語：CV (129) / CVVC (194) / VCV (105)
  - 中国語：CV (78) / CVVC (203) / VCV (130)
  - 英語：CV (93)
- 🌍 **言語間変換（クロスリンガル）** — 中→日、日→中、英→日など 7 つのモードに対応。スマートマッチ機能付き。ローマ字/ピンインを介することで、対象言語をネイティブサポートしていない TTS エンジンでも音源を生成可能。
- 🛡️ **強制生成（Force Build）** — 対象の TTS エンジンが選択した言語をサポートしていない場合、システムのデフォルト言語に自動フォールバックして生成を中断させません。
- ⚡ **マルチエンジン一括生成** — 選択した複数の TTS エンジンごとに、一度に音源を生成します。
- ⏱️ **生成時間の予測** — 生成開始前に、推定所要時間と総音節数をポップアップで表示します。
- 📥 **カスタム音素表インポート** — 任意の `.txt` 音素表を読み込み可能。読み込み前にプレビューで確認できます。

### 生産性と診断ツール
- 📂 **音源管理リスト** — 生成済みの音源をスキャンして一覧表示。ZIP共有、`oto.ini` 編集、UST生成、削除などをワンタップで実行可能。
- ⚖️ **エンジン比較** — 利用可能な全エンジンで同じ音節を素早く合成し、A/B 試聴ができます。
- 📊 **グローバル実行ログ** — 例外、メニュー操作、ネットワークリクエストの結果を記録。アプリ内でリアルタイム表示、クリア、共有が可能。
- 📝 **ビルド追跡** — 各音源に `_log.txt`（音節ごとの所要時間統計）と `_reference.txt`（発音参照表）を自動生成。
- 🖼️ **波形とプレビュー画像** — 単一音節の波形を表示し、生成完了後に音源のプレビュー画像（ダークモード対応）を自動生成。

### 音声処理
- 🔊 **純粋な C# 音声処理** — サードパーティライブラリ不使用
- ✂️ **前後の無音自動トリミング** — 閾値 0.005f、両端 8ms のパディング
- 🎚️ **話速・ピッチ調整** — 0.5–1.5× / 0.5–2.0×
- 🔄 **レジューム対応** — 既存の音声ファイルをスキップ

### UI / UX
- 🎨 **4 種類の UI スタイル** — Material 3 / HarmonyOS / Windows UWP / Android 4 Holo
- 🌗 **ダークモード** — システムに従う / ライト / ダーク
- 🎨 **ダイナミックカラー** — Android 12+ の壁紙ベース
- 🌍 **三言語 UI** — 中文 / English / 日本語
- 🔀 **完全カスタムコンポーネント** — システムの Picker / ActionSheet / Alert を廃止。すべてのダイアログが現在の UI テーマに追従します。
- ⚙️ **高度なメニュー** — 右上の `⋯` メニューから、音源管理、一括生成、比較、ログなどを一元管理できます。

---

## 📱 使い方

### 基本フロー
1. [Releases](../../releases/latest) から最新の APK をダウンロードしてインストール
2. 初回起動時に **ファイル、メディア、ネットワーク** へのアクセスを許可
3. 以下を選択：
   - **音源の言語**（日本語 / 中国語 / 英語；CV / CVVC / VCV）
   - **言語間変換モード**（デフォルトはネイティブ。非対応エンジンでもローマ字マッピングで生成可能）
   - **TTS エンジン**（選択すると音源名が自動入力）
   - **話速・ピッチ**（デフォルト推奨）
   - **音源名**
   - アイコン / 立ち絵 / 作者（すべて任意）
4. **「音源を生成」** をタップし、予想時間を確認
5. 完了後、操作メニューが表示されます：
   - **波形を見る / 試聴** → 詳細ページを開く
   - **oto.ini を編集** → システム共有でテキストエディタへ
   - **UST テンプレートを生成** → `test.ust` を書き出し
   - **ZIP を共有** → 共有シートから任意のアプリへ

### 高度な機能（右上 `⋯` メニュー）
- **生成済みの音源**：履歴の管理
- **マルチエンジン一括生成**：選択した全エンジンを一度に実行
- **エンジン比較**：各エンジンの音色を素早く試聴
- **更新を確認**：手動で GitHub Releases を確認
- **実行ログ**：トラブルシューティングに必須
- **キャッシュをクリア**：一時領域を解放

### クラウド TTS（任意）
1. 設定 → **クラウド TTS 設定**
2. Azure Speech または Google Cloud の API キーを入力
3. ホームに戻ると、エンジン一覧に `☁ Azure (...)` または `☁ Google Cloud (...)` が追加されます

**Azure の設定**：
- [Azure Portal](https://portal.azure.com/) で Speech サービスを作成
- **Key** と **Region**（例：`eastasia`）をコピー
- 音声名（例：`ja-JP-NanamiNeural`）は空欄でも可

**Google の設定**：
- [Google Cloud Console](https://console.cloud.google.com/) で **Text-to-Speech API** を有効化
- API キーを作成して入力

> ⚠️ クラウド TTS はネットワークが必要です。地域によっては Google にアクセスできない場合があります。

### 出力先
```
内部ストレージ/Download/UtauTtsBuilder/{音源名}/
```

---

## 📂 出力構造

```
{音源名}/
├── あ.wav             # 音節音声（44.1kHz / 16-bit / Mono）
├── か.wav
├── ちゃ.wav           # 拗音
├── a_k.wav            # CVVC 遷移音
├── ...
├── oto.ini            # UTAU パラメータ（Shift-JIS）
├── character.txt      # UTAU 記述（Shift-JIS）
├── character.yaml     # OpenUtau 記述（UTF-8、立ち絵含む）
├── preview.png        # 音源プレビュー画像（自動生成）
├── icon.png           # アイコン（任意）
├── portrait.png       # 立ち絵（任意）
├── test.ust           # UST テンプレート（任意）
├── _log.txt           # 生成ログ（所要時間統計）
└── _reference.txt     # 発音参照表（別名と TTS テキストのマッピング）
```

---

## 🛠️ ソースからビルド

### 必要条件
| コンポーネント | バージョン |
|---|---|
| .NET SDK | 10.0 以上 |
| .NET MAUI workload | `dotnet workload install maui-android` |
| JDK | 17 |
| Android SDK | API 21+ |

### ローカルビルド
```bash
git clone https://github.com/styisme/UtauTtsBuilder.git
cd UtauTtsBuilder
dotnet restore
dotnet build -f net10.0-android -c Release
```

### GitHub Actions
プロジェクトには `.github/workflows/build-android.yml` が設定されています：
- `main` への push で自動ビルド
- 手動トリガー：Actions ページ → **Run workflow**
- APK は **Artifacts** からダウンロード

**バージョン自動注入**：Actions が `CHANGELOG.md` から最新の `## [x.y.z]` を抽出し、アプリバージョンとして注入します。

> 💡 **ローカル開発環境がない場合**：リポジトリを Fork して GitHub 上で編集すれば、Actions が APK をビルドします。

---

## ⚠️ 既知の制限

- **TTS の品質がすべて** — システム TTS は朗読用であり、歌唱には不向き。繋ぎ目が目立ちます
- **CVVC / VCV は手動調整が必要** — [vLabeler](https://github.com/sdercolin/vlabeler) または [setParam](https://github.com/LiCybora/setParam) を強く推奨
- **言語間変換にはアクセントが残る場合があります** — ローマ字/ピンインを介した発音は、元の言語のイントネーションが残ることがあります
- **クラウド TTS はネットワーク必須** — 地域によっては Google がブロックされています
- **アイコンは BMP 推奨** — オリジナル UTAU は PNG サポートが限定的。OpenUtau は問題なし
- **AI 生成コードには未知のバグが含まれる可能性** — 上記の AI 声明をご覧ください

---

## 🤝 コントリビュート

Issue と PR を歓迎します！**まず [CONTRIBUTING.md](CONTRIBUTING.md) をお読みください**：
- ローカル開発環境の構築
- コードスタイルと Conventional Commits
- ブランチモデルと PR フロー
- リリースプロセス

---

## 📜 ライセンス

[MIT License](LICENSE)。

**特記事項**：
- 生成された音源の音声著作権は TTS プロバイダに帰属します
- 各 TTS プロバイダの利用規約に従ってください
- **商用利用や権利侵害を目的とした使用は禁止**です
- **実在の人物の声を模倣しないでください**

---

## 🙏 クレジット

- [.NET MAUI](https://github.com/dotnet/maui) — クロスプラットフォームフレームワーク
- [UTAU](http://utau2008.xrea.jp/) — クラシックな歌声合成ソフト
- [OpenUtau](https://github.com/stakira/OpenUtau) — モダンな UTAU 代替
- [Keep a Changelog](https://keepachangelog.com/) — チェンジログ規約
- [Conventional Commits](https://www.conventionalcommits.org/) — コミット規約
- すべてのコントリビュータ

---

## 📚 ドキュメント

| ドキュメント | 説明 |
|---|---|
| [CHANGELOG.md](CHANGELOG.md) | バージョン履歴 |
| [CONTRIBUTING.md](CONTRIBUTING.md) | コントリビュートガイド |
| [docs/AI_DISCLOSURE.md](docs/AI_DISCLOSURE.md) | AI 生成の詳細 |
| [LICENSE](LICENSE) | MIT ライセンス |

---

**⚠️ 免責事項**：本プロジェクトは実験的なツールであり、コードの大部分は AI によって生成されています。使用は自己責任でお願いします。生成された音源の音声はデバイスのシステム TTS またはサードパーティのクラウドサービスに由来します。各サービスの利用規約に従ってください。