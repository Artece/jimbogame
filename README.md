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

*\*scenes are what Unity uses to encapsulate the idea of what is currently loaded and being displayed, think like a level in an old-school game; it's possible to have multiple scenes in a project and switch between them, but currently this project only has one.*

*\*\*the canvas is what Unity's UI system uses to represent the screen of your device, it also functionally acts as a root object for all UI objects in the scene.*

If you have a hunt around in the Assets/ folder, the project should kind of explain itself to an extent:
Our scene SampleScene.unity is in the Scenes/ folder.
**StartupController.cs in the Scripts/ folder is currently where all of the code is.**
Textures contains some really quick placeholder assets I made to more or less emulate what I saw in the WPF project.
VideoPlayer contains some random clips from discord to serve as placeholders, as well as an internal Unity object (VideoTexutre.renderTexture) that Unity uses for playing videos.

Artists, feel free of course to replace any of the placeholder assets.

If we go to the hierarchy panel and expand some of the dropdowns on the objects inside, then select the Canvas, we should see something like this:

![2](https://user-images.githubusercontent.com/25913592/117659672-26d98d80-b194-11eb-9c51-3d5b4363a044.PNG)

In the inspector, we can see multiple things separated into sections. **These are known as components in Unity; each object can have lots of components attached to it.** At the bottom of this list, we can see a Script component, connected to the file Assets/Scripts/StartupController.cs.

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

Clicking any button will activate the "OnClick" method for that button, which in the top button's case will deactivate the menu background panel and activate the videoplayer panel.

This panel also has buttons: a back button to return to the menu screen, and four buttons emulating the game controls / the choice menu. At present, only the top left of these buttons does anything, which is just call the "Nextvid" function, rotating which video is playing.



