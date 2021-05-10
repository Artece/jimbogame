# jimbogame
 Sample unity project for super seducer style fan game for jinnytty

Hello everyone!

Just writing this as a quick explanation to the project to help people work on it if you decide to use it; feel free of course to ask me any questions about it, I may not be able to respond immediately but will probably be able to get back to you on it before long.

Not sure how much experience with Unity everybody has, so will try to explain things as I go; I also feel like I should preface that I'm not a Unity super user, I've made some things before and they turned out well, but it's been a while so I'm a bit rusty.



Anyhow, to open the project in Unity:
1. Make sure you've got the correct version (currently the latest version: 2020.3.7f1); I recommend registering an account and then getting a personal use license (free unless you make >$100K from your projects), then installing Unity hub to install the latest version with the default settings. Although if you're a developer on intend to tinker with the code at all then I recommend you make sure the option to install Microsoft Visual Studio Community edition 2019 is ticked (can't remember if it's default), it's also free but you'll need a Microsoft account for it, however I understand the previous project was built in VS so this is probably redundant info.
2. Clone the project from this repo to your pc
3. Go to the project root folder and then go to Assets/Scenes/ and open SampleScene.unity



This should hopefully open the project in Unity and list it on the projects tab of Unity hub; you should then hopefully be presented with a screen resembling this:

![1](https://user-images.githubusercontent.com/25913592/117655896-926d2c00-b18f-11eb-9358-8bd08a4f86f4.PNG)



In the top left, you can see the hierarchy panel, this lists the objects that are in the scene* that is currently loaded.

In the center, you can see the scene itself, which doesn't really look like much in this case, but the white box represents the canvas**. There is also the game tab; when you play the project, this is the container that the scene runs in.

Down the righthand side, you can see the inspector, this will show details of selected objects or assets and allow you to change parameters.

Along the bottom, you can see the project panel, and then in another tab the console.

The project panel will allow you to explore the file structure of the project and do various things from within Unity.

The console displays build errors and debug messages (for devs, Debug.Log() is how you output to this mainly).

TLDR for artists: To import assets of any kind (images, video, audio, etc.) just drag and drop into the project panel, the inspector will let you tweak things further. Also you might wanna hit "File->Save Project", I won't lie in that I don't really know if you need to, but it assuages my paranoia.

*\*scenes are what Unity uses to encapsulate the idea of what is currently loaded and being displayed, think like a level in an old-school game; it's possible to have multiple scenes in a project and switch between them, but currently this project only has one. Any changes to the scene you must do "File->Save" or hit Ctrl-s*

*\*\*the canvas is what Unity's UI system uses to represent the screen of your device, it also functionally acts as a root object for all UI objects in the scene.*

If you have a hunt around in the Assets/ folder, the project should kind of explain itself to an extent:
Our scene SampleScene.unity is in the Scenes/ folder.
**StartupController.cs in the Scripts/ folder is currently where all of the code is.**
Textures contains some really quick placeholder assets I made to more or less emulate what I saw in the WPF project.
VideoPlayer contains some random clips from discord to serve as placeholders, as well as an internal Unity object (VideoTexutre.renderTexture) that Unity uses for playing videos.

Artists, feel free of course to replace any of my scuffed placeholder assets.

If we go to the hierarchy panel and expand some of the dropdowns on the objects inside, then select the Canvas, we should see something like this:

![2](https://user-images.githubusercontent.com/25913592/117659672-26d98d80-b194-11eb-9c51-3d5b4363a044.PNG)

In the inspector, we can see multiple things separated into sections. **These are known as components in Unity; each object can have lots of components attached to it.** At the bottom of this list, we can see a Script component, connected to the file Assets/Scripts/StartupController.cs.

**Just a side note, but almost every object in Unity will have "GameObject" and "Transform" or "Rect Transform" components.**

If you go to the project panel and navigate to Assets/Scripts/ then open StartupController.cs, it should open in VisualStudio like so:

![3](https://user-images.githubusercontent.com/25913592/117661463-51c4e100-b196-11eb-84c2-94484802744c.PNG)

If you scroll down to the bottom of the file, you'll find our point of entry: the function "Start" on line 81. In Unity most if not all scripts derive from the "MonoBehaviour" class, wherein "Start" is always run once when first instantiated, and then "Update" is run every frame that the engine processes/renders.

    // Start is called before the first frame update
    void Start()
    {
        //canvas = GetComponent<Canvas>();
        vppanel = transform.Find("video player panel").gameObject;
        mbgpanel = transform.Find("menu bg panel").gameObject;
        logopanel = transform.Find("logo panel").gameObject;
        StartCoroutine(LogoAnimation(logoDur));
        ParseVids();
        NextVid();
    }

In our start function, you can see first of all that we assign some variables: vppanel, mbgpanel, etc. (apologies for terrible variable names). These represent the video player, menu background, and logo child objects that you can see under the canvas object in the hierarchy. Essentially we're just getting references to them for later.

We then initiate the logo animation, this is implemented as a coroutine currently which I'll go into later. Then we parse the assets folder for video clips (this would be where I imagine the scenario json parsing would be inserted), I then call "NextVid" to pop the queue once to avoid double clicking the button the first time in, try commenting it out and running the game to see what I mean.

By the way, if you make any changes to the code, Unity will automatically compile it and warn you of errors when you switch back to the main Unity window. To play the scene, you just need to hit the play button at the top in the center of the main window of Unity, it should then automatically switch to the game tab allowing interaction. It's pretty basic as you've probably seen from the video:

https://user-images.githubusercontent.com/25913592/117666868-35c43e00-b19c-11eb-8f4c-00fbd8f41bdb.mp4

The first thing we see in the video is the logo animation, which is hardcoded animation that we saw get called as a coroutine in the start function. Coroutines are basically analogous to starting a new thread, or a background process, if you're familiar with those terms. It allows you to initiate something which is going to occur over a larger span of time without blocking the rest of the code. In Unity, you can call StartCoroutine on any function that returns an IEnumerator, in our case this is the "LogoAnimation" function near the top of the file:

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

The menu background panel is the next thing we see, which is much like the logo panel in that it has an Image component (Assets/Textures/background.png) and a canvas group. However, it is inactive by default as it is activated by the animation, and it also has another component to it (the vertical layout group), and some children. The vertical layout group is a convenient component that automatically lays out the children in a vertical configuration; there are various parameters we can adjust to affect the layout and I basically just tried to roughly copy the WPF version.
The children are the four vertically arranged button objects, each having an Image (the button's sprite) and the Button component itself, which we can configure in the inspector.

If we select the topmost button under menu bg panel in the hierarchy and examine it in the inspector by scrolling down to it, we can see that it has two OnClick objects and methods linked: GameObject.SetActive(true) on video player panel, and GameObject.SetActive(false) on menu bg panel. This will, as you may have guessed, activate the until now inactive video player panel, and deactivate its parent the menu bg panel, effectively switching to the game screen. Replacing this with an animation would be as easy as writing the function (probably abstract the logo animation code) to do so and then linking to that instead. The button objects also each have their own child with a Text component, labelling the buttons; these are in child objects because Unity only permits one "graphic" component per object.
