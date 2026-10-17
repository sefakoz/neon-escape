# NEON ESCAPE ∞ — Godot 4 Rebuild

Status: PRE-PRODUCTION. The HTML5 version on main is a prototype, NOT a release candidate.

## Non-negotiable vertical slice
- One complete 90-second playable level with a start, win and loss state.
- Responsive touch steering, no speed scaling with level, dead zone, left/right hand options.
- One recognizable hero with authored idle, run, hit, invincible and victory animations.
- Three enemy types with telegraphed attacks and distinct movement.
- Collectible crystals, 7-second joker with visible countdown and enemy destruction, satisfying hit feedback.
- Parallax layered floor, depth, lighting and legible danger indicators.
- Licensed original SFX assets for collect, hit, explosion, joker, portal and UI. Audio settings, mute and volume.
- iOS haptics through native integration; never promise browser vibration on iPhone.
- Localized TR/EN/ES UI, age-appropriate difficulty, pause/resume and restart.
- Performance and touch QA on physical iPhone and Android, target stable 60 FPS.

## Implementation
Godot 4 2D project in this branch; keep legacy HTML/PWA on main for reference. Scene structure: Boot, Menu, Settings, Game, HUD, Pause, Results; reusable Player, Enemy, Pickup, Portal, VFX. Audio buses Master/Music/SFX. Save settings in user://. Separate game logic from visuals.

## Release gates
1. Vertical slice accepted after hands-on playtest.
2. Content production and accessibility review.
3. Crash/performance tests on multiple devices, safe areas and interruption handling.
4. Signed iOS/Android builds, store metadata, age ratings, privacy disclosures and developer-account submission.

Do not label complete until each gate has evidence.