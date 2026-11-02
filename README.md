# Space Invaders 3D — CSCI 381

## How to download and run

**Please clone this repository with Git LFS instead of using GitHub's Download ZIP button.** Images and fonts are stored with Git Large File Storage (LFS). The ZIP download may contain text pointers instead of the actual assets, which prevents Unity from importing them correctly.

### 1. Install the prerequisites

- Git and [Git LFS](https://git-lfs.com/).
- Unity Hub and **Unity Editor 6000.6.0f1**, the version used for this project.

### 2. Clone the project

Run these commands in a terminal after installing Git LFS:

```sh
git lfs install
git clone https://github.com/ye-htut-maung/YeHtut-Maung-Space-Invaders-3D.git
cd YeHtut-Maung-Space-Invaders-3D
git lfs pull
```

Wait for the clone and LFS downloads to finish before opening Unity. If you already downloaded a ZIP, use the newly cloned folder for the following steps.

### 3. Open and play in Unity

1. In Unity Hub, add the cloned project from disk. Select the folder containing `Assets`, `Packages`, and `ProjectSettings`.
2. Open the project with **Unity 6000.6.0f1**.
3. Wait for Unity to download packages, import assets, and compile scripts. The first launch may take several minutes; the `Library` folder is generated automatically.
4. In Unity's Project window, open **`Assets/Scenes/SampleScene.unity`**. Make sure this scene is open before pressing Play.
5. Press **Play** in the Unity toolbar, then click **Start Game** in the Game window.

Use the **left/right arrows** to move and **Space** to shoot. Click inside the Game window if it does not receive keyboard input.

If Unity reports that an image or font cannot be read, close Unity, run `git lfs pull` inside the cloned project folder, and reopen the project.

Original supplied images are in `SourceArt/`. Unity uses the imported copies in `Assets/Art/` and `Assets/Resources/`.

## Required changes

| Requirement | What changed and how |
| --- | --- |
| Speed up surviving aliens | Fleet speed increases from 4 to 10 units/second based on how many aliens have been destroyed. |
| Detect when the game cannot be won | A visible defense line sits above the spaceship. An alien's bottom edge crossing it immediately triggers Game Over. |
| Keep the spaceship onscreen | Movement is clamped to the camera's left/right edges, accounting for the full ship graphic. |

### Code-quality fixes

| Issue | Fix |
| --- | --- |
| Projectile speed depended on the physics timestep | Both bullet types now move using speed in units/second multiplied by `Time.fixedDeltaTime`. |
| Alien counter text was rebuilt every frame | The counter updates only when aliens spawn or die; renamed `score` to `remainingAliens`. |
| Ship could process input after a fatal hit | A death flag and disabled controls immediately prevent further movement or shooting before destruction. |
| Collision handling accepted unrelated objects or repeated hits | Handlers check the target component and use once-only guards. Player/enemy bullets can still intercept each other. |

## Optional requirements implemented

| Requirement | What changed and how |
| --- | --- |
| Starting and ending screens | Added Start, Pause, Game Over, and victory screens. Game state controls movement and time; Restart reloads the scene. Destroying all aliens triggers victory. |
| Lives with a display | The ship has three cyan hearts. Accepted hits remove one life and give 1.25 seconds of flashing protection; zero lives ends the game. |
| Better alien and spaceship graphics | Replaced primitive models with PNG SpriteRenderers and simple matching 3D colliders. Both projectile prefabs also use larger cyan/violet sprites. |
| Collision animations | Added glowing violet alien bursts, cyan ship-hit/explosion effects, and mixed bullet-interception sparks. Particles fade and clean themselves up, including on end screens. |
| Replace the background | Added a starfield sprite behind gameplay. Uniform scaling fills different window shapes without stretching. |
| Balance difficulty | Added a 0.35-second player firing cooldown and tuned fleet speeds. A taller playfield gives more time before aliens reach the defense line. |

## Additional improvements

- Fleet bounds adapt to narrow views so aliens stay visible.
- Rigidbody constraints keep the ship upright after hits.
- A transparent cyan/violet HUD groups smaller hearts and the remaining-alien counter.
