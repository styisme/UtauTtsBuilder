# Contributing Guide

Thanks for contributing to **UtauTtsBuilder**! This project follows a lightweight but explicit source management convention:
**Conventional Commits + SemVer 2.0.0 + CHANGELOG + CI auto-validation**.

---

## ⚠️ About AI-Generated Code

Most of this project's code is **AI-assisted**, see [docs/AI_DISCLOSURE.md](docs/AI_DISCLOSURE.md). When submitting a PR, please:

- State whether the change was AI-generated
- Manually verify the functionality (at least once on a real device)
- If you also used AI, mention it in the PR description

---

## Local Development

### Requirements

| Component | Version |
|---|---|
| .NET SDK | 10.0 or later |
| .NET MAUI workload | `dotnet workload install maui-android` |
| JDK | 17 |
| Android SDK | API 21+ |

### First Clone

```bash
git clone https://github.com/styisme/UtauTtsBuilder.git
cd UtauTtsBuilder
dotnet workload install maui-android
dotnet restore
```

### Build

```bash
# Debug (recommended during development)
dotnet build -f net10.0-android -c Debug

# Release
dotnet build -f net10.0-android -c Release
```

### Run on Device

```bash
dotnet build -t:Run -f net10.0-android
```

Or install the APK directly:

```bash
adb install bin/Debug/net10.0-android/com.example.utauttsbuilder-Signed.apk
```

### Cloud Build

The project is configured with GitHub Actions:
- Auto-build on push to `main` or `master`
- Manual trigger: Actions page → **Run workflow**
- Download the APK from the **Artifacts** section

**No local dev environment?** Edit files directly on GitHub and let Actions build the APK for you.

---

## Project Structure

```
UtauTtsBuilder/
├── UtauTtsBuilder.csproj        # Project file
├── MauiProgram.cs               # MAUI startup + Handler registration
├── App.xaml / App.xaml.cs       # Global resources + lifecycle
│
├── MainPage.xaml / .cs          # Home page (voicebank generation)
├── SettingsPage.xaml / .cs      # Settings
├── AboutPage.xaml / .cs         # About
├── CloudSettingsPage.xaml / .cs # Cloud TTS configuration
├── SelectionPage.xaml / .cs     # Custom list picker (replaces Picker)
├── ActionSheetPage.xaml / .cs   # Custom action menu (replaces ActionSheet)
│
├── Platforms/Android/
│   ├── AndroidManifest.xml
│   ├── MainActivity.cs
│   ├── MainApplication.cs
│   ├── TtsEngineHelper.cs       # Enumerate TTS engines on device
│   └── TtsSynthesizer.cs        # Wrap local TTS synthesis to WAV
│
├── Services/
│   ├── JapaneseCvReclist.cs     # Japanese CV reclist (129 syllables)
│   ├── ReclistProvider.cs       # Multi-language + CVVC/VCV + custom reclist
│   ├── WavProcessor.cs          # Pure C# WAV parsing / resampling / silence trim
│   ├── OtoGenerator.cs          # oto.ini / character.txt / character.yaml
│   ├── VoicebankBuilder.cs      # Main flow orchestration (local / cloud)
│   ├── CloudConfigService.cs    # Cloud API key storage
│   ├── CloudTtsEngine.cs        # Azure / Google Cloud TTS
│   ├── UstTemplateGenerator.cs  # Generate test.ust
│   ├── ThemeService.cs          # Theme + UI style entry point
│   ├── ThemeHelper.cs           # Dynamic color (Android 12+)
│   ├── UiStyleService.cs        # Four UI style palettes
│   └── Localization.cs          # Tri-language strings + Loc.T()
│
└── .github/workflows/
    └── build-android.yml        # CI build script
```

**Layering principles**:

- **UI layer** (`*Page.xaml`): Only UI + event bindings
- **Platform layer** (`Platforms/Android/`): All Android API calls live here
- **Service layer** (`Services/`): Pure C# business logic, platform-agnostic

---

## Code Style

### C# Conventions

- Use **4-space indentation** (no tabs)
- Private fields prefixed with `_` (e.g. `_enginePackage`)
- Async methods end with `Async`
- Prefer `string?` over unannotated `string`
- .NET 10 has `ImplicitUsings` enabled — don't write redundant `using`

### Namespace Conflicts (Important)

Because `Platforms/Android/` uses the namespace `UtauTtsBuilder.Platforms.Android`, **the compiler treats it as a root**. All Android API calls must use the `global::` prefix:

```csharp
// ✅ Recommended
using AndroidTts = global::Android.Speech.Tts.TextToSpeech;

// ✅ Also fine
var locale = global::Java.Util.Locale.Japanese;

// ❌ Avoid (parsed as UtauTtsBuilder.Platforms.Android.Speech)
using Android.Speech.Tts;
```

### XAML Conventions

- `x:Name` in **PascalCase**, matching backend fields
- Event handlers: `OnXxxClicked` / `OnXxxChanged`
- **Colors must use `DynamicResource`** (not `StaticResource`, or theme switches break)
- Styles live in `App.xaml` `<Application.Resources>`
- Cards use `Border` + `StrokeShape="RoundRectangle 12"`, not the deprecated `Frame`
- **Do not use system `Picker` / `DisplayActionSheet`** — use `SelectionPage` / `ActionSheetPage` instead

### Localization Conventions

- **All user-visible text must go through `Loc.T()`**, no hardcoded Chinese/English
- When adding a key, sync all three languages in `Localization.cs` (Chinese / English / Japanese)
- For parameterized text, use `Loc.F("key_fmt", arg1, arg2)`

```csharp
// ✅
StatusLabel.Text = Loc.F("engines_found_fmt", count);

// ❌
StatusLabel.Text = $"Found {count} TTS engines";
```

### Theme Conventions

- Color keys prefixed with `Md3*` (e.g. `Md3Primary` / `Md3OnSurface`)
- Size keys prefixed with `Ui*` (e.g. `UiBtnCorner` / `UiBtnHeight`)
- Theme switching must go through `ThemeService.Apply()`, never modify `Resources` directly

---

## Commit Convention

Follow [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>(<scope>): <subject>
```

### Allowed types

| type | Description |
|---|---|
| `feat` | New feature |
| `fix` | Bug fix |
| `docs` | Documentation only |
| `style` | Formatting (spaces, semicolons, etc.) |
| `refactor` | Refactoring |
| `perf` | Performance improvement |
| `test` | Tests |
| `build` | Build system or dependency changes |
| `ci` | CI configuration |
| `chore` | Misc (version bumps, .gitignore) |
| `revert` | Revert |

### Common scopes

| scope | Description |
|---|---|
| `tts` | TTS synthesis |
| `oto` | oto.ini generation |
| `wav` | Audio processing |
| `reclist` | Syllable lists |
| `ui` | UI |
| `i18n` | Localization |
| `theme` | Themes / UI styles |
| `cloud` | Cloud TTS |
| `builder` | Voicebank build flow |
| `ci` | CI configuration |

### Examples

```
feat(cloud): add Azure Speech support
fix(oto): correct cutoff sign for mobile UTAU
feat(reclist): expand Japanese CV to 129 syllables
docs(changelog): add v1.1.1 section
style(ui): migrate to Material Design 3
```

---

## Branch Model

| Branch | Purpose |
|---|---|
| `master` | Stable branch (current default) |
| `feat/xxx` | Feature branch |
| `fix/xxx` | Fix branch |
| `docs/xxx` | Docs branch |

---

## Pull Request Flow

1. Fork the repo (or edit directly)
2. Create a branch: `git checkout -b feat/your-feature`
3. Make changes, test locally / via Actions
4. Commit using Conventional Commits
5. Push and open a PR
6. Wait for CI + review

### PR description should contain

```markdown
## What
Brief summary of what this PR does

## Type
- [ ] feat / fix / refactor / docs / chore

## Related Issue
Closes #123

## Testing
- [ ] Local build passes
- [ ] Tested on real device
- [ ] Actions build passes

## AI-generated?
- [ ] Yes (specify model and manual verification)
- [ ] No
```

---

## Issues

### Bug Reports

Please include:

- **Device model** + **Android version**
- **TTS engine name & version** (e.g. iFlytek 3.0)
- **Steps to reproduce**
- **Expected** vs **actual** behavior
- Relevant logs (`_log.txt`), screenshots, or `oto.ini` fragments

### Feature Requests

- Describe the use case
- If possible, provide reference projects or implementation ideas

---

## Versioning

Follow [Semantic Versioning 2.0.0](https://semver.org/):

| Change | Version bump |
|---|---|
| Breaking change | **MAJOR** |
| Backward-compatible feature | **MINOR** |
| Bug fix | **PATCH** |

### ⚙️ Automatic Version Injection (Important)

**You don't need to edit `.csproj`!** GitHub Actions will:

1. Extract the latest `## [x.y.z]` from `CHANGELOG.md` as `ApplicationDisplayVersion`
2. Use `YYYYMMDD` as `ApplicationVersion` (Android internal integer)
3. Auto-rename the APK to `UtauTtsBuilder-v{x.y.z}.apk`

So you **only need to maintain `CHANGELOG.md`**, version numbers follow automatically.

---

## CHANGELOG Maintenance

All user-facing changes must be recorded under `[Unreleased]` in `CHANGELOG.md`.

**Categories**:

- `Added` — New features
- `Changed` — Behavior changes
- `Deprecated` — Deprecated features
- `Removed` — Removed features
- `Fixed` — Bug fixes
- `Security` — Security-related

**When releasing**:

1. Change `[Unreleased]` to `[x.y.z] - YYYY-MM-DD`
2. Add a new empty `[Unreleased]` section below
3. Update the compare links at the bottom

---

## Release Process

Maintainer steps to release a new version:

1. Ensure all code is pushed to `master`
2. Update `CHANGELOG.md`: freeze `[Unreleased]` into a new version number
3. Commit + Push
4. Wait for GitHub Actions to succeed (green check)
5. Download the APK from **Artifacts**
6. Go to **Releases** → **Draft a new release**
   - Tag: `v{x.y.z}` → click **Create new tag on publish**
   - Title: `v{x.y.z}`
   - Description: paste the corresponding CHANGELOG section + install notes
   - Upload the APK
   - ✅ Check **Set as the latest release**
7. **Publish release**

---

## FAQ

### Q: Build fails with `Xamarin.Android 找不到 Java`

Check that JDK 17 is installed and `JAVA_HOME` points to JDK 17.

### Q: Actions fails with `NETSDK1045`

`.NET 10` may still be in preview. Make sure `dotnet-version` in the workflow matches `TargetFramework` in `.csproj`.

### Q: "This TTS engine doesn't support Japanese" when generating

The installed TTS engine has no Japanese voice pack. Try:
- Install Google TTS and download the Japanese pack
- Use a different engine that supports Japanese

### Q: Voicebank shows garbled text in UTAU

`oto.ini` and `character.txt` must use **Shift-JIS** encoding. This project handles it — just be careful when editing manually.

### Q: Mobile UTAU won't play any sound

- Check that `oto.ini` filenames are **kana** (e.g. `あ.wav`, not `a.wav`)
- Check that `cutoff` is a **positive number**
- Check that `character.txt` is in the voicebank root
- The note lyrics must exactly match the aliases (`ちゃ`, not `cha`)

### Q: How to add a new language reclist

1. Add an entry to `VoicebankLanguage` enum in `ReclistProvider.cs`
2. Add a `BuildXxx()` or `ConvertCvDict()` branch
3. Update `GetReclist` / `GetLanguageCode` / `GetDisplayName`
4. Add tri-language display names in `Localization.cs`
5. Add it to `InitLanguageList()` in `MainPage.xaml.cs`

---

## Contact

- **Issues**: Report bugs, suggest features
- **Pull Requests**: Submit code
- **@styisme**: Anything else

---

Thanks again for contributing! 🎉