# BASIC MAGIC TILES 3

## How to run the project 
1) Clone the repository
2) Open the project with Unity version 2021.3.45f1 (preferred) or any 2021.3.xxx
3) Open MainScene at Assets/Scenes/MainScene

### On Unity Editor
4) Click Run button

### On an Android device (if Android build support installed)
4) Plug in an android device
5) File -> Build Settings -> Android -> Switch Platform
6) Run Device -> Choose your device
7) Close the window -> File -> Build and Run

## Design Choice
- I stored song data in a seperate files and didn't hard-code in scripts. This would make it easier to tweak the beat map without rebuilding the whole game, it is also easier to switch songs or expand this basic game in a way that it can play many different songs.
- I used Object pooling for tiles and visual elements to enhance performance.
- I used Singleton on some global manager like UIManager or GameManager, centralize the logic and easier to maintain.

## Attribution
- This game does not use any external assets from Unity Asset Store.