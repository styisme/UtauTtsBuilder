using System.Globalization;

namespace UtauTtsBuilder.Services;

public enum UiLanguage
{
    Auto,
    Chinese,
    English,
    Japanese
}

public static class Loc
{
    private const string KeyLang = "ui_language";

    public static UiLanguage Mode
    {
        get
        {
            var s = Preferences.Default.Get(KeyLang, "auto");
            return s switch
            {
                "zh" => UiLanguage.Chinese,
                "en" => UiLanguage.English,
                "ja" => UiLanguage.Japanese,
                _ => UiLanguage.Auto
            };
        }
        set
        {
            var s = value switch
            {
                UiLanguage.Chinese => "zh",
                UiLanguage.English => "en",
                UiLanguage.Japanese => "ja",
                _ => "auto"
            };
            Preferences.Default.Set(KeyLang, s);
        }
    }

    /// <summary>当前生效语言：zh / en / ja</summary>
    public static string Active
    {
        get
        {
            if (Mode == UiLanguage.Auto)
            {
                var sys = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                if (sys == "zh") return "zh";
                if (sys == "ja") return "ja";
                return "en";
            }
            return Mode switch
            {
                UiLanguage.Chinese => "zh",
                UiLanguage.English => "en",
                UiLanguage.Japanese => "ja",
                _ => "en"
            };
        }
    }

    public static string T(string key)
    {
        if (!Strings.TryGetValue(key, out var v)) return key;
        return Active switch
        {
            "zh" => v.Zh,
            "ja" => v.Ja,
            _ => v.En
        };
    }

    public static string F(string key, params object[] args)
    {
        var t = T(key);
        try { return string.Format(t, args); }
        catch { return t; }
    }

    public static string GetLanguageName(UiLanguage m) => m switch
    {
        UiLanguage.Chinese => "中文",
        UiLanguage.English => "English",
        UiLanguage.Japanese => "日本語",
        _ => T("lang_auto")
    };

    // ============================================================
    // 字符串表
    // ============================================================
    private static readonly Dictionary<string, (string Zh, string En, string Ja)> Strings = new()
    {
        // ===== MainPage =====
        ["app_title"] = ("UTAU 声库生成器", "UTAU Voicebank Builder", "UTAU 音源ライブラリ生成"),
        ["app_subtitle"] = ("从系统 TTS 引擎一键生成", "Generate voicebanks from system TTS", "システムTTSから音源を生成"),

        ["sec_language"] = ("① 声库语言", "① Voicebank language", "① 音源の言語"),
        ["btn_import_reclist"] = ("导入自定义音素表", "Import custom reclist", "カスタム音素表を読み込む"),
        ["btn_clear"] = ("清除", "Clear", "クリア"),

        ["sec_engine"] = ("② TTS 引擎", "② TTS engine", "② TTS エンジン"),
        ["btn_refresh_engines"] = ("刷新引擎列表", "Refresh engine list", "エンジン一覧を更新"),

        ["sec_name"] = ("③ 声库名称", "③ Voicebank name", "③ 音源名"),
        ["ph_name"] = ("例如: 小艺", "e.g. Xiaoyi", "例：ずんだもん"),

        ["sec_icon"] = ("④ 图标（可选）", "④ Icon (optional)", "④ アイコン（任意）"),
        ["hint_icon"] = ("建议 100×100 的 PNG / BMP / JPG", "Recommended 100×100 PNG / BMP / JPG", "100×100 の PNG / BMP / JPG を推奨"),
        ["btn_pick_icon"] = ("选择图标", "Choose icon", "アイコンを選択"),

        ["sec_portrait"] = ("⑤ 立绘（可选）", "⑤ Portrait (optional)", "⑤ 立ち絵（任意）"),
        ["hint_portrait"] = ("仅 OpenUtau 会显示立绘", "Displayed only in OpenUtau", "OpenUtau のみ表示"),
        ["btn_pick_portrait"] = ("选择立绘", "Choose portrait", "立ち絵を選択"),

        ["sec_author"] = ("⑥ 作者（可选）", "⑥ Author (optional)", "⑥ 作者（任意）"),
        ["ph_author"] = ("默认: Auto-generated", "Default: Auto-generated", "既定：Auto-generated"),

        ["sec_output"] = ("⑦ 输出路径（可选）", "⑦ Output path (optional)", "⑦ 出力先（任意）"),
        ["hint_output"] = ("留空 = 内部存储/Download/UtauTtsBuilder/", "Empty = Internal/Download/UtauTtsBuilder/", "空欄 = 内部/Download/UtauTtsBuilder/"),
        ["ph_output"] = ("例如: Music/Utau", "e.g. Music/Utau", "例：Music/Utau"),

        ["zip_checkbox"] = ("生成后自动压缩为 ZIP", "Auto-compress to ZIP", "生成後に ZIP に圧縮"),
        ["btn_build"] = ("开始生成声库", "Build voicebank", "音源を生成"),

        ["not_selected"] = ("未选择", "Not selected", "未選択"),
        ["selected_fmt"] = ("已选择: {0}", "Selected: {0}", "選択済み：{0}"),

        ["engines_found_fmt"] = ("已发现 {0} 个 TTS 引擎", "Found {0} TTS engine(s)", "{0} 個の TTS エンジンを検出"),
        ["engines_load_failed_fmt"] = ("加载 TTS 引擎失败: {0}", "Failed to load TTS engines: {0}", "TTS エンジンの読み込み失敗：{0}"),

        // ===== 弹窗 =====
        ["title_tip"] = ("提示", "Notice", "お知らせ"),
        ["msg_select_engine_and_name"] = ("请选择引擎并输入声库名称", "Please choose an engine and enter a voicebank name", "エンジンを選択し、音源名を入力してください"),

        ["title_import_failed"] = ("导入失败", "Import failed", "読み込み失敗"),
        ["title_parse_failed"] = ("解析失败", "Parse failed", "解析失敗"),
        ["msg_parse_failed"] = (
            "音素表里没有解析到任何音节。\n\n格式要求（每行一条）：\n  别名=文件名\n  别名,文件名\n  别名 文件名\n\n以 # 开头的行为注释，可指定 TTS 语言：\n  # language: ja-JP",
            "No syllables parsed from the file.\n\nFormat (one entry per line):\n  alias=filename\n  alias,filename\n  alias filename\n\nLines starting with # are comments; you can set TTS language:\n  # language: ja-JP",
            "音素表から音節を解析できませんでした。\n\n形式（1行1項目）：\n  別名=ファイル名\n  別名,ファイル名\n  別名 ファイル名\n\n# で始まる行はコメント。TTS 言語も指定可能：\n  # language: ja-JP"
        ),
        ["title_choose_tts_lang"] = ("选择 TTS 发音语言", "Choose TTS language", "TTS の発音言語を選択"),
        ["lang_ja_jp"] = ("日语 (ja-JP)", "Japanese (ja-JP)", "日本語 (ja-JP)"),
        ["lang_zh_cn"] = ("中文 (zh-CN)", "Chinese (zh-CN)", "中国語 (zh-CN)"),
        ["lang_en_us"] = ("英语 (en-US)", "English (en-US)", "英語 (en-US)"),
        ["action_cancel"] = ("取消", "Cancel", "キャンセル"),

        ["title_import_success"] = ("导入成功", "Import success", "読み込み成功"),
        ["msg_import_success_fmt"] = ("音素表: {0}\n音节数: {1}\nTTS 语言: {2}", "Reclist: {0}\nSyllables: {1}\nTTS language: {2}", "音素表：{0}\n音節数：{1}\nTTS 言語：{2}"),

        ["title_clear_reclist"] = ("清除自定义音素表", "Clear custom reclist", "カスタム音素表をクリア"),
        ["msg_clear_reclist"] = ("确定要清除吗？", "Are you sure?", "本当にクリアしますか？"),
        ["btn_clear_action"] = ("清除", "Clear", "クリア"),

        ["title_cannot_create_dir"] = ("无法创建目录", "Cannot create directory", "ディレクトリを作成できません"),
        ["title_error"] = ("错误", "Error", "エラー"),
        ["title_done"] = ("完成", "Done", "完了"),
        ["msg_done_fmt"] = ("声库已生成到:\n{0}\n\nZIP: {1}", "Voicebank generated at:\n{0}\n\nZIP: {1}", "音源を生成しました:\n{0}\n\nZIP: {1}"),
        ["btn_share_zip"] = ("分享 ZIP", "Share ZIP", "ZIP を共有"),
        ["btn_close"] = ("关闭", "Close", "閉じる"),
        ["btn_ok"] = ("好", "OK", "OK"),
        ["share_zip_title"] = ("分享声库 ZIP", "Share voicebank ZIP", "音源 ZIP を共有"),

        ["title_pick_image_failed"] = ("选择图片失败", "Failed to pick image", "画像の選択に失敗"),

        // ===== 进度消息 =====
        ["prog_init_engine_fmt"] = ("正在初始化 TTS 引擎（{0}）...", "Initializing TTS engine ({0})...", "TTS エンジンを初期化中（{0}）..."),
        ["prog_synth_fmt"] = ("[{0}/{1}] 合成: {2}", "[{0}/{1}] Synthesizing: {2}", "[{0}/{1}] 合成中：{2}"),
        ["prog_no_output_fmt"] = ("[{0}/{1}] {2}: 无输出，跳过", "[{0}/{1}] {2}: no output, skipped", "[{0}/{1}] {2}：出力なし、スキップ"),
        ["prog_failed_fmt"] = ("[{0}/{1}] {2} 失败: {3}", "[{0}/{1}] {2} failed: {3}", "[{0}/{1}] {2} 失敗：{3}"),
        ["prog_write_oto"] = ("正在写入 oto.ini...", "Writing oto.ini...", "oto.ini を書き込み中..."),
        ["prog_copy_icon_fmt"] = ("已复制图标: {0}", "Icon copied: {0}", "アイコンをコピー：{0}"),
        ["prog_copy_portrait_fmt"] = ("已复制立绘: {0}", "Portrait copied: {0}", "立ち絵をコピー：{0}"),
        ["prog_icon_copy_failed_fmt"] = ("图标复制失败: {0}", "Icon copy failed: {0}", "アイコンのコピー失敗：{0}"),
        ["prog_portrait_copy_failed_fmt"] = ("立绘复制失败: {0}", "Portrait copy failed: {0}", "立ち絵のコピー失敗：{0}"),
        ["prog_write_chartxt"] = ("正在写入 character.txt（UTAU）...", "Writing character.txt (UTAU)...", "character.txt（UTAU）を書き込み中..."),
        ["prog_write_charyaml"] = ("正在写入 character.yaml（OpenUtau）...", "Writing character.yaml (OpenUtau)...", "character.yaml（OpenUtau）を書き込み中..."),
        ["prog_compressing"] = ("正在压缩 ZIP...", "Compressing ZIP...", "ZIP を圧縮中..."),
        ["prog_compress_failed_fmt"] = ("压缩失败: {0}", "Compress failed: {0}", "圧縮失敗：{0}"),
        ["prog_done_fmt"] = ("完成！共 {0} 个音节，共享音频 {1} 个，失败 {2} 个",
                             "Done! {0} syllables, {1} shared, {2} failed",
                             "完了！{0} 音節、共有 {1}、失敗 {2}"),

        // ===== SettingsPage =====
        ["settings_title"] = ("设置", "Settings", "設定"),
        ["settings_appearance"] = ("外观", "Appearance", "外観"),
        ["settings_theme_auto"] = ("跟随系统", "Follow system", "システムに従う"),
        ["settings_theme_light"] = ("浅色", "Light", "ライト"),
        ["settings_theme_dark"] = ("深色", "Dark", "ダーク"),
        ["settings_dynamic"] = ("动态取色", "Dynamic color", "ダイナミックカラー"),
        ["settings_use_wallpaper"] = ("使用壁纸颜色", "Use wallpaper color", "壁紙の色を使用"),
        ["settings_dynamic_hint"] = ("仅 Android 12+ 有效", "Android 12+ only", "Android 12+ のみ"),
        ["settings_dynamic_footer"] = ("关闭动态取色后，应用会使用 MD3 基线紫作为主题色。",
                                       "When dynamic color is off, the app uses the MD3 baseline purple.",
                                       "ダイナミックカラーをオフにすると、MD3 のベース紫を使用します。"),
        ["settings_language"] = ("界面语言", "Language", "表示言語"),
        ["lang_auto"] = ("自动", "Auto", "自動"),

        // ===== Reclist 显示名 =====
        ["reclist_ja_cv"] = ("日语 CV (69 音节)", "Japanese CV (69)", "日本語 CV（69 音節）"),
        ["reclist_ja_cvvc"] = ("日语 CVVC (133 音节)", "Japanese CVVC (133)", "日本語 CVVC（133 音節）"),
        ["reclist_ja_vcv"] = ("日语 VCV (105 音节)", "Japanese VCV (105)", "日本語 VCV（105 音節）"),
        ["reclist_zh_cv"] = ("中文 CV (78 音节)", "Chinese CV (78)", "中国語 CV（78 音節）"),
        ["reclist_en_cv"] = ("英语 CV (93 音节)", "English CV (93)", "英語 CV（93 音節）"),
        ["reclist_custom_fmt"] = ("自定义: {0}", "Custom: {0}", "カスタム：{0}"),
        ["custom_default_name"] = ("自定义", "Custom", "カスタム"),

        // ===== Settings 追加 =====
        ["settings_ui_style"] = ("UI 风格", "UI style", "UI スタイル"),
        ["settings_about"] = ("关于", "About", "このアプリについて"),
        ["about_title"] = ("关于", "About", "このアプリについて"),
        ["about_github"] = ("GitHub 仓库", "GitHub repository", "GitHub リポジトリ"),
        ["about_license"] = ("查看 MIT 协议", "View MIT license", "MIT ライセンスを見る"),
        ["about_license_title"] = ("MIT License", "MIT License", "MIT License"),
        ["about_license_body"] = (
            "本项目使用 MIT 协议发布，允许自由使用、修改和再分发，但需保留版权声明。",
            "Released under the MIT License. Free to use, modify, and redistribute with attribution.",
            "MIT ライセンスのもとで公開されています。著作権表示を保持すれば自由に利用・改変・再配布できます。"
        ),
        ["about_desc"] = (
            "UTAU 声库生成器 — 从 Android 系统 TTS 引擎一键生成 UTAU 声库。\n\n支持日语 CV / CVVC / VCV、中文 CV、英语 CV，以及自定义音素表导入。",
            "UTAU Voicebank Builder — Generate UTAU voicebanks from Android system TTS engines.\n\nSupports Japanese CV / CVVC / VCV, Chinese CV, English CV, and custom reclist import.",
            "UTAU 音源ライブラリ生成 — Android のシステム TTS から UTAU 音源を生成します。\n\n日本語 CV / CVVC / VCV、中国語 CV、英語 CV、カスタム音素表の読み込みに対応。"
        ),
    };
}
