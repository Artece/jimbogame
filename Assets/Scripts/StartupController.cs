using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Linq;
using Models;

public class StartupController : MonoBehaviour
{
    // easily modifiable parameters for animations
    public float introDur = 2f, pkFlashRate = 2f, menuAnimBgDur = 5f, menuAnimBgTrans = 1f, videoFadeTrans = 1f, buttonFadeTrans = .25f;

    // not implemented yet, list of scenarios
    private List<DirectoryInfo> scDir;
    private List<ScenarioSchema> scenarios;
    private ScenarioSchema selectedScenario;
    private string selectedScenarioPath;
    private ScoreAdjustment currentScore;
    private StoryPath currentBranchPos;
    private VideoPlayer.EventHandler lastEh;

    // shader background panel, video player panel, menu background panel, studio logo panel, scenario select panel, game settings panel
    private GameObject sbp, vpp, mbp, pkp, ssp, gsp;

    private AudioSource mua, b1a, b2a;

    private class AudioSettingsJson
    {
        public float musicVolume = .2f;
        public float button1Volume = .2f;
        public float button2Volume = .2f;
        public AudioSettingsJson(float mv, float b1, float b2)
        {
            musicVolume = mv;
            button1Volume = b1;
            button2Volume = b2;
        }
    }

    // background panel 0 and 1, for menu screen
    private Image bp0, bp1;
    private bool maTick = false;

    public void CallCoroutine(string s)
    {
        var args = s.Split(',');
        object[] argv = new object[args.Length - 1];
        for (int i = 0; i < argv.Length; ++i)
        {
            var v = args[i + 1].Split(':');
            switch (v[0])
            {
                default:
                case "s":
                    argv[i] = v[1];
                    break;
                case "b":
                    argv[i] = bool.Parse(v[1]);
                    break;
                case "i":
                    argv[i] = int.Parse(v[1]);
                    break;
                case "f":
                    argv[i] = float.Parse(v[1]);
                    break;
            }
        }
        if (args.Length < 2)
        {
            argv = null;
        }
        StartCoroutine(args[0], argv);
    }

    private IEnumerator maCatch(IEnumerator clock, bool resetClock)
    {
        // this function only exists to prevent weird behaviour when transitioning back to the menu page
        while (true)
        {
            // basically we detect if the menu page went inactive then if we were due to show the alternative background we set it
            yield return new WaitWhile(() => mbp.activeSelf);
            var bp0a = (maTick) ? 1f : 0f;
            var bp1a = (maTick) ? 0f : 1f;
            bp0.CrossFadeAlpha(bp0a, 0f, true);
            bp1.CrossFadeAlpha(bp1a, 0f, true);
            if (resetClock) StopCoroutine(clock);
            yield return new WaitUntil(() => mbp.activeSelf);
            if (resetClock) StartCoroutine(clock = maClock());
        }
    }

    private IEnumerator maClock()
    {
        while (true)
        {
            yield return new WaitForSeconds(menuAnimBgDur);
            maTick = !maTick;
            var bp0a = (maTick) ? 1f : 0f;
            var bp1a = (maTick) ? 0f : 1f;
            bp0.CrossFadeAlpha(bp0a, menuAnimBgTrans, false);
            bp1.CrossFadeAlpha(bp1a, menuAnimBgTrans, false);
        }
    }

    private void When(YieldInstruction cond, System.Action action)
    {
        StartCoroutine(_When(cond, action));
    }
    private void When(CustomYieldInstruction cond, System.Action action)
    {
        StartCoroutine(_When(cond, action));
    }
    private IEnumerator _When(CustomYieldInstruction cond, System.Action action)
    {
        yield return cond;
        action();
    }
    private IEnumerator _When(YieldInstruction cond, System.Action action)
    {
        yield return cond;
        action();
    }

    private IEnumerator IntroAnimation()
    {
        float qdur = introDur * .25f;

        // get the "press any key" text and the pkp image, and set both to be forsenCD so we can fade in
        pkp.gameObject.SetActive(true);
        var text = pkp.GetComponentInChildren<Text>();
        var pki = pkp.GetComponent<Image>();
        text.CrossFadeAlpha(0f, 0f, true);
        pki.CrossFadeAlpha(0f, 0f, true);

        // wait for a bit (smoother looking during testing)
        yield return new WaitForSeconds(qdur);

        text.CrossFadeAlpha(1f, qdur, false);
        pki.CrossFadeAlpha(1f, qdur, false);

        yield return new WaitForSeconds(qdur);

        sbp.SetActive(true);

        // slowly pulse the text until a kep is pressed
        // first get the sin offset by taking the current time times the flash rate and adding half pi
        var sinOffset = 1.57f + Time.time * pkFlashRate;
        // then loop until we get an input
        while (!Input.anyKey)
        {
            // fade the text to the absolute sin of the rate-scaled time value with the offset
            text.CrossFadeAlpha(Mathf.Abs(Mathf.Sin((Time.time * pkFlashRate) - sinOffset)), Time.deltaTime, false);
            // don't wait, we want to come back next frame or occasionally you'll miss an input which feels scuffed
            yield return null;
        }

        // play sound when we enter
        GameObject.Find("Button2Audio").GetComponent<AudioSource>().Play();

        // after user input flash the text three times rapidly before fading out completely
        for (int i = 0; i < 3; i++)
        {
            text.CrossFadeAlpha(0f, 0.05f, false);
            yield return new WaitForSeconds(0.05f);
            text.CrossFadeAlpha(1f, 0.05f, false);
            yield return new WaitForSeconds(0.05f);
        }
        text.CrossFadeAlpha(0f, 0.05f, false);
        yield return new WaitForSeconds(0.05f);

        // then fade out press any key screen
        pki.CrossFadeAlpha(0f, qdur, false);

        // set mbp to active
        mbp.SetActive(true);

        // set buttons to be non-interactable as otherwise their mouseover animations can fuck up the fade in
        var bns = new List<Button>(mbp.GetComponentsInChildren<Button>());
        bns.ForEach((b) => b.interactable = false);

        // loop through all graphical components of mbp and make transparent
        var gfx = new List<Graphic>(mbp.GetComponentsInChildren<Graphic>());
        gfx.ForEach((g) => g.CrossFadeAlpha(0f, 0f, true));

        // then wait
        yield return new WaitForSeconds(qdur);

        // deactivate pkp as no longer needed
        pkp.SetActive(false);

        // start the fade in of mbp
        gfx.ForEach((g) => g.CrossFadeAlpha(1f, qdur, false));

        // just experimenting with forking coroutines with lambda exps
        //When(new WaitForSeconds(1f), () => bns.ForEach((b) => b.interactable = true));

        // get the background panels
        bp0 = mbp.transform.Find("BP0").gameObject.GetComponent<Image>();
        bp1 = mbp.transform.Find("BP1").gameObject.GetComponent<Image>();

        // override the fade in on bp0 as we only want one bg to start with
        bp0.CrossFadeAlpha(0f, 0f, true);

        // then enter menu animation
        IEnumerator clock;
        StartCoroutine(clock = maClock());
        StartCoroutine(maCatch(clock, true));

        // make sure to reenable them again ;)
        // much simpler to just wait like this than use When()
        yield return new WaitForSeconds(qdur);
        bns.ForEach((b) => b.interactable = true);
    }

    private Sprite LoadSprite(string path)
    {
        byte[] d = File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(0, 0);
        tex.LoadImage(d);
        var sprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2());
        return sprite;
    }

    // not implemented
    public void ParseScenarios()
    {
        var strs = new string[2]{ "", "" };
        vpUrlQ = new Queue<string>(strs);

        // get a list of all the scenarios in the scenarios folder
        DirectoryInfo dir = new DirectoryInfo(Application.streamingAssetsPath + "/Scenarios");
        scDir = new List<DirectoryInfo>(dir.GetDirectories());
        scenarios = new List<ScenarioSchema>();
        // get a list of the buttons on SSP
        var buttons = ssp.transform.GetComponentsInChildren<Button>();
        for (int i = 0; i < Mathf.Min(buttons.Length, scDir.Count); ++i)
        {
            // start on the 1st button as 0th is back, and get the image
            var img = buttons[i + 1].gameObject.GetComponent<Image>();
            var txt = buttons[i + 1].gameObject.GetComponentInChildren<Text>();
            // make the path by combining the scenarios folder, with the scenario folder in question, with the filename
            // this has to be relative to the resources folder because Resources.Load will only look in there
            // Resources.Load also hates file extensions for some inexplicable reason, so beware
            var path = Application.streamingAssetsPath + "/Scenarios/" + scDir[i].Name;

            // actually load it
            var sprite = LoadSprite(path + "/bg.png");
            var json = File.ReadAllText(path + "/scenario.json");
            ScenarioSchema scenario = (ScenarioSchema)JsonUtility.FromJson(json, typeof(ScenarioSchema));
            scenarios.Add(scenario);

            // and assign
            img.sprite = sprite;
            txt.text = scenario.Title + "\n" + scenario.Subtitle + "\n" + scenario.Description;
        }
        // go through and make the rest transparent because we wanna see the pog bg shader not empty white rectangle sprites
        for (int i = Mathf.Min(buttons.Length, scDir.Count); i < Mathf.Max(buttons.Length, scDir.Count) - 1; ++i)
        {
            buttons[i + 1].gameObject.GetComponent<Image>().CrossFadeAlpha(0f, 0f, true);
            buttons[i + 1].interactable = false;
        }

        new List<Graphic>(vpp.transform.Find("MidRow").GetComponentsInChildren<Graphic>()).ForEach((g) =>
        {
            g.gameObject.SetActive(false);
            GameObject.Destroy(g.gameObject);
        });
    }

    private IEnumerator LoadScenario(object[] parms)
    {
        int index = (int)parms[0];
        selectedScenario = scenarios[index];
        selectedScenarioPath = Application.streamingAssetsPath + "/Scenarios/" + scDir[index].Name + "/";

        sbp.SetActive(false);
        //vpp.SetActive(true);

        currentBranchPos = selectedScenario.Settings.StartPath;
        currentScore = selectedScenario.Settings.StartScore;

        GameChoice();

        var gfx = new List<Graphic>(vpp.GetComponentsInChildren<Graphic>());
        var bns = new List<Button>(vpp.GetComponentsInChildren<Button>());
        RawImage ri = vpp.GetComponent<RawImage>();
        gfx.ForEach((g) => g.CrossFadeAlpha(0f, 0f, true));
        gfx.Remove(ri);
        bns.ForEach((b) => b.gameObject.SetActive(false));
        vpp.SetActive(true);

        var vpc = vpp.GetComponent<VideoPlayer>();
        SetVideoUrl(vpc, selectedScenarioPath + "pre.avi");
        vpc.Play();
        ri.CrossFadeAlpha(1f, videoFadeTrans, false);

        yield return new WaitUntil(() => vpc.isPlaying);
        yield return new WaitWhile(() => vpc.isPlaying);

        bns.ForEach((b) => b.gameObject.SetActive(true));
        gfx.ForEach((g) => g.CrossFadeAlpha(1f, 1f, false));
    }

    [System.Serializable]
    private class ButtonArrayWrapper
    {
        public ButtonSchema[] values;
    }

    public void GameChoice() {
        // get the mid row
        var mr = vpp.transform.Find("MidRow");
        var sprite = mr.parent.transform.Find("TopRow").Find("Back").GetComponent<Image>().sprite;

        // clear all current children of MidRow i.e. existing buttons
        foreach (Transform c in mr.transform)
        {
            GameObject.Destroy(c.gameObject);
        }

        // parse the options file for button layout
        // have to wrap in an object as JsonUtility doesn't support just arrays to start apparently
        var buttonsJson = "{\"values\":" + File.ReadAllText(selectedScenarioPath + currentBranchPos + "options.json") + "}";
        var gameButtons = new List<ButtonSchema>(((ButtonArrayWrapper)JsonUtility.FromJson(buttonsJson, typeof(ButtonArrayWrapper))).values);
        var vpc = vpp.GetComponent<VideoPlayer>();

        var bb = vpp.transform.Find("TopRow").transform.Find("Back").gameObject;

        // add a new child to MidRow for each ButtonSchema
        //gameButtons.ForEach((b) =>
        foreach (ButtonSchema b in gameButtons)
        {
            // add the button gameobject and make it child of mr
            //var go = new GameObject(b.Label);
            var go = GameObject.Instantiate(bb);
            go.name = b.Label;
            go.transform.SetParent(mr.transform, false);
            // give it a canvasrenderer so we can see it
            //go.AddComponent<CanvasRenderer>();
            
            // give it an image and set it to be the default ui sprite
            //var img = go.AddComponent<Image>();
            //img.type = Image.Type.Sliced;
            //img.sprite = sprite;
            //img.CrossFadeAlpha(0f, 0f, true);
            //img.sprite.

            // give it a button script
            //var btn = go.AddComponent<Button>();
            if (b.Path.Branch == null)
            {
                b.Path.Branch = currentBranchPos.Branch;
                b.Path.StartPosition = currentBranchPos.StartPosition + 1;
            }

            var btn = go.GetComponent<Button>();
            
            //// wish we could just do btn.colors.disabledColor = Color.white
            //var colors = btn.colors;
            //colors.disabledColor = Color.white;
            //btn.colors = colors;

            for (int i = 0; i < btn.onClick.GetPersistentEventCount(); ++i)
            {
                btn.onClick.SetPersistentListenerState(i, UnityEngine.Events.UnityEventCallState.Off);
            }

            btn.onClick.AddListener(() => {
                currentScore += b.ScoreAdjustment;

                // get rid of the buttons
                new List<Button>(mr.transform.GetComponentsInChildren<Button>()).ForEach((b) => b.interactable = false);
                new List<Graphic>(mr.transform.GetComponentsInChildren<Graphic>()).ForEach((g) =>
                {
                    g.CrossFadeAlpha(0f, buttonFadeTrans, false);
                    When(new WaitForSeconds(buttonFadeTrans), () => g.gameObject.SetActive(false));
                });

                // play button sound
                b1a.Play();

                // update video player clip
                SetVideoUrl(vpc, selectedScenarioPath + currentBranchPos + b.VideoFilename);

                var noCondEnding = true;
                if (b.Endings.Count > 0)
                {
                    // endings
                    foreach (var ending in b.Endings)
                    {
                        if (currentScore.Points >= ending.WhenPointsAreBetween[0] && currentScore.Points < ending.WhenPointsAreBetween[1])
                        {
                            // use this ending
                            vpc.loopPointReached += AddLoopPointEventHandler((vp) =>
                            {
                                // don't use setvideourl here because we don't wanna have two jump back two
                                vpc.url = selectedScenarioPath + "endings/" + ending.VideoFilename;
                                //SetVideoUrl(vp, selectedScenarioPath + "endings/" + ending.VideoFilename);
                                vp.loopPointReached += AddLoopPointEventHandler((vp) => EndScreen(b));
                                vp.Play();
                            });
                            noCondEnding = false;
                            break;
                        }
                    }
                        
                }

                if (noCondEnding)
                {
                    // detect health etc.
                    if (currentScore.HP < 1 || b.ButtonType == ButtonType.End.ToString())
                    {
                        // lose
                        vpc.loopPointReached += AddLoopPointEventHandler((vp) =>
                        EndScreen(b));
                    }
                    else
                    {
                        // trigger the next screen
                        vpc.loopPointReached += AddLoopPointEventHandler((vp) =>
                        GameChoice());
                        //vpc.loopPointReached += (DumbStuff() += (vp) => GameChoice());
                        // update the branch pos
                        currentBranchPos = b.Path;
                    }
                }

                // play video after delay
                When(new WaitForSeconds(buttonFadeTrans), () => vpc.Play());
            });

            // give it a child with a text component for the label
            //var tc = new GameObject("Text");
            //tc.transform.parent = go.transform;
            //var txt = tc.AddComponent<Text>();
            var txt = go.GetComponentInChildren<Text>();
            txt.text = b.Label;
            //txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            //txt.color = Color.black;
            //txt.fontSize = 34;
            //txt.alignment = TextAnchor.MiddleCenter;
            //txt.horizontalOverflow = HorizontalWrapMode.Overflow;
            //txt.verticalOverflow = VerticalWrapMode.Overflow;
        }//);
    }

    public float doSpeedInsteadOfSkip = 1f;

    public void SkipToEnd(VideoPlayer vpc)
    {
        if (doSpeedInsteadOfSkip > 1f)
        {
            vpc.playbackSpeed = doSpeedInsteadOfSkip;
        }
        else
        {
            vpc.frame = (long)(vpc.frameCount - 10);
        }
    }

    private Queue<string> vpUrlQ;
    private void SetVideoUrl(VideoPlayer vp, string url)
    {
        vp.url = url;
        vpUrlQ.Dequeue();
        vpUrlQ.Enqueue(url);
    }

    public void RemovelastEh(VideoPlayer vp)
    {
        vp.loopPointReached -= lastEh;
    }

    private VideoPlayer.EventHandler AddLoopPointEventHandler(VideoPlayer.EventHandler ourCode)
    {
        VideoPlayer.EventHandler eh = (vp) => { };
        eh += (vp) => vp.loopPointReached -= eh;
        eh += ourCode;
        lastEh = eh;
        return eh;
    }

    public void EndScreen(ButtonSchema b)
    {
        // get the mid row
        var mr = vpp.transform.Find("MidRow");
        var sprite = mr.parent.transform.Find("TopRow").Find("Back").GetComponent<Image>().sprite;

        // clear all current children of MidRow i.e. existing buttons
        foreach (Transform c in mr.transform)
        {
            GameObject.Destroy(c.gameObject);
        }// add the button gameobject and make it child of mr

        var go = new GameObject("Retry Button");
        go.transform.parent = mr.transform;
        // give it a canvasrenderer so we can see it
        go.AddComponent<CanvasRenderer>();

        // give it an image and set it to be the default ui sprite
        var img = go.AddComponent<Image>();
        img.type = Image.Type.Sliced;
        img.sprite = sprite;
        //img.sprite.

        var vpc = vpp.GetComponent<VideoPlayer>();

        // give it a button script
        var btn = go.AddComponent<Button>();
        btn.onClick.AddListener(() =>
        {
            go.SetActive(false);
            GameObject.Destroy(go);
            SetVideoUrl(vpc, vpUrlQ.Peek());
            vpc.Play();
            vpc.loopPointReached += AddLoopPointEventHandler((vp) => GameChoice());
        });
        
        // give it a child with a text component for the label
        var tc = new GameObject("Text");
        tc.transform.parent = go.transform;
        var txt = tc.AddComponent<Text>();
        txt.text = (b.EndScreenMessage == null || b.EndScreenMessage == "") ? "Try again?" : b.EndScreenMessage;
        txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        txt.color = Color.black;
        txt.fontSize = 34;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;
        txt.verticalOverflow = VerticalWrapMode.Overflow;
    }

    public void LoadSettings()
    {
        var asj = (AudioSettingsJson)JsonUtility.FromJson(File.ReadAllText(Application.persistentDataPath + "/AudioSettings.json"), typeof(AudioSettingsJson));
        mua.volume = asj.musicVolume;
        b1a.volume = asj.button1Volume;
        b2a.volume = asj.button2Volume;
    }

    public void SaveSettings()
    {
        var asj = new AudioSettingsJson(mua.volume, b1a.volume, b2a.volume);
        var str = JsonUtility.ToJson(asj);
        File.WriteAllText(Application.persistentDataPath + "/AudioSettings.json", str);
    }

    private IEnumerator Quit() {
        var quitDur = .8f;
        var canv = GameObject.Find("Canvas");
        var gfx = canv.GetComponentsInChildren<Graphic>();
        var bns = canv.GetComponentsInChildren<Button>();
        foreach (Button b in bns)
        {
            b.interactable = false;
        }
        yield return new WaitForSeconds(quitDur * .25f);
        foreach (Graphic g in gfx)
        {
            g.CrossFadeAlpha(0f, quitDur *.5f, false);
        }
        yield return new WaitForSeconds(quitDur * .75f);
#if UNITY_EDITOR
        // Application.Quit() does not work in the editor so
        // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /*  _____ _   _ _____________   __ ______ _____ _____ _   _ _____ 
     * |  ___| \ | |_   _| ___ \ \ / / | ___ \  _  |_   _| \ | |_   _|
     * | |__ |  \| | | | | |_/ /\ V /  | |_/ / | | | | | |  \| | | |  
     * |  __|| . ` | | | |    /  \ /   |  __/| | | | | | | . ` | | |  
     * | |___| |\  | | | | |\ \  | |   | |   \ \_/ /_| |_| |\  | | |  
     * \____/\_| \_/ \_/ \_| \_| \_/   \_|    \___/ \___/\_| \_/ \_/  
     */                                                        

    // Start is called before the first frame update
    void Start()
    {
        // grab references to important objects
        sbp = transform.Find("SBP").gameObject;
        vpp = transform.Find("VPP").gameObject;
        var vpc = vpp.GetComponent<VideoPlayer>();
        var vpb = vpp.GetComponent<Button>();
        vpc.loopPointReached += (vp) => vp.playbackSpeed = 1f;
        vpc.loopPointReached += (vp) => vpb.interactable = false;
        vpc.started += (vp) => vpb.interactable = true;
        mbp = transform.Find("MBP").gameObject;
        pkp = transform.Find("PKP").gameObject;
        ssp = transform.Find("SSP").gameObject;
        gsp = transform.Find("GSP").gameObject;
        mua = GameObject.Find("MusicAudio").GetComponent<AudioSource>();
        b1a = GameObject.Find("Button1Audio").GetComponent<AudioSource>();
        b2a = GameObject.Find("Button2Audio").GetComponent<AudioSource>();
        LoadSettings();
        StartCoroutine(IntroAnimation());
        //var bs = new ButtonSchema();
        //bs.ButtonType = ButtonType.End.ToString();
        //bs.Label = "test";
        //File.WriteAllText(Application.persistentDataPath + "/test.json", JsonUtility.ToJson(bs, true));
    }

    // Update is called once per frame
    //void Update()
    //{
        
    //}
}
