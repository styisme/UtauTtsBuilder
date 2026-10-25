using UtauTtsBuilder.Services;
using UtauTtsBuilder.Platforms.Android;

namespace UtauTtsBuilder;

public partial class TtsToDSPage : ContentPage
{
    private const string CorpusZh = 
        "我冒了严寒，回到相隔二千余里，别了二十余年的故乡去。时候既然是深冬；渐近故乡时，天气又阴晦了，冷风吹进船舱中，呜呜的响，从蓬隙向外一望，苍黄的天底下，远近横着几个萧索的荒村，没有一些活气。我的心禁不住悲凉起来了。阿！这不是我二十年来时时记得的故乡？我所记得的故乡全不如此。我的故乡好得多了。但要我记起他的美丽，说出他的佳处来，却又没有影像，没有言辞了。仿佛也就如此。" +
        "这时候，我的脑里忽然闪出一幅神异的图画来：深蓝的天空中挂着一轮金黄的圆月，下面是海边的沙地，都种着一望无际的碧绿的西瓜，其间有一个十一二岁的少年，项带银圈，手捏一柄钢叉，向一匹猹尽力的刺去，那猹却将身一扭，反从他的胯下逃走了。这少年便是闰土。" +
        "中华人民共和国，简称中国，位于亚洲大陆的东部、太平洋西岸。陆地面积约960万平方公里。中国是一个统一的多民族国家，迄今为止，通过识别并由中央政府确认的民族有56个。中国是世界上人口最多的发展中国家。" +
        "萌娘百科对OpenUTAU的介绍：OpenUTAU是由杉田朗主导、由UTAU社区为UTAU社区打造的免费开源歌声合成编辑器，以实现UTAU框架与引擎的现代化而开发。" +
        "关于DiffSinger：DiffSinger是一个基于扩散概率模型的声学模型，用于歌声合成（SVS）。它可以将噪声迭代地转换为梅尔频谱图。";

    private const string CorpusJa = 
        "重音テト（かさねてと、Kasane Teto）は、2008年4月1日のエイプリルフール前夕に、匿名掲示板2chで「存在しないボーカロイド」として企画されたバーチャルシンガーです。初音ミクなどの影響を受けて誕生し、後にUTAUの音源が提供され、本物のバーチャルシンガーとして活動を始めました。" +
        "足立レイ（あだち れい、Adachi Rei）は、missile 39（みさいる）によって制作されたオリジナルキャラクターです。正弦波の合成によって作られた声源を持ち、UTAU音源ライブラリやA.I.VOICE音声ライブラリを保有しています。" +
        "日本国は、アジア大陸の東、太平洋の西岸に位置する島国です。北海道、本州、四国、九州の4つの大きな島と、6800以上の小さな島々からなります。面積は約37.8万平方キロメートル。首都は東京です。";

    private const string CorpusEn = 
        "The United States of America, commonly known as the United States or America, is a country primarily located in North America. It consists of 50 states, a federal district, five major unincorporated territories, nine Minor Outlying Islands, and 326 Indian reservations." +
        "OpenUTAU is a free, open-source editor made for the UTAU community. It is developed by a group of volunteers and is designed to modernize the UTAU framework and engine. It features multi-track support, accompaniment, and automatic pre-rendering." +
        "DiffSinger is an acoustic model for Singing Voice Synthesis (SVS) based on the diffusion probabilistic model. It is a parameterized Markov chain that iteratively converts noise into a mel-spectrogram conditioned on the music score.";

    public TtsToDSPage()
    {
        InitializeComponent();
        ApplyLocalization();
        UpdateCharCounts();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ApplyLocalization();
    }

    private void ApplyLocalization()
    {
        TitleLabel.Text = Loc.T("ds_corpus_title");
        CorpusSectionLabel.Text = Loc.T("ds_corpus_section_builtin");
        CorpusHintLabel.Text = Loc.T("ds_corpus_hint");
        
        LblZh.Text = Loc.T("ds_corpus_zh");
        LblJa.Text = Loc.T("ds_corpus_ja");
        LblEn.Text = Loc.T("ds_corpus_en");

        CustomSectionLabel.Text = Loc.T("ds_corpus_section_custom");
        TextEditor.Placeholder = Loc.T("ds_corpus_custom_ph");
        AugLabel.Text = Loc.T("ds_corpus_aug");
        StartBtn.Text = Loc.T("ds_corpus_btn_start");
    }

    private void UpdateCharCounts()
    {
        LblZhCount.Text = $"({CorpusZh.Length})";
        LblJaCount.Text = $"({CorpusJa.Length})";
        LblEnCount.Text = $"({CorpusEn.Length})";
    }

    private async void OnStartClicked(object sender, EventArgs e)
    {
        var requests = new List<DsCorpusRequest>();

        if (ChkZh.IsChecked) requests.Add(new DsCorpusRequest { Text = CorpusZh, LanguageCode = "zh-CN" });
        if (ChkJa.IsChecked) requests.Add(new DsCorpusRequest { Text = CorpusJa, LanguageCode = "ja-JP" });
        if (ChkEn.IsChecked) requests.Add(new DsCorpusRequest { Text = CorpusEn, LanguageCode = "en-US" });

        var customText = TextEditor.Text?.Trim();
        if (!string.IsNullOrEmpty(customText))
        {
            var sysLang = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            string code = sysLang == "zh" ? "zh-CN" : sysLang == "ja" ? "ja-JP" : "en-US";
            requests.Add(new DsCorpusRequest { Text = customText, LanguageCode = code });
        }

        if (requests.Count == 0)
        {
            await DialogService.ShowAlertAsync(this, Loc.T("title_tip"), Loc.T("ds_corpus_no_text"), Loc.T("btn_ok"));
            return;
        }

        var engines = TtsEngineHelper.GetAvailableEngines();
        if (engines.Count == 0)
        {
            await DialogService.ShowAlertAsync(this, Loc.T("title_error"), Loc.T("engines_found_fmt"), Loc.T("btn_ok"));
            return;
        }
        var engine = engines.FirstOrDefault(en => en.IsDefault) ?? engines.First();

        StartBtn.IsEnabled = false;
        ProgressBar.IsVisible = true;
        ProgressBar.Progress = 0;

        var progress = new Progress<string>(msg => StatusLabel.Text = msg);
        var zipPath = Path.Combine(FileSystem.CacheDirectory, "DiffSinger_Corpus.zip");

        try
        {
            await TtsToDiffSingerService.GenerateMixedCorpusAsync(
                requests, engine.Name, 0.85f, 1.0f, ChkAug.IsChecked, zipPath, progress);

            StatusLabel.Text = Loc.T("ds_corpus_done");
            ProgressBar.Progress = 1;

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = Loc.T("ds_corpus_title"),
                File = new ShareFile(zipPath)
            });
        }
        catch (Exception ex)
        {
            await DialogService.ShowAlertAsync(this, Loc.T("title_error"), ex.Message, Loc.T("btn_ok"));
            StatusLabel.Text = Loc.T("ds_corpus_failed");
        }
        finally
        {
            StartBtn.IsEnabled = true;
            ProgressBar.IsVisible = false;
        }
    }

    private async void OnBackClicked(object sender, EventArgs e) => await Navigation.PopAsync();
}
