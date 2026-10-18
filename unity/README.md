# NEON ESCAPE ∞ — Unity 6 rebuild

This is the **Unity source starting point**, not a finished game, not an exported iOS/Android app, and not a professional asset integration.

## Setup
1. Install Unity Hub: https://unity.com/download
2. Install Unity 6 LTS with **Universal Render Pipeline (URP)**, Android Build Support (SDK/NDK/OpenJDK), and iOS Build Support when on macOS.
3. Create a **3D URP** Unity project locally.
4. Copy the `Assets/Scripts/NeonEscapeGame.cs` file into the project's `Assets/Scripts` folder.
5. Create a scene, add an empty GameObject named `GameController`, attach `NeonEscapeGame`.
6. Optional: add a Canvas + Text UI and assign the statusText field.
7. Press Play. WASD/arrows on desktop, touch-relative movement on mobile builds.

## Replace placeholders before release
- Import licensed 3D animated characters and environment assets from Quaternius/Kenney or Unity Asset Store.
- Assign heroPrefab, enemyPrefab, crystalPrefab, powerupPrefab in Inspector.
- Add animator controllers, distinct enemies, 3D VFX/SFX, proper floating joystick visuals, accessibility and polished UI.
- Add save data, menus, analytics, crash reporting, privacy/consent, rewarded ads with optional user opt-in and a verified purchase flow.
- Test in Unity Editor, then physical iPhone/Android devices. **No device build has been tested yet.**
- Cloudflare remains a separate optional backend for score validation and profiles, not the 3D renderer.

The Unity Editor project and generated binaries must be built using an actual Unity installation. Do not claim this repository contains a production-ready game.
