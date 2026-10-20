using UnityEngine;

/// <summary>Runtime HUD minimap with exploration fog. Attach automatically via NeonEscapeGame.</summary>
public class NeonMiniMap : MonoBehaviour
{
    public NeonMazeWorld maze;
    public Transform target;
    public float panelSize = 190f;
    public int revealRadius = 2;
    bool[,] discovered;
    Texture2D pixel;
    GUIStyle titleStyle;

    void Awake()
    {
        pixel = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        pixel.SetPixel(0, 0, Color.white);
        pixel.Apply();
    }

    void Update()
    {
        if (!maze || !target) return;
        if (discovered == null || discovered.GetLength(0) != maze.width || discovered.GetLength(1) != maze.height)
            discovered = new bool[maze.width, maze.height];
        var cell = maze.WorldToCell(target.position);
        for (int dx = -revealRadius; dx <= revealRadius; dx++)
        for (int dy = -revealRadius; dy <= revealRadius; dy++)
        {
            int x = cell.x + dx, y = cell.y + dy;
            if (x >= 0 && y >= 0 && x < maze.width && y < maze.height && dx*dx + dy*dy <= revealRadius*revealRadius)
                discovered[x,y] = true;
        }
    }

    void Draw(Rect rect, Color color)
    {
        var previous = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, pixel);
        GUI.color = previous;
    }

    void OnGUI()
    {
        if (!maze || !target || discovered == null) return;
        if (titleStyle == null)
        {
            titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 12;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = new Color(.45f, .95f, 1f);
        }
        float size = Mathf.Min(panelSize, Screen.width * .34f, Screen.height * .34f);
        var panel = new Rect(Screen.width - size - 18, Screen.height - size - 28, size, size + 10);
        Draw(panel, new Color(.015f, .025f, .06f, .88f));
        GUI.Label(new Rect(panel.x + 10, panel.y + 3, size - 20, 20), "NEON // MAP", titleStyle);
        float margin = 12f;
        float area = size - margin * 2 - 17;
        float tile = area / Mathf.Max(maze.width, maze.height);
        float ox = panel.x + margin + (area - maze.width * tile) * .5f;
        float oy = panel.y + 21 + (area - maze.height * tile) * .5f;
        for (int x = 0; x < maze.width; x++)
        for (int y = 0; y < maze.height; y++)
        {
            if (!discovered[x,y]) continue;
            var rect = new Rect(ox + x * tile, oy + (maze.height - y - 1) * tile,
                                Mathf.Max(1,tile - 1), Mathf.Max(1,tile - 1));
            Draw(rect, maze.IsCorridor(x,y) ? new Color(.05f,.55f,.78f,.95f) : new Color(.12f,.16f,.29f,.85f));
        }
        var player = maze.WorldToCell(target.position);
        Draw(new Rect(ox + player.x * tile - 1, oy + (maze.height - player.y - 1) * tile - 1,
                      Mathf.Max(4,tile+2), Mathf.Max(4,tile+2)), Color.white);
    }

    void OnDestroy()
    {
        if (pixel) Destroy(pixel);
    }
}
