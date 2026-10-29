# Space Invaders 3D — CSCI 381

Open `Assets/Scenes/SampleScene.unity`, enter Play mode, and click **Start Game**. Use the **left/right arrows** to move and **Space** to shoot.

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
