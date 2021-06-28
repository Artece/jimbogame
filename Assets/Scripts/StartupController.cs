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

    public bool showScore { get; set; }

    // not implemented yet, list of scenarios
    private List<DirectoryInfo> scDir;
    private List<ScenarioSchema> scenarios;
    private ScenarioSchema selectedScenario;
    private string selectedScenarioPath;
    private ScoreAdjustment currentScore;
    private StoryPath currentBranchPos;

    // shader background panel, video player panel, menu background panel, studio logo panel, scenario select panel, game settings panel
    private GameObject sbp, vpp, mbp, pkp, ssp, gsp;

    private Text subtitleText;

    private AudioSource mua, b1a, b2a, via;

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

    private Texture2D LoadTexture(string path)
    {
        byte[] d = File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(0, 0);
        tex.LoadImage(d);
        return tex;
    }

    private Sprite LoadSprite(string path)
    {
        var tex = LoadTexture(path);
        var sprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2());
        return sprite;
    }

    // not implemented
    public void ParseScenarios()
    {
        mediaQueue = new Queue<string>();

        GameObject.Find("DEBUG").GetComponent<Text>().text = "";

        if (!mua.isPlaying)
            mua.Play();

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
            var path = Application.streamingAssetsPath + "/Scenarios/" + scDir[i].Name + "/";

            // actually load it
            var json = File.ReadAllText(path + "scenario.json");
            ScenarioSchema scenario = (ScenarioSchema)JsonUtility.FromJson(json, typeof(ScenarioSchema));
            scenarios.Add(scenario);
            var sprite = LoadSprite(path + scenario.Image);

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

        // recreate video player to guarantee we get rid of dangling event handlers etc
        var vp = vpp.GetComponent<VideoPlayer>();
        var vt = vp.targetTexture;
        Destroy(vp);
        vp = vpp.AddComponent<VideoPlayer>();
        vp.audioOutputMode = VideoAudioOutputMode.AudioSource;
        vp.SetTargetAudioSource(0, via);
        vp.aspectRatio = VideoAspectRatio.Stretch;
        vp.targetTexture = (RenderTexture)vt;
        vp.isLooping = false;
        vp.skipOnDrop = true;
        vp.waitForFirstFrame = true;
        vp.playOnAwake = false;

        // set our image to be videotex again
        var ri = vpp.GetComponent<RawImage>();
        ri.texture = vt;

        // flush the video texture to prevent displaying old contents
        ClearVideoTexture();

        new List<Graphic>(vpp.transform.Find("MidRow").GetComponentsInChildren<Graphic>()).ForEach((g) =>
        {
            g.gameObject.SetActive(false);
            GameObject.Destroy(g.gameObject);
        });
    }

    private void ClearVideoTexture()
    {
        var vp = vpp.GetComponent<VideoPlayer>();
        vp.Stop();
        var vt = vp.targetTexture;
        var rt = RenderTexture.active;
        RenderTexture.active = vt;
        GL.Clear(true, true, Color.clear);
        RenderTexture.active = rt;
    }

    private IEnumerator LoadScenario(object[] parms)
    {
        int index = (int)parms[0];
        selectedScenario = scenarios[index];
        selectedScenarioPath = Application.streamingAssetsPath + "/Scenarios/" + scDir[index].Name + "/";

        sbp.SetActive(false);
        //vpp.SetActive(true);

        currentBranchPos = new StoryPath(selectedScenario.Settings.StartingBranch, selectedScenario.Settings.StartingPathPosition);
        currentScore = new ScoreAdjustment(selectedScenario.Settings.StartingHP, selectedScenario.Settings.StartingPoints);

        mua.Stop();

        if (showScore)
        {
            GameObject.Find("DEBUG").GetComponent<Text>().text = currentScore.ToString();
        }

        var vpc = vpp.GetComponent<VideoPlayer>();
        vpc.url = selectedScenarioPath + selectedScenario.IntroVideo;

        GameChoice();

        var gfx = new List<Graphic>(vpp.GetComponentsInChildren<Graphic>());
        var bns = new List<Button>(vpp.GetComponentsInChildren<Button>());
        RawImage ri = vpp.GetComponent<RawImage>();
        gfx.ForEach((g) => g.CrossFadeAlpha(0f, 0f, true));
        gfx.Remove(ri);
        gfx.Remove(subtitleText);
        subtitleText.CrossFadeAlpha(1f, 0f, true);
        ri.CrossFadeAlpha(1f, videoFadeTrans, false);
        bns.ForEach((b) => b.gameObject.SetActive(false));
        
        vpp.SetActive(true);
        string[] vs = { selectedScenario.IntroVideo };
        AppendMediaQueue(vpc, selectedScenarioPath, vs);
        PlayMediaQueue(vpc);

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

        foreach (var sc in SubCoroutines)
        {
            StopCoroutine(sc);
        }
        SubCoroutines.Clear();

        // clear all current children of MidRow i.e. existing buttons
        foreach (Transform c in mr.transform)
        {
            GameObject.Destroy(c.gameObject);
        }

        if (!Directory.Exists(selectedScenarioPath + currentBranchPos))
        {
            currentBranchPos.StartPosition--;
            EndScreen();
            return;
        }
        // parse the options file for button layout
        // have to wrap in an object as JsonUtility doesn't support just arrays to start apparently
        var buttonsJson = "{\"values\":" + File.ReadAllText(selectedScenarioPath + currentBranchPos + "options.json") + "}";
        var gameButtons = new List<ButtonSchema>(((ButtonArrayWrapper)JsonUtility.FromJson(buttonsJson, typeof(ButtonArrayWrapper))).values);
        var vpc = vpp.GetComponent<VideoPlayer>();

        var bb = vpp.transform.Find("TopRow").transform.Find("Back").gameObject;

        retryVideo = vpc.url;

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
                if (showScore)
                {
                    GameObject.Find("DEBUG").GetComponent<Text>().text = currentScore.ToString();
                }

                // get rid of the buttons
                new List<Button>(mr.transform.GetComponentsInChildren<Button>()).ForEach((b) => b.interactable = false);
                new List<Graphic>(mr.transform.GetComponentsInChildren<Graphic>()).ForEach((g) =>
                {
                    g.CrossFadeAlpha(0f, buttonFadeTrans, false);
                    When(new WaitForSeconds(buttonFadeTrans), () => g.gameObject.SetActive(false));
                });

                // play button sound
                b1a.Play();

                When(new WaitForSeconds(buttonFadeTrans), () =>
                {
                    System.Action action = null;
                    if (b.Videos.Count > 0)
                    {
                        foreach (var video in b.Videos)
                        {
                            if (currentScore.Points >= video.WhenPointsAreBetween[0] && currentScore.Points < video.WhenPointsAreBetween[1])
                            {
                                // use this video
                                string[] urls = video.VideoFilename.ToArray();
                                AppendMediaQueue(vpc, selectedScenarioPath + currentBranchPos, urls);
                                break;
                            }
                        }
                    }

                    var noCondEnding = true;
                    if (b.Endings.Count > 0)
                    {
                        // endings
                        foreach (var ending in b.Endings)
                        {
                            if (currentScore.Points >= ending.WhenPointsAreBetween[0] && currentScore.Points < ending.WhenPointsAreBetween[1])
                            {
                                // use this ending
                                string[] urls = { ending.VideoFilename };
                                AppendMediaQueue(vpc, selectedScenarioPath + "endings/", urls);
                                action = () => EndScreen(ending.EndScreenMessage, b.ButtonType);
                                noCondEnding = false;
                                break;
                            }
                        }
                    }

                    if (noCondEnding)
                    {
                        // detect health etc.
                        if (b.ButtonType == ButtonType.End.ToString() || currentScore.HP < 1)
                        {
                            // lose
                            AppendMediaQueue(vpc, selectedScenarioPath + currentBranchPos, b.VideoFilename);
                            action = () => EndScreen(b.EndScreenMessage, b.ButtonType);
                        }
                        else
                        {
                            AppendMediaQueue(vpc, selectedScenarioPath + currentBranchPos, b.VideoFilename);
                            action = () => GameChoice();
                            currentBranchPos = b.Path;
                        }
                    }

                    PlayMediaQueue(vpc, action);
                });
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

    public void EndScreen(string EndScreenMessage = null, string buttonType = null)
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

        if (buttonType == null || buttonType == ButtonType.Outcome.ToString())
        {
            btn.onClick.AddListener(() =>
            {
                GameObject.Find("DEBUG").GetComponent<Text>().text = "";
                go.SetActive(false);
                GameObject.Destroy(go);
                vpp.SetActive(false);
                sbp.SetActive(true);
                mbp.SetActive(true);
                b2a.Play();
                mua.Play();
            });
        }
        else if (buttonType == ButtonType.End.ToString())
        {
            btn.onClick.AddListener(() =>
            {
                go.SetActive(false);
                GameObject.Destroy(go);
                PlayMediaUrl(vpc, retryVideo);
                vpc.loopPointReached += AddVideoEventHandler((vp) => GameChoice());
            });
        }

        // give it a child with a text component for the label
        var tc = new GameObject("Text");
        tc.transform.parent = go.transform;
        var txt = tc.AddComponent<Text>();
        txt.text = (EndScreenMessage == null || EndScreenMessage == "") ? "Try again?" : EndScreenMessage;
        txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        txt.color = Color.black;
        txt.fontSize = 34;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;
        txt.verticalOverflow = VerticalWrapMode.Overflow;
    }

    public float doSpeedInsteadOfSkip = 1f;

    public void SkipToEnd()
    {
        var vp = vpp.GetComponent<VideoPlayer>();
        if (doSpeedInsteadOfSkip > 1f)
        {
            vp.playbackSpeed = doSpeedInsteadOfSkip;
        }
        else
        {
            vp.frame = (long)(vp.frameCount);
            //vpc.frame = (long)(vpc.frameCount - 10);
        }
    }

    private List<IEnumerator> SubCoroutines;
    private void PlaySubtitles(VideoPlayer vp)
    {
        var foo = vp.url.Split('/');
        var bar = foo[foo.Length - 1].Split('.');
        var baz = selectedScenarioPath + "Subtitles/" + bar[0] + ".srt";
        var contents = File.ReadAllText(baz);
        contents = contents.Trim();
        print(contents);
        print("ENVNL" + System.Environment.NewLine + "ENVNL");

        gsp.SetActive(true);
        var sel = GameObject.Find("ti").transform.Find("InputField").transform.Find("Text").GetComponent<Text>().text;
        gsp.SetActive(false);

        var splitter = System.Environment.NewLine;
        if (sel != null || sel != "")
        {
            if (sel.StartsWith("\\n")) splitter = "\n";
            if (sel.StartsWith("\\r")) splitter = "\r";
            if (sel.StartsWith("\\r\\n")) splitter = "\r\n";
        }
        splitter += splitter;
        //splitter = "\r\n\r\n";
        string[] sections;
        if (sel.EndsWith("regex"))
        {
            sections = System.Text.RegularExpressions.Regex.Split(contents, splitter);
        }
        else
        {
            sections = contents.Split(new string[] { splitter }, System.StringSplitOptions.RemoveEmptyEntries);
        }
        print("sectons length: " + sections.Length);
        foreach (var s in sections)
        {
            print("section");
            print(s);
            print("section end");
            var nlines = s.Split(new string[] { System.Environment.NewLine }, System.StringSplitOptions.RemoveEmptyEntries);
            var olines = s.Split('\n');
            var lines = olines;
            print("lines");
            foreach (var l in lines) print(l);
            print("lines end");
            print("A");
            var times = lines[1].Split(new string[] { " --> " }, System.StringSplitOptions.None);
            print("B");
            var sta = times[0].Split(':');
            print("C");
            var sta2 = sta[2].Split(',');
            print("D");
            float start = float.Parse(sta[0]) * 3600f + float.Parse(sta[1]) * 60f + float.Parse(sta2[0]) + float.Parse(sta2[1]) * .001f;
            print("E");
            var fin = times[1].Split(':');
            print("F");
            var fin2 = fin[2].Split(',');
            print("G");
            float finish = float.Parse(fin[0]) * 3600f + float.Parse(fin[1]) * 60f + float.Parse(fin2[0]) + float.Parse(fin2[1]) * .001f;
            print("H");
            var sub = lines[2];
            print("scheduled start time: " + start + " | scheduled finish time: " + finish + " | text to display: " + sub);
            var eps = 0.05f;
            // is below a race condition? will try adding an eps to start
            // might need to consider a queue of some kind here
            print("I");
            var staIe = _When(new WaitForSeconds(start + eps), () =>
            {
                subtitleText.text = sub;
                print("start time: " + start + " reached, displaying text: " + sub);
            });
            print("J");
            var finIe = _When(new WaitForSeconds(finish), () =>
            {
                subtitleText.text = "";
                print("finish time: " + finish + " reached, clearing text: " + sub);
            });
            print("K");
            SubCoroutines.Add(staIe);
            print("L");
            SubCoroutines.Add(finIe);
            print("M");
            StartCoroutine(staIe);
            print("N");
            StartCoroutine(finIe);
            print("O");
            vp.loopPointReached += AddVideoEventHandler((vp) =>
            {
                subtitleText.text = "";
                StopCoroutine(staIe);
                StopCoroutine(finIe);
                SubCoroutines.Remove(staIe);
                SubCoroutines.Remove(finIe);
            });
            print("P");
        }
    }

    private bool PlayMediaUrl(VideoPlayer vp, string url)
    {
        List<string> supportedTypes = new List<string>(new string[]{"asf","avi" ,"dv " ,"m4v" ,"mov" ,"mp4" ,"mpg" ,"mpeg","ogv" ,"vp8" ,"webm","wmv"});
        var ext = url.Split('.')[1];
        // check if file ext is supported video type, if not assume it's an image
        if (!supportedTypes.Contains(ext))
        {
            var tex = LoadTexture(url);
            var ri = vpp.GetComponent<RawImage>();
            var vt = ri.texture;
            ri.texture = tex;
            ClearVideoTexture();
            vp.started += AddVideoEventHandler((vp) => ri.texture = vt);
            return true;
        }
        vp.url = url;
        PlaySubtitles(vp);
        vp.Play();
        return false;
    }

    private string retryVideo;
    private Queue<string> mediaQueue;

    private void PlayMediaQueue(VideoPlayer vp, System.Action action = null)
    {
        var png = PlayMediaUrl(vp, mediaQueue.Dequeue());
        if (png)
        {
            if (mediaQueue.Count > 1)
            {
                vp.url = mediaQueue.Dequeue();
                When(new WaitForSeconds(1f), () =>
                {
                    vp.Play();
                    PlaySubtitles(vp);
                });
            }
            else
            {
                When(new WaitForSeconds(1f), action);
            }
        }
        vp.loopPointReached += AddVideoEventHandler((vp) =>
        {
            if (mediaQueue.Count < 1)
            {
                if (action != null)
                {
                    action();
                }
            }
            else
            {
                PlayMediaQueue(vp, action);
            }
        });
    }

    private void AppendMediaQueue(VideoPlayer vp, string path, string[] urls)
    {
        if (mediaQueue == null)
        {
            mediaQueue = new Queue<string>();
        }
        foreach (var s in urls)
        {
            mediaQueue.Enqueue(path + s);
        }
    }

    private VideoPlayer.EventHandler AddVideoEventHandler(VideoPlayer.EventHandler ourCode)
    {
        VideoPlayer.EventHandler eh = (vp) => { };
        eh += (vp) => vp.loopPointReached -= eh;
        eh += ourCode;
        return eh;
    }

    private VideoPlayer.ErrorEventHandler AddVideoErrorEventHandler(VideoPlayer.ErrorEventHandler ourCode)
    {
        VideoPlayer.ErrorEventHandler eh = (vp, s) => { };
        eh += (vp, s) => vp.errorReceived -= eh;
        eh += ourCode;
        return eh;
    }

    private class SettingsJson
    {
        public float musicVolume = .2f;
        public float buttonVolume = .2f;
        public float videoVolume = .2f;
        public bool showScore = true;
        public SettingsJson(float mv, float bv, float vv, bool ss)
        {
            musicVolume = mv;
            buttonVolume = bv;
            videoVolume = vv;
            showScore = ss;
        }
    }

    public void LoadSettings()
    {
        var path = Application.persistentDataPath + "/Settings.json";
        if (File.Exists(path))
        {
            var asj = (SettingsJson)JsonUtility.FromJson(File.ReadAllText(path), typeof(SettingsJson));
            mua.volume = asj.musicVolume;
            b1a.volume = asj.buttonVolume;
            b2a.volume = asj.buttonVolume;
            via.volume = asj.videoVolume;
            showScore = asj.showScore;
        }
        var mr = gsp.transform.Find("MidRow");
        mr.Find("mv").Find("Slider").gameObject.GetComponent<Slider>().value = mua.volume;
        mr.Find("bv").Find("Slider").gameObject.GetComponent<Slider>().value = b1a.volume;
        mr.Find("vv").Find("Slider").gameObject.GetComponent<Slider>().value = via.volume;
        mr.Find("ss").gameObject.GetComponent<Toggle>().isOn = showScore;
    }

    public void SaveSettings()
    {
        var asj = new SettingsJson(mua.volume, b1a.volume, via.volume, showScore);
        var str = JsonUtility.ToJson(asj);
        File.WriteAllText(Application.persistentDataPath + "/Settings.json", str);
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
        subtitleText = vpp.transform.Find("BotRow").GetComponentInChildren<Text>();
        mbp = transform.Find("MBP").gameObject;
        pkp = transform.Find("PKP").gameObject;
        ssp = transform.Find("SSP").gameObject;
        gsp = transform.Find("GSP").gameObject;
        mua = GameObject.Find("MusicAudio").GetComponent<AudioSource>();
        b1a = GameObject.Find("Button1Audio").GetComponent<AudioSource>();
        b2a = GameObject.Find("Button2Audio").GetComponent<AudioSource>();
        via = GameObject.Find("VideoAudio").GetComponent<AudioSource>();
        LoadSettings();
        StartCoroutine(IntroAnimation());
        SubCoroutines = new List<IEnumerator>();

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
