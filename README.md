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


vbp: As in "Video Player Panel", this is the panel that the video player is rendered to, it effectively acts as the main "game" screen too for this reason. It has a button to go back to the scenario selcet screen (ssp) whence we entered, and also the four "Game Choice" buttons which will in the end be used by the player to make decisions to progress the game via branching and modifying the score, which will in effect just alter the video clip and button text practically speaking.

![4](https://user-images.githubusercontent.com/25913592/118380592-dd959d80-b5da-11eb-88cb-20925a96f33f.PNG)

ssp: As in "Scenario Select Panel", this is the panel where parsed scenarios are laid out with their thumbnails, titles and descriptions etc. Each scenario thumbnail is a button which enters the scenario by taking us to the video player (vbp) and there is also a back button to return to the menu (mbp).

![5](https://user-images.githubusercontent.com/25913592/118380601-f43bf480-b5da-11eb-8809-b7854700c14f.PNG)

mbp: As in "Menu Background Panel", this is the main menu, it has two background panels: bp0 and bp1, which alternate every 5 seconds or so. It has the game logo and three buttons: Start game, Settings and Quit, which all do what you might expect: Start game takes you to the scenario select screen (ssp), Settings takes you to the settings (gsp) and Quit quits the game entirely.

![6](https://user-images.githubusercontent.com/25913592/118380613-09b11e80-b5db-11eb-82c5-de3390f0f1ee.PNG)

pkp: As in "Press any Key Panel", this is the first thing we see when the game is fully loaded in after the unity splash screens, it used to play host to the logo animation as well but I discovered there was a built-in way to display this as part of the splash screens. It has some text which flashes when we press a key.

![7](https://user-images.githubusercontent.com/25913592/118380628-19306780-b5db-11eb-8579-4bef29afb1e0.PNG)

gsp: As in "Game Settings Panel", this is where the settings will go, it's linked to by the settings button in mbp, and you can return there with the back button.

![8](https://user-images.githubusercontent.com/25913592/118380636-2a797400-b5db-11eb-9de1-6c899273d228.PNG)

----------------------------------------------------------------------README FROM HERE DOWN NOT UP TO DATE----------------------------------------------------------------------

If we switch back to Visual Studio, you can see that after getting the panels, the first thing we do is initiate the intro animation, which is implemented as a coroutine. Coroutines are basically analogous to starting a new thread, or a background process, if you're familiar with those terms. It allows you to initiate something which is going to occur over a larger span of time (much longer than a single frame for animations) without blocking the rest of the code. In Unity, you can call StartCoroutine on any function that returns an IEnumerator, in our case this is the "IntroAnimation" function near the top of the file:

    private IEnumerator LogoAnimation(float dur)
    {
        yield return new WaitForSeconds(dur/4);

        CanvasGroup lpc = logopanel.GetComponent<CanvasGroup>();
        float t = 0f;

        while (t < dur)
        {
            t += Time.deltaTime;
            lpc.alpha = Mathf.Sin(3.14f * t / dur);
            yield return null;
        }
        
        logopanel.SetActive(false);

        mbgpanel.SetActive(true);
        CanvasGroup mpc = mbgpanel.GetComponent<CanvasGroup>();
        
        t = 0f;
        dur /= 2;
        while (t < dur)
        {
            t += Time.deltaTime;
            mpc.alpha = Mathf.Sin(1.57f * t / dur);
            yield return null;
        }
    }

Now, in Unity when using Coroutines, using the "yield" and "return" keywords will make the function exit where it was, allowing Unity to continue on with the rest of whatever it has to do, only to resume where we left off at some later point. In this case you can see that we return an instance of the "WaitForSeconds" class, which tells Unity to not resume our code until that many Seconds have elapsed, however there are other options such as waiting until a condition is true etc.

We then grab a reference to the "CanvasGroup" component attached to the logo panel object, this is a Unity UI component that allows child objects to inherit their parents UI properties, which is useful for altering entire of groups of UI objects at once (as we're about to). We then initialise t to 0, representing the passage of time or the progress of the animation or however you want to think of it.

We then enter a loop, modifiying the transparency of the logo panel and yielding to allow the game to continue, by returning null we resume the next frame. This has the effect of gradually fading in the logo and then fading it out again, as we use the Sin of t * Pi / the animation duration i.e. 0 -> 1 -> 0.

Once this first loop is complete, we deactivate the logo panel as we no longer need it and so don't need to process it, then activate the menu background panel and grab a reference to its CanvasGroup.

We then repeat the same process only half way this time, hence diving the duration by two and using half of Pi, allowing the menu background panel to fade in nicely and then stay there.

The menu panel currently has four buttons to emulate the look of the WPF project, but only the top and bottom buttons have any function currently; the top initiates the "game" and the bottom will quit.

Clicking any button will activate the "OnClick" method for that button, which is a feature of Unity's button component that allows you to assign public methods from any other object(s) in the scene that will be called when the button is clicked. In the top button's case, this will deactivate the menu background panel and activate the videoplayer panel. I didn't bother animating this as I felt that'd been shown already so it's just a hard transition right now by swapping which panels are active.

This panel also has buttons: a back button to return to the menu screen, and four buttons emulating the game controls / the choice menu. At present, only the top left of these buttons does anything, which is just call the "Nextvid" function, rotating which video is playing.

The functions "ParseVids" and "NextVid" shouldn't be too hard to figure out but I'll just briefly go through them:

    private void ParseVids()
    {
        DirectoryInfo dir = new DirectoryInfo("./Assets/VideoPlayer");
        var fia = dir.GetFiles("*.m4v");
        videoList = new Queue<FileInfo>();
        foreach (FileInfo fi in fia)
        {
            videoList.Enqueue(fi);
        }
    }

    public void NextVid()
    {
        VideoPlayer vp = vppanel.GetComponent<VideoPlayer>();
        var fi = videoList.Dequeue();
        videoList.Enqueue(fi);
        vp.url = fi.FullName;
    }

All that's basically happening is that in ParseVids we look inside the Assets/VideoPlayer/ folder and get a list of all files with an extension of ".m4v", then we store them all in a queue for later use. NextVid essentially just pops the clip we want off the queue and then requeues it in order to cycle, setting the video player's source url to be the path of the clip we just cycled to.

That's pretty much all the code right now except Quit, which just either quits or stops the editor playing, so I'll just go through some of the things in Unity itself to flesh out my explanation.

So, these panels I've been talking about are basically just objects that are children of the canvas object, they have various UI components like "Image", "RawImage" or "Button" attached to them to help them function. There are three right now, and to be honest we may not ever need more than that, it just depends what paradigm people prefer. I'm sure you may already have a pretty good idea of what they each do (assuming my readme has been of any use), but I'll just briefly run through and explain anyway to clarify any doubts that may still be lingering:

The logo panel is by far the simplest, it's also the only panel that is active at when starting the game, although invisible due to it's alpha being set to 0 in anticipation of the animation. It is literally just a basic UI object, with an Image component that links to Assets/Textures/studio logo.png, and a canvas group (even though it has no children, just easier to modify alpha this way).

The menu background panel is the next thing we see, which is much like the logo panel in that it has an Image component (Assets/Textures/background.png) and a canvas group. However, it is inactive by default as it is activated by the animation, and it also has another component to it (the vertical layout group), and some children. The Vertical Layout Group is a convenient component that automatically lays out the children in a vertical configuration; there are various parameters we can adjust to affect the layout and I basically just tried to roughly copy the WPF version.
The children are the four vertically arranged button objects, each having an Image (the button's sprite) and the Button component itself, which we can configure in the inspector.

If we select the topmost button under menu bg panel in the hierarchy and examine it in the inspector by scrolling down to it, we can see that it has two OnClick objects and methods linked: GameObject.SetActive(true) on video player panel, and GameObject.SetActive(false) on menu bg panel. This will, as you may have guessed, activate the until now inactive video player panel, and deactivate its parent the menu bg panel, effectively switching to the game screen. Replacing this with an animation would be as easy as writing the function (probably abstract the logo animation code) to do so and then linking to that instead. The button objects also each have their own child with a Text component, labelling the buttons; these are in child objects because Unity only permits one "graphic" component per object.

The video player panel is the most complex of the UI elements currently, although still not very much so. It is similar to the menu background panel, but has a few key differences, namely that it has a RawImage component instead of an Image component, as it needs to display a RenderTexture not just a sprite, and that it has a Grid Layout Group component instead of the vertical one, though they function in much the same way. The biggest difference however, is of course that it contains the Video Player component. 

The Video Player is fairly simple, it takes a clip or a URL, and then renders it to the RenderTexture that is connected to the RawImage; it also has all the standard controls you might expect, which are accessible through public methods.

The video player panel also has button children, although the back button (the topmost child), has a LayoutElement component which is used to exempt it from the grid layout scheme. The back button takes you back to the menu, again by just switching which objects are active. The only other button that does anything is the top left button in the grid, which calls NextVid, thereby cycling which video plays.

I think this about covers it, any questions just ping me or dm me.

Functionally, I think the only changes that should be necessary for the game code coming from the WPF project would be to ensure there are public methods which the buttons can hook for game choices to update the game state, and also methods calling outward to control which video clip is playing and whether it's paused based on the game logic.
