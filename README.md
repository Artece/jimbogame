# jimbogame
 Sample unity project for super seducer style fan game for jinnytty

Hello everyone!

Just writing this as a quick explanation to the project to help people work on it if you decide to use it; feel free of course to ask me any questions about it, I may not be able to respond immediately but will probably be able to get back to you on it before long.

Not sure how much experience with unity everybody has, so will try to explain things as I go; I also feel like I should preface that I'm not a unity super user, I've made some things before and they turned out well, but it's been a while so I'm a bit rusty.



Anyhow, to open the project in unity:
1. Make sure you've got the correct version (currently the latest version: 2020.3.7f1); I recommend registering an account and then getting a personal use license (free unless you make >$100K from your projects), then installing unity hub to install the latest version with the default settings. Although if you're a developer on intend to tinker with the code at all then I recommend you make sure the option to install Microsoft Visual Studio Community edition 2019 is ticked (can't remember if it's default), it's also free but you'll need a Microsoft account for it, however I understand the previous project was built in VS so this is probably redundant info.
2. Clone the project from this repo to your pc
3. Go to the project root folder and then go to Assets/Scenes/ and open SampleScene.unity



This should hopefully open the project in unity and list it on the projects tab of unity hub; you should then hopefully be presented with a screen resembling this:

![1](https://user-images.githubusercontent.com/25913592/117655896-926d2c00-b18f-11eb-9358-8bd08a4f86f4.PNG)



In the top left, you can see the hierarchy panel, this lists the objects that are in the scene* that is currently loaded.

In the center, you can see the scene itself, which doesn't really look like much in this case, but the white box represents the canvas**.

Down the righthand side, you can see the inspector, this will show details of selected objects or assets and allow you to change parameters.

Along the bottom, you can see the project panel, and then in another tab the console.

The project panel will allow you to explore the file structure of the project and do various things from within unity.

The console displays build errors and debug messages (for devs, Debug.Log() is how you output to this mainly).

*\*scenes are what unity uses to encapsulate the idea of what is currently loaded and being displayed, think like a level in an old-school game; it's possible to have multiple scenes in a project and switch between them, but currently this project only has one.*

*\*\*the canvas is what unity's UI system uses to represent the screen of your device, it also functionally acts as a root object for all UI objects in the scene.*

If you have a hunt around in the Assets/ folder, the project should kind of explain itself to an extent:
Our scene SampleScene.unity is in the Scenes/ folder.
**StartupController.cs in the Scripts/ folder is currently where all of the code is.**
Textures contains some really quick placeholder assets I made to more or less emulate what I saw in the WMP project.
VideoPlayer contains some random clips from discord to serve as placeholders, as well as an internal unity object (VideoTexutre.renderTexture) that unity uses for playing videos.

Artists, feel free of course to replace any of the placeholder assets.

If we go to the hierarchy panel and expand some of the dropdowns on the objects inside, then select the Canvas, we should see something like this:

![2](https://user-images.githubusercontent.com/25913592/117659672-26d98d80-b194-11eb-9c51-3d5b4363a044.PNG)

In the inspector, we can see multiple things separated into sections. **These are known as components in unity; each object can have lots of components attached to it.** At the bottom of this list, we can see a Script component, connected to the file Assets/Scripts/StartupController.cs.

If you go to the project panel and navigate to Assets/Scripts/ then open StartupController.cs, it should open in VisualStudio like so:

![3](https://user-images.githubusercontent.com/25913592/117661463-51c4e100-b196-11eb-84c2-94484802744c.PNG)

