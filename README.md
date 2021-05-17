# jimbogame
 Sample unity project for super seducer style fan game for jinnytty

Hello everyone!

Just writing this as a quick explanation to the project to help people work on it if you decide to use it; feel free of course to ask me any questions about it, I may not be able to respond immediately but will probably be able to get back to you on it before long.

Not sure how much experience with Unity everybody has, so will try to explain things as I go; I also feel like I should preface that I'm not a Unity super user, I've made some things before and they turned out well, but it's been a while so I'm a bit rusty, also another warning my capitalisation and notation isn't entirely consistent all the way through this doc, I had no idea how exhausting it'd all be to explain erring on the side of caution, so went over multiple times hence some inconsistency.

Here's a brief video demo of the latest build:

https://user-images.githubusercontent.com/25913592/118380715-cd31f280-b5db-11eb-87c7-222f9e7282e7.mp4


Anyhow, to open the project in Unity:
1. Make sure you've got the correct version (currently version: 2020.3.7f1, plan to update to latest soon); I recommend registering an account and then getting a personal use license (free unless you make >$100K from your projects), then installing Unity hub to install the latest version with the default settings. Although if you're a developer on intend to tinker with the code at all then I recommend you make sure the option to install Microsoft Visual Studio Community edition 2019 is ticked (can't remember if it's default), it's also free but you'll need a Microsoft account for it, however I understand the previous project was built in VS so this is probably redundant info.
2. Clone the project from this repo to your pc
3. Go to the project root folder and then go to Assets/Scenes/ and open SampleScene.unity

This should hopefully open the project in Unity and list it on the projects tab of Unity hub; you should then hopefully be presented with a screen resembling this:

![1](https://user-images.githubusercontent.com/25913592/118378940-00ba5000-b5cf-11eb-9932-2c8d34d4a2df.png)

In the top left, you can see the hierarchy panel, this lists the objects that are in the scene* that is currently loaded.

In the center, you can see the scene itself, which doesn't really look like much in this case, but the white box represents the canvas**. There is also the game tab; when you play the project, this is the container that the scene runs in.

Down the righthand side, you can see the inspector, this will show details of selected objects or assets and allow you to change parameters.

Along the bottom, you can see the console panel, and then in another tab the project panel.

The project panel will allow you to explore the file structure of the project and do various things from within Unity.

The console displays build errors and debug messages.

**TLDR for artists:**
To import assets of any kind (images, video, audio, etc.) just drag and drop into the project panel, the inspector will let you tweak things further. Also you might wanna hit "File->Save Project", I won't lie in that I don't really know if you need to, but it assuages my paranoia.

*\*scenes are what Unity uses to encapsulate the idea of what is currently loaded and being displayed, think like a level in an old-school game; it's possible to have multiple scenes in a project and switch between them, but currently this project only has one. Any changes to the scene you must do "File->Save" or hit Ctrl-s*

*\*\*the canvas is what Unity's UI system uses to represent the screen of your device, it also functionally acts as a root object for all UI objects in the scene.*

If you have a hunt around in the Assets/ folder, the project should kind of explain itself to an extent:
Our scene SampleScene.unity is in the Scenes/ folder.
**StartupController.cs in the Scripts/ folder is currently where all of the code is.**
Textures contains some really quick placeholder assets I made to more or less emulate what I saw in the WPF project.
VideoPlayer contains some random clips from discord to serve as placeholders, as well as an internal Unity object (VideoTexutre.renderTexture) that Unity uses for playing videos.

To play the scene, you just need to hit the play button at the top in the center of the main window of Unity, it should then automatically switch to the game tab allowing interaction. It's pretty basic as you've probably seen from the original video:

https://user-images.githubusercontent.com/25913592/117666868-35c43e00-b19c-11eb-8f4c-00fbd8f41bdb.mp4
If we go to the hierarchy panel and expand some of the dropdowns on the objects inside, then select the Canvas, we should see something like this:

![2](https://user-images.githubusercontent.com/25913592/118379046-b6859e80-b5cf-11eb-84a0-010146a45cc5.PNG)

In the inspector, we can see multiple things separated into sections. **These are known as components in Unity; each object can have lots of components attached to it.** At the bottom of this list, we can see a Script component, connected to the file Assets/Scripts/StartupController.cs.

**Just a side note, but almost every object in Unity will have "GameObject" and "Transform" or "Rect Transform" components.**

If you go to the project panel and navigate to Assets/Scripts/ then open StartupController.cs, it should open in VisualStudio.

By the way, if you make any changes to the code, Unity will automatically compile it and warn you of errors when you refocus editor window.

If you scroll down to the bottom of the file, you'll find our point of entry: the function "Start". In Unity most if not all scripts derive from the "MonoBehaviour" class, wherein "Start" is always run once when first instantiated, and then "Update" is run every frame that the engine processes/renders.

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

In our start function, you can see first of all that we assign some variables: sbp, vpp, etc. These are references to the various UI panels we can see in the hierarchy with corresponding names.

These panels and their functions are as follows:

sbp: As in "Shader Background Panel", this is the panel that the grid shader is rendered to, it sits at the top of the hierarchy because the hierarchy doubles as the occulsion order for 2D elements; thus it's at the top so that it's rendered first in order to be benhind the others.

*There's almost no code that references sbp, as all we really ever do with it is turn it on or off, so instead as a quick aside I'll explain the panel structure as an example of what the workflow can look like.*
The panel consists of a game object that is a child of the canvas, it has a rect transform as all UI elements have and canvas renderer (so unity knows to draw it). I created it by right-clicking on the canvas in the hierarchy and selecting "Create->UI->Panel", which sets up a nice preconfigured object for us to use. I then removed the "Image" component and added a "RawImage", as I read that "Image" components are intended for handling sprites more so, whereas "RawImage" is recommended for shaders and video players etc. In the project panel I then when to the shaders folder, and in the same way as before selected "Create", only this time, "Create->Shader->Image Effect Shader". I then opened it in VisualStudio and ported the shader from the WPF version, then created a material to which the shader is applied, which is in turn applied to the "RawImage", as you can see in the image below. This is generally how I tend to work with unity I find, lay down the broad strokes in the editor and then fill in the finer details using code, although there are a lot of editor-based features which are cool and useful I've never specialsed to the degree that I've felt the need to learn them.

![3](https://user-images.githubusercontent.com/25913592/118379751-bd62e000-b5d4-11eb-9b2a-f6b7abe4da0c.PNG)
*\*I figure it's better to have more pictures in the readme, as some people, like our very own Jimbonius, are very visual learners*

vbp: As in "Video Player Panel", this is the panel that the video player is rendered to, it effectively acts as the main "game" screen too for this reason. It has a button to go back to the scenario selcet screen (ssp) whence we entered, and also the four "Game Choice" buttons which will in the end be used by the player to make decisions to progress the game via branching and modifying the score, which will in effect just alter the video clip and button text practically speaking. Just as an aside on buttons, clicking any button will activate the "OnClick" method for that button, which is a feature of Unity's button component that allows you to assign public methods from any other object(s) in the scene that will be called when the button is clicked. The button objects also each have their own child with a Text component, labelling the buttons; these are in child objects because Unity only permits one "graphic" component per object.

![4](https://user-images.githubusercontent.com/25913592/118380592-dd959d80-b5da-11eb-88cb-20925a96f33f.PNG)

ssp: As in "Scenario Select Panel", this is the panel where parsed scenarios are laid out with their thumbnails, titles and descriptions etc. Each scenario thumbnail is a button which enters the scenario by taking us to the video player (vbp) and there is also a back button to return to the menu (mbp).

![5](https://user-images.githubusercontent.com/25913592/118380601-f43bf480-b5da-11eb-8809-b7854700c14f.PNG)

mbp: As in "Menu Background Panel", this is the main menu, it has two background panels: bp0 and bp1, which alternate every 5 seconds or so. It has the game logo and three buttons: Start game, Settings and Quit, which all do what you might expect: Start game takes you to the scenario select screen (ssp), Settings takes you to the settings (gsp) and Quit quits the game entirely.

![6](https://user-images.githubusercontent.com/25913592/118380613-09b11e80-b5db-11eb-82c5-de3390f0f1ee.PNG)

pkp: As in "Press any Key Panel", this is the first thing we see when the game is fully loaded in after the unity splash screens, it used to play host to the logo animation as well but I discovered there was a built-in way to display this as part of the splash screens. It has some text which flashes when we press a key.

![7](https://user-images.githubusercontent.com/25913592/118380628-19306780-b5db-11eb-8579-4bef29afb1e0.PNG)

gsp: As in "Game Settings Panel", this is where the settings will go, it's linked to by the settings button in mbp, and you can return there with the back button.

![8](https://user-images.githubusercontent.com/25913592/118380636-2a797400-b5db-11eb-9de1-6c899273d228.PNG)

If we switch back to Visual Studio, you can see that after getting the panels, the first thing we do is initiate the intro animation, which is implemented as a coroutine. Coroutines are basically analogous to starting a new thread, or a background process, if you're familiar with those terms. It allows you to initiate something which is going to occur over a larger span of time (much longer than a single frame for animations) without blocking the rest of the code. In Unity, you can call StartCoroutine on any function that returns an IEnumerator, in our case this is the "IntroAnimation" function near the top of the file:

    private IEnumerator IntroAnimation()
    {
        float qdur = introDuration * .25f;
        
Essentially we set pkp to be active, then set it and it's child text to be transparent so we can fade it in, we wait for a bit then start the fade. Once the fade in is done, we activate sbp underneath pkp, so when we fade out pkp the shader is running. This is beacuse the reason it's not running in the first place is it looks a bit weird to fade in to pkp when you can already see the shader:

        // get the "press any key" text and the pkp image, and set both to be forsenCD so we can fade in
        pkp.gameObject.SetActive(true);
        var text = pkp.GetComponentInChildren<Text>();
        var pki = pkp.GetComponent<Image>();
        text.CrossFadeAlpha(0f, 0f, true);
        pki.CrossFadeAlpha(0f, 0f, true);

Now, in Unity when using Coroutines, using the "yield" and "return" keywords will make the function exit where it was, allowing Unity to continue on with the rest of whatever it has to do, only to resume where we left off at some later point. In this case you can see that we return an instance of the "WaitForSeconds" class, which tells Unity to not resume our code until that many Seconds have elapsed; however, there are other options such as waiting until a condition is true and so on:

        // wait for a bit (smoother looking during testing)
        yield return new WaitForSeconds(qdur);

        text.CrossFadeAlpha(1f, qdur, false);
        pki.CrossFadeAlpha(1f, qdur, false);

        yield return new WaitForSeconds(qdur);

        sbp.SetActive(true);
        
We then enter a loop where we slowly pulse the text in and out until the player hits a key, whereupon we flash the text three times rapily:

        // slowly pulse the text until a kep is pressed
        // first get the sin offset by taking the current time times the flash rate and adding half pi
        var sinOffset = 1.57f + Time.time * pressAnyKeyFlashRate;
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
        
Before fading out pkp:
        
        text.CrossFadeAlpha(0f, 0.05f, false);
        yield return new WaitForSeconds(0.05f);

        // then fade out press any key screen
        pki.CrossFadeAlpha(0f, qdur, false);

        // set mbp to active
        mbp.SetActive(true);
        
Then fading in mbp by making it transparent, waiting then starting the fade in:

        var gfx = mbp.GetComponentsInChildren<Graphic>();
        foreach (Graphic g in gfx)
        {
            g.CrossFadeAlpha(0f, 0f, true);
        }
        yield return new WaitForSeconds(qdur);

        // deactivate pkp
        pkp.SetActive(false);
        
        bp0 = mbp.transform.Find("BP0").gameObject.GetComponent<Image>();
        bp1 = mbp.transform.Find("BP1").gameObject.GetComponent<Image>();

        foreach (Graphic g in gfx)
        {
            g.CrossFadeAlpha(1f, 1f, false);
        }
Except BP0, which we want to keep transparent as we only want to show one menu background image to start with:

        bp0.CrossFadeAlpha(0f, 0f, true);
        
We then fork of the two coroutines that make up the menu animation:

        // then enter menu animation
        IEnumerator clock;
        StartCoroutine(clock = maClock());
        StartCoroutine(maCatch(clock, true));
    }

It might seem like a bit much to implement a background swapper as two parallel coroutines, but it's just what's convenient given the parameters of the problem. Essentially, we want to fade between the two backgrounds, and it'd be nice if we could easily regulate the timing and behaviour:

So we have one coroutine which very simply toggles the state of a global boolean maTick every time menuAnimationBackgroundDuration elapses, then fades between the panels:

    private IEnumerator maClock()
    {
        while (true)
        {

We wait for the time to fade:

            yield return new WaitForSeconds(menuAnimationBackgroundDuration);

Then toggle the state and fade accordingly:

            maTick = !maTick;
            var bp0a = (maTick) ? 1f : 0f;
            var bp1a = (maTick) ? 0f : 1f;
            bp0.CrossFadeAlpha(bp0a, menuAnimationTransitionDuration, false);
            bp1.CrossFadeAlpha(bp1a, menuAnimationTransitionDuration, false);
        }
    }

We also have a coroutine which catches when we navigate away from the menu screen in order to regulate resulting irregularites:

    private bool maTick = false;

    private IEnumerator maCatch(IEnumerator clock, bool resetClock)
    {
        // this function only exists to prevent weird behaviour when transitioning back to the menu page
        while (true)
        {
            // basically we detect if the menu page went inactive then if we were due to show the alternative background we set it
            yield return new WaitWhile(() => mbp.activeSelf);
            
When mbp becomes inactive we restore the transparency to what it should be, then if we care about the clock we wait until mbp is active again to restart it:

            var bp0a = (maTick) ? 1f : 0f;
            var bp1a = (maTick) ? 0f : 1f;
            bp0.CrossFadeAlpha(bp0a, 0f, true);
            bp1.CrossFadeAlpha(bp1a, 0f, true);
            if (resetClock) StopCoroutine(clock);
            yield return new WaitUntil(() => mbp.activeSelf);
            if (resetClock) StartCoroutine(clock = maClock());
        }
    }

The following function is used to load sprites from files. This is because in order to preserve the directory structure of the scenarios folder, we need to put it in a special purrpose folder called "StreamingAssets". This unfortunately means we can't put in another special purpose folder called "Resources", which would allow us to use Resources.Load, so we have to write our own.

    private Sprite LoadSprite(string path)
    {
        byte[] d = File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(0, 0);
        tex.LoadImage(d);
        var sprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2());
        return sprite;
    }
    
To parse the scenarios, we get a list of subfolders in the scenarios directory, then load bg.png as a sprite to use in ssp.
    
    // not implemented
    public void ParseScenarios()
    {
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

This function is called by whichever scenario button we click on in ssp, we determine which we clicked then begin to load that scenario. We switch to vpp and load pre.avi.

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
    
We then wait until pre avi has finished to fade in vpp's controls:

    private IEnumerator AwaitAfterPreAvi(VideoPlayer vpc, List<Graphic> gfx, RawImage ri)
    {
        vpc.Play();
        ri.CrossFadeAlpha(1f, videoFadeInDuration, false);
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


I think this about covers it, any questions just ping me or dm me (EZ Clap).
