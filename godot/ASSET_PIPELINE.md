# NEON ESCAPE 3D asset pipeline (approved)

## Visual target
Stylized premium sci-fi arena, coherent material language and silhouettes; avoid untextured primitives as release art.

## CC0 sources
- Quaternius Toon Shooter Game Kit: https://quaternius.com/packs/toonshootergamekit.html (74 models, animated characters)
- Quaternius Sci-Fi Essentials Kit: https://quaternius.itch.io/sci-fi-essentials-kit (Standard pack available without mandatory payment)
- Quaternius Universal Animation Library: https://quaternius.itch.io/universal-animation-library
- Kenney modular space kit: https://kenney.nl/assets/modular-space-kit

## Required integration
1. Download and verify actual pack archives and license text.
2. Import glTF/FBX into Godot 4; inspect rigs, materials, animation clips, and triangle counts.
3. Choose **one** dominant art style. Adjust materials consistently rather than mixing packs indiscriminately.
4. Replace current 2D Main scene with 3D CharacterBody3D, camera rig, NavigationAgent3D enemies and collision layers.
5. Implement animation tree (idle/run/hit/death), joystick, spawning, pickups, damage, VFX, SFX.
6. Build web export with Godot and profile iPhone Safari; also test native iOS and Android.
7. Only after successful build copy export to cloudflare/public and deploy via Wrangler.

No third-party model files have been downloaded or imported yet. The old 2D vertical slice and browser Three.js demo are not the final game.
