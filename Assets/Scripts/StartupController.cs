using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Models;

public class StartupController : MonoBehaviour
{
    // easily modifiable parameters for animations
    public float introDuration = 2f, pressAnyKeyFlashRate = 2f, menuAnimationBackgroundDuration = 5f, menuAnimationTransitionDuration = 1f;
    
    // not implemented yet, list of scenarios
    private List<DirectoryInfo> scenarios;
    
    // shader background panel, video player panel, menu background panel, studio logo panel, scenario select panel, game settings panel
    private GameObject sbp, vpp, mbp, pkp, ssp, gsp;
    
    // background panel 0 and 1, for menu screen
    private Image bp0, bp1;
    private bool bp0Active = false;

    private IEnumerator MenuAnimationCatch()
    {
        // this function only exists to prevent weird behaviour when transitioning back to the menu page
        while (true)
        {
            // basically we detect if the menu page went inactive then if we were due to show the alternative background we set it
            yield return new WaitUntil(() => !mbp.activeSelf);
            if (bp0Active)
            {
                bp0.CrossFadeAlpha(1f, 0f, true);
                bp1.CrossFadeAlpha(0f, 0f, true);
            }
            yield return new WaitUntil(() => mbp.activeSelf);
        }
    }

   private IEnumerator MenuAnimation()
    {
        StartCoroutine(MenuAnimationCatch());
        // loop forever
        while (true)
        {
            bp0Active = false;
            // fade out bp0 over transition duration seconds and fade in bp1
            bp0.CrossFadeAlpha(0f, menuAnimationTransitionDuration, false);
            bp1.CrossFadeAlpha(1f, menuAnimationTransitionDuration, false);
            // wait until the menu screen is active
            yield return new WaitUntil(() => mbp.activeSelf);
            // wait until we have to change backgrounds again
            yield return new WaitForSeconds(menuAnimationBackgroundDuration);

            // rinse and repeat only in reverse
            bp0Active = true;
            bp0.CrossFadeAlpha(1f, menuAnimationTransitionDuration, false);
            bp1.CrossFadeAlpha(0f, menuAnimationTransitionDuration, false);
            yield return new WaitUntil(() => mbp.activeSelf);
            yield return new WaitForSeconds(menuAnimationBackgroundDuration);
        }
    }

    private IEnumerator IntroAnimation()
    {
        // handy variables
        float hdur = introDuration * .5f, qdur = introDuration * .25f, hpi = 1.57f;

        pkp.gameObject.SetActive(true);
        // get the "press any key" text and the logo image, and set both to be transparent so we can fade in
        var text = pkp.GetComponentInChildren<Text>();
        // get the image of pkp
        var pki = pkp.GetComponent<Image>();
        text.CrossFadeAlpha(0f, 0f, true);
        pki.CrossFadeAlpha(0f, 0f, true);

        // wait for a bit (smoother looking during testing)
        yield return new WaitForSeconds(qdur);

        /*

        // fade the image in
        pki.CrossFadeAlpha(1f, qdur, false);
        // wait for fade to finish
        yield return new WaitForSeconds(qdur);
        // then fade out and wait
        pki.CrossFadeAlpha(0f, qdur, false);
        yield return new WaitForSeconds(qdur);

        //// change the sprite (the texture basically) of pki to the press any key screen
        pki.sprite = pressAnyKeyScreen;
        // then fade it back in along with the text
        */

        text.CrossFadeAlpha(1f, qdur, false);
        pki.CrossFadeAlpha(1f, qdur, false);

        yield return new WaitForSeconds(qdur);

        sbp.SetActive(true);

        // slowly pulse the text until a kep is pressed
        // first get the sin offset by taking the current time times the flash rate and adding half pi
        var sinOffset = hpi + Time.time * pressAnyKeyFlashRate;
        // then loop until we get an input
        while (!Input.anyKey)
        {
            // fade the text to the absolute sin of the rate-scaled time value with the offset
            text.CrossFadeAlpha(Mathf.Abs(Mathf.Sin((Time.time * pressAnyKeyFlashRate) - sinOffset)), Time.deltaTime, false);
            // don't wait, we want to come back next frame or occasionally you'll miss an input which feels scuffed
            yield return null;
        }

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
        yield return new WaitForSeconds(qdur);

        // deactivate the logo panel and switch to the menu view
        pkp.SetActive(false);
        mbp.SetActive(true);

        // get the canvas group of menu background panel
        CanvasGroup mpc = mbp.GetComponent<CanvasGroup>();

        // then fade it in gradually, could also use crossfade alpha on every child of it here
        for (float t = 0f; t < qdur; t += Time.deltaTime)
        {
            mpc.alpha = Mathf.Sin(hpi * t / qdur);
            yield return null;
        }

        // then enter menu animation
        StartCoroutine(MenuAnimation());
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
        /*
         * Unity seems to not package the directory structure when building for a stanalone exe (with current settings at least).
         * This leads to it not loading assets properly as well as raising questions about how to differentiate different scenarios.
         * So may need to rethink these hardcoded paths and directory parsing, perhaps use JSON representation of hierarchy instead?
         */

        // get a list of all the scenarios in the scenarios folder
        DirectoryInfo dir = new DirectoryInfo(Application.streamingAssetsPath + "/Scenarios");
        scenarios = new List<DirectoryInfo>(dir.GetDirectories());
        // get a list of the buttons on SSP
        var buttons = ssp.transform.GetComponentsInChildren<Button>();
        for (int i = 0; i < Mathf.Min(buttons.Length, scenarios.Count); ++i)
        {
            // start on the 1st button as 0th is back, and get the image
            var img = buttons[i + 1].gameObject.GetComponent<Image>();
            var txt = buttons[i + 1].gameObject.GetComponentInChildren<Text>();
            // make the path by combining the scenarios folder, with the scenario folder in question, with the filename
            // this has to be relative to the resources folder because Resources.Load will only look in there
            // Resources.Load also hates file extensions for some inexplicable reason, so beware
            var path = Application.streamingAssetsPath + "/Scenarios/" + scenarios[i].Name + "/bg.png";

            // actually load it
            var sprite = LoadSprite(path);
            //var sprite = Resources.Load<Sprite>(path);
            
            // and assign
            img.sprite = sprite;
            txt.text = scenarios[i].Name;
        }
        // go through and make the rest transparent because we wanna see the pog bg shader not empty white rectangle sprites
        for (int i = Mathf.Min(buttons.Length, scenarios.Count); i < Mathf.Max(buttons.Length, scenarios.Count) - 1; ++i)
        {
            buttons[i + 1].gameObject.GetComponent<Image>().CrossFadeAlpha(0f, 0f, true);
            buttons[i + 1].interactable = false;
        }
    }

    public void LoadScenario(GameObject btn)
    {
        var index = int.Parse(btn.gameObject.name.Substring(2,1));
        var scName = scenarios[index].Name;
        var vpc = vpp.GetComponent<VideoPlayer>();
        //var clip = Resources.Load<VideoClip>("StreamingAssets/Scenarios/" + scName + "/pre");
        vpc.url = Application.streamingAssetsPath + "/Scenarios/" + scName + "/pre.avi";
        vpp.SetActive(true);
        //vpc.clip = clip;
        //vpc.waitForFirstFrame = true;
        //vpc.skipOnDrop = true;
        //vpc.Stop();
        //vpc.time = 0f;
        //vpc.Play();
        List<Graphic> gfx = new List<Graphic>(vpp.GetComponentsInChildren<Graphic>());
        RawImage ri = vpp.GetComponent<RawImage>();
        for (int i = 0; i < gfx.Count; ++i)
        {
            var g = gfx[i];
            g.CrossFadeAlpha(0f, 0f, true);
            var gb = g.gameObject.GetComponent<Button>();
            if (gb != null)
            {
                gb.interactable = false;
            }
        }
        gfx.Remove(ri);
        sbp.SetActive(false);
        StartCoroutine(AwaitAfterPreAvi(vpc, gfx, ri));
    }

    private IEnumerator AwaitAfterPreAvi(VideoPlayer vpc, List<Graphic> gfx, RawImage ri)
    {
        //vpc.frame = 0;
        //yield return new WaitForSeconds(5f);
        /* 
         * unity video player is kinda bad, seems to always do this really annoying behaviour
         * where it doesn't reset to the first frame before drawing so you get a static image of the last frame of the video for like ~5frames or something, 
         * then a really jarring jump from the last frame to the first when it actually starts playing
         * 
         * After hours of experimentation, the best fix I've found for this so far is just to fade in the image while this jump is occuring so it doesn't look bad lol
         * you can see some of the things I tried commented out
         */
        vpc.Play();
        ri.CrossFadeAlpha(1f, .5f, false);
        yield return new WaitUntil(() => vpc.isPlaying);
        yield return new WaitUntil(() => !vpc.isPlaying);
        vpc.Stop();
        vpc.clip = null;
        foreach (var g in gfx)
        {
            g.CrossFadeAlpha(1f, 1f, false);
            var gb = g.gameObject.GetComponent<Button>();
            if (gb != null)
            {
                gb.interactable = true;
            }
        }
    }

    public void Quit()
    {
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
        mbp = transform.Find("MBP").gameObject;
        pkp = transform.Find("PKP").gameObject;
        ssp = transform.Find("SSP").gameObject;
        gsp = transform.Find("GSP").gameObject;
        bp0 = mbp.transform.Find("BP0").gameObject.GetComponent<Image>();
        bp1 = mbp.transform.Find("BP1").gameObject.GetComponent<Image>();
        // set alpha of background panel 0 to be 0 so that it doesn't bleed through before animation starts
        bp0.CrossFadeAlpha(0f, 0f, true);
        StartCoroutine(IntroAnimation());
    }

    // Update is called once per frame
    //void Update()
    //{
        
    //}
}
