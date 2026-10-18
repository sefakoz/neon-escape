# Cloudflare deployment scaffold

This is a **deployment scaffold**, not a deployed game.

Cloudflare Workers Static Assets hosts a **built web export** of the Godot game.
Cloudflare does not render or compile 3D assets; Godot does.

1. Build and test the Godot 3D project locally or via a verified CI build.
2. Copy the **complete Godot web export** (including WASM/PCK/HTML/JS) into `cloudflare/public/`.
3. From the `cloudflare/` folder run `npx wrangler deploy` using an authorized Cloudflare session.
4. Test the published URL on iPhone Safari, Android Chrome, and desktop.
5. Add D1 for authenticated leaderboard only after anti-cheat and user identity are implemented.

Do **not** deploy the old `play-3d.html` and call it the final game.
Do **not** commit API tokens or account credentials.
Cloudflare deployment has not been performed from this workspace.
Documentation: https://developers.cloudflare.com/workers/static-assets/
