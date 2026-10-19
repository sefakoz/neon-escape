using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Self-contained 3D gameplay vertical slice. Attach to an empty GameObject in a new 3D URP scene.
/// Procedural geometry is strictly a placeholder until licensed models and animation rigs are imported.</summary>
public class NeonEscapeGame : MonoBehaviour
{
    public float roundSeconds = 90f;
    public float playerSpeed = 7f;
    public int startingHearts = 3;
    public Text statusText;
    public GameObject heroPrefab;
    public GameObject enemyPrefab;
    public GameObject crystalPrefab;
    public GameObject powerupPrefab;

    NeonMazeWorld maze;
    Transform hero;
    Camera gameCamera;
    readonly List<Transform> enemies = new List<Transform>();
    readonly List<Pickup> pickups = new List<Pickup>();
    float remaining, spawnTimer, pickupTimer, invulnerable, powered;
    int score, hearts;
    bool playing;
    Vector2 stickOrigin, stickVector;
    int touchFinger = -1;

    class Pickup { public Transform item; public bool isPower; }

    void Start()
    {
        gameCamera = Camera.main;
        if (!gameCamera)
        {
            var cameraObject = new GameObject("Follow Camera");
            gameCamera = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
        }
        maze = GetComponent<NeonMazeWorld>();
        if (!maze) maze = gameObject.AddComponent<NeonMazeWorld>();
        var light = new GameObject("Sun").AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.8f;
        light.transform.rotation = Quaternion.Euler(55, -30, 0);
        NewRound();
    }

    public void NewRound()
    {
        foreach (var enemy in enemies) if (enemy) Destroy(enemy.gameObject);
        foreach (var pickup in pickups) if (pickup.item) Destroy(pickup.item.gameObject);
        enemies.Clear(); pickups.Clear();
        if (hero) Destroy(hero.gameObject);
        hero = CreateObject(heroPrefab, PrimitiveType.Capsule, "Hero", Color.cyan, maze ? maze.StartPosition : new Vector3(0, 1, 0));
        remaining = roundSeconds; hearts = startingHearts; score = 0;
        spawnTimer = 0; pickupTimer = 0; invulnerable = 2; powered = 0;
        touchFinger = -1; stickVector = Vector2.zero; playing = true;
    }

    Transform CreateObject(GameObject prefab, PrimitiveType fallback, string objectName, Color color, Vector3 position)
    {
        var obj = prefab ? Instantiate(prefab) : GameObject.CreatePrimitive(fallback);
        obj.name = objectName; obj.transform.position = position;
        if (!prefab) obj.GetComponent<Renderer>().material.color = color;
        return obj.transform;
    }

    void Update()
    {
        if (!playing) return;
        float dt = Time.deltaTime;
        remaining -= dt;
        invulnerable = Mathf.Max(0, invulnerable - dt);
        powered = Mathf.Max(0, powered - dt);
        ReadTouch();
        Vector2 keyboard = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Vector2 movement = (stickVector + keyboard).normalized;
        Vector3 motion = new Vector3(movement.x, 0, movement.y);
        Vector3 step = motion * playerSpeed * dt;
        Vector3 nextX = hero.position + new Vector3(step.x, 0, 0);
        if (!maze || maze.CanWalk(nextX)) hero.position = nextX;
        Vector3 nextZ = hero.position + new Vector3(0, 0, step.z);
        if (!maze || maze.CanWalk(nextZ)) hero.position = nextZ;
        if (motion.sqrMagnitude > .01f) hero.rotation = Quaternion.Slerp(hero.rotation, Quaternion.LookRotation(motion), dt * 12);
        Vector3 cameraTarget = hero.position + new Vector3(0, 20, -16);
        gameCamera.transform.position = Vector3.Lerp(gameCamera.transform.position, cameraTarget, dt * 6);
        gameCamera.transform.LookAt(hero.position);

        spawnTimer -= dt; pickupTimer -= dt;
        if (spawnTimer <= 0 && enemies.Count < 24)
        {
            SpawnEnemy(); spawnTimer = Mathf.Max(.65f, 2.3f - (roundSeconds - remaining) * .012f);
        }
        if (pickupTimer <= 0) { SpawnPickup(); pickupTimer = 3.2f; }
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            var e = enemies[i];
            if (!e) { enemies.RemoveAt(i); continue; }
            Vector3 delta = hero.position - e.position; delta.y = 0;
            Vector3 enemyStep = delta.normalized * (2.4f + (roundSeconds - remaining) * .015f) * dt;
            if (!maze || maze.CanWalk(e.position + enemyStep)) e.position += enemyStep;
            if (delta.sqrMagnitude < 2.25f)
            {
                if (powered > 0)
                {
                    score += 15; Destroy(e.gameObject); enemies.RemoveAt(i);
                }
                else if (invulnerable <= 0)
                {
                    hearts--; invulnerable = 2;
                    if (hearts <= 0) { EndRound(); break; }
                }
            }
        }
        for (int i = pickups.Count - 1; i >= 0; i--)
        {
            var p = pickups[i]; if (!p.item) { pickups.RemoveAt(i); continue; }
            p.item.Rotate(0, 100 * dt, 0);
            if ((hero.position - p.item.position).sqrMagnitude < 2)
            {
                if (p.isPower) powered = 7;
                score += p.isPower ? 5 : 12;
                Destroy(p.item.gameObject); pickups.RemoveAt(i);
            }
        }
        if (statusText) statusText.text = "♥ " + hearts + "    ◆ " + score + "    " + Mathf.CeilToInt(Mathf.Max(0, remaining)) + "s" + (powered > 0 ? "    POWER" : "");
        if (maze && maze.ReachedGoal(hero.position)) { score += 250; EndRound(); }
        if (remaining <= 0) EndRound();
    }

    void ReadTouch()
    {
        // Floating relative joystick. Touch never teleports the player.
        if (Input.touchCount == 0) { touchFinger = -1; stickVector = Vector2.zero; return; }
        foreach (var touch in Input.touches)
        {
            if (touchFinger == -1 && touch.phase == TouchPhase.Began)
            {
                touchFinger = touch.fingerId; stickOrigin = touch.position;
            }
            if (touch.fingerId != touchFinger) continue;
            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                touchFinger = -1; stickVector = Vector2.zero;
            }
            else
            {
                Vector2 offset = touch.position - stickOrigin;
                stickVector = Vector2.ClampMagnitude(offset / 65f, 1);
            }
        }
    }

    void SpawnEnemy()
    {
        float a = Random.Range(0, Mathf.PI * 2);
        Vector3 position = maze ? maze.RandomFloorPosition() : hero.position + new Vector3(Mathf.Cos(a) * 18, 0, Mathf.Sin(a) * 18);
        position.y = 1;
        enemies.Add(CreateObject(enemyPrefab, PrimitiveType.Cube, "Enemy", new Color(1, .2f, .4f), position));
    }

    void SpawnPickup()
    {
        bool power = Random.value < .22f;
        Vector3 pos = maze ? maze.RandomFloorPosition() : new Vector3(Random.Range(-19f, 19f), 1, Random.Range(-19f, 19f));
        var item = CreateObject(power ? powerupPrefab : crystalPrefab, PrimitiveType.Sphere, power ? "Powerup" : "Crystal", power ? Color.green : Color.yellow, pos);
        item.localScale = Vector3.one * .65f;
        pickups.Add(new Pickup { item = item, isPower = power });
    }

    void EndRound()
    {
        playing = false;
        if (statusText) statusText.text = (hearts > 0 ? "SURVIVED" : "GAME OVER") + " · SCORE " + score + " · Tap restart";
    }
}
