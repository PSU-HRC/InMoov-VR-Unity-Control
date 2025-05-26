# VR-Tutorial-Setup
### VR Controls only supported with Windows OS!!! Mac users can still open and contribute to the Unity project, they just won't be able to test the VR controls. This is due to the Meta Quest Link application not working on Mac. The purpose of this repository is to help you get started in your first VR project. This project is already set up so that all you have to do is install the Unity files and you'll be able to immediately test out the VR interface. However, video references will be linked at the bottom if you want to see the step-by-step making of this project.
We will be using a Meta Quest 2 and Unity to implement VR controls into our club's 3D printed InMoov Humanoid Robot. Unity is an extremely popular game engine which utilizes C# to write scripts for game development, as well as libraries and packages to create the game environement itself. Of these libraries, there are also some that provide support for Virtual Reality capabilities, which is what we will be researching and experimenting with as we learn how to collect data from the Meta Quest, create algorithms to manipulate that data as we need, use APIs to send that data to our Arduino microchips, and eventually have full control of our robot.

#### Installing Unity
1. [Install Unity Hub](https://unity.com/download)
2. Once installed, go to [Unity's Download Archive](https://unity.com/releases/editor/archive) and download version 2022.3.45f1.
3. When prompted, make sure that the Android Build Support checkbox and its children are checked, and continue with the download process. This will take a while.
   
#### If you don't have Git set up on your device (Skip otherwise)
1. [Install Git for Windows](https://git-scm.com/download/win)
   [Install Git for Mac](https://git-scm.com/download/mac)
2. Edit and run these commands in your terminal<br/>
     git config --global user.name "GitHub username here"<br/>
     git config --global user.email "GitHub email here"

#### Clone the Repository
1. In your terminal, navigate to the directory where you want to store the project folder<br/>
     Ex. cd Desktop/HRC
2. Clone the Repo using the HTTPS link, found by pressing the green "Code" button.<br/>
     Ex. git clone https://github.com/XXX
3. Double check you've done everything correctly by running:<br/>
     git remote -v

#### Open the Project in Unity
1. In the UnityHub Add dropdown, choose "Add project from disk".
2. Find your project and select it.
3. Open the project
4. Go to the "Scenes" folder in the bottom Project window and open "VR Test Scene"
      
#### Install the Meta Quest Link app
1. [Download Meta Quest Link] (https://www.meta.com/help/quest/articles/headsets-and-accessories/oculus-rift-s/install-app-for-link/)
2. Create a Meta account
3. Plug in Meta Quest 2 Headset and estbalish the link. Requires action on your laptop and inside the headset.
4. Once link is established, choose the "extend screen" option from the menu in the headset. Choose Unity and run the program.

# Video References
1. [Youtube Playlist of a Very Good Unity VR Setup](https://youtube.com/playlist?list=PLX8u1QKl_yPD4IQhcPlkqxMt35X2COvm0&si=6ncEnU9DhJC6cByr). Check out videos 1-3, and 6
