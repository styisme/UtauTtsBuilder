using UtauTtsBuilder.Services;
using UtauTtsBuilder.Platforms.Android;

namespace UtauTtsBuilder;

public partial class TtsToDSPage : ContentPage
{
    // 中文超长语料（鲁迅《故乡》全篇节选 + 国家百科）
    private const string CorpusZh = 
        "我冒了严寒，回到相隔二千余里，别了二十余年的故乡去。时候既然是深冬；渐近故乡时，天气又阴晦了，冷风吹进船舱中，呜呜的响，从蓬隙向外一望，苍黄的天底下，远近横着几个萧索的荒村，没有一些活气。我的心禁不住悲凉起来了。阿！这不是我二十年来时时记得的故乡？我所记得的故乡全不如此。我的故乡好得多了。但要我记起他的美丽，说出他的佳处来，却又没有影像，没有言辞了。仿佛也就如此。" +
        "这时候，我的脑里忽然闪出一幅神异的图画来：深蓝的天空中挂着一轮金黄的圆月，下面是海边的沙地，都种着一望无际的碧绿的西瓜，其间有一个十一二岁的少年，项带银圈，手捏一柄钢叉，向一匹猹尽力的刺去，那猹却将身一扭，反从他的胯下逃走了。这少年便是闰土。" +
        "我于是日日盼望新年，新年到，闰土也就到了。好容易到了年末，有一日，母亲告诉我，闰土来了，我便飞跑的去看。他正在厨房里，紫色的圆脸，头戴一顶小毡帽，颈上套一个明晃晃的银项圈，这可见他的父亲十分爱他，怕他死去，所以在神佛面前许下愿心，用圈子将他套住了。" +
        "中华人民共和国，简称中国，位于亚洲大陆的东部、太平洋西岸。陆地面积约960万平方公里。中国领土北起漠河以北的黑龙江江心，南到南沙群岛南端的曾母暗沙，东起黑龙江与乌苏里江汇合处，西到帕米尔高原。中国是一个统一的多民族国家，迄今为止，通过识别并由中央政府确认的民族有56个。中国是世界上人口最多的发展中国家。" +
        "萌娘百科对OpenUTAU的介绍：OpenUTAU是由杉田朗主导、由UTAU社区为UTAU社区打造的免费开源歌声合成编辑器，以实现UTAU框架与引擎的现代化而开发，加入了多音轨、伴奏、自动预渲染等功能。" +
        "关于DiffSinger：DiffSinger是一个基于扩散概率模型（Diffusion Probabilistic Model）的声学模型，用于歌声合成（SVS）。它可以将噪声迭代地转换为梅尔频谱图。";

    // 日语超长语料（重音Teto、足立零百科）
    private const string CorpusJa = 
        "重音テト（かさねてと、Kasane Teto）は、2008年4月1日のエイプリルフール前夕に、匿名掲示板2chで「存在しないボーカロイド」として企画されたバーチャルシンガーです。初音ミクなどの影響を受けて誕生し、後にUTAUの音源が提供され、本物のバーチャルシンガーとして活動を始めました。声は小山乃舞世が提供し、キャラクターデザインは線（Sen）が手掛けています。公式設定では年齢31歳、性別はキメラとされています。" +
        "足立レイ（あだち れい、Adachi Rei）は、missile 39（みさいる）によって制作されたオリジナルキャラクターです。正弦波の合成によって作られた声源を持ち、UTAU音源ライブラリやA.I.VOICE音声ライブラリを保有しています。彼女は元々「等身大美少女ロボット」として設計され、その製造計画は2017年に始まりました。2025年1月1日に基本部分が完成し、長年の歳月をかけて実体化が進められています。" +
        "日本国は、アジア大陸の東、太平洋の西岸に位置する島国です。北海道、本州、四国、九州の4つの大きな島と、6800以上の小さな島々からなります。面積は約37.8万平方キロメートル。首都は東京です。日本は環太平洋火山帯に位置しているため、地震や火山活動が非常に活発です。年間を通じて温和で湿潤な気候が特徴です。";

    // 英语超长语料（美国百科）
    private const string CorpusEn = 
        "The United States of America, commonly known as the United States or America, is a country primarily located in North America. It consists of 50 states, a federal district, five major unincorporated territories, nine Minor Outlying Islands, and 326 Indian reservations. It is the world's third-largest country by both land and total area. The United States shares land borders with Canada to its north and with Mexico to its south. It has maritime borders with the Bahamas, Cuba, Russia, and other nations. With a population of over 333 million, it is the most populous country in the Americas and the third most populous in the world. The national capital is Washington, D.C., and the most populous city and financial center is New York City." +
        "OpenUTAU is a free, open-source editor made for the UTAU community. It is developed by a group of volunteers and is designed to modernize the UTAU framework and engine. It features multi-track support, accompaniment, automatic pre-rendering, and a rich MIDI editor. It also provides an extensible real-time phonetic editing system and supports various singing synthesis engines like DiffSinger, NNSVS, and VOICEVOX." +
        "DiffSinger is an acoustic model for Singing Voice Synthesis (SVS) based on the diffusion probabilistic model. It is a parameterized Markov chain that iteratively converts noise into a mel-spectrogram conditioned on the music score. By implicitly optimizing the variational bound, DiffSinger can be stably trained and generate realistic outputs. It introduces a shallow diffusion mechanism to improve voice quality and speed up inference.";

    public TtsToDSPage()
    {
        InitializeComponent();
        UpdateCharCounts();
    }

    private void UpdateCharCounts()
    {
        LblZhCount.Text = $"({CorpusZh.Length} 字)";
        LblJaCount.Text = $"({CorpusJa.Length} 字)";
        LblEnCount.Text = $"({CorpusEn.Length} chars)";
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
            await DialogService.ShowAlertAsync(this, "提示", "请至少勾选一种内置语料或输入自定义文本。", "OK");
            return;
        }

        var engines = TtsEngineHelper.GetAvailableEngines();
        if (engines.Count == 0)
        {
            await DialogService.ShowAlertAsync(this, "错误", "未找到可用的系统 TTS 引擎。", "OK");
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

            StatusLabel.Text = "生成完毕！";
            ProgressBar.Progress = 1;

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "分享 DiffSinger 语料库",
                File = new ShareFile(zipPath)
            });
        }
        catch (Exception ex)
        {
            await DialogService.ShowAlertAsync(this, "生成失败", ex.Message, "OK");
            StatusLabel.Text = "生成中断。";
        }
        finally
        {
            StartBtn.IsEnabled = true;
            ProgressBar.IsVisible = false;
        }
    }

    private async void OnBackClicked(object sender, EventArgs e) => await Navigation.PopAsync();
}
