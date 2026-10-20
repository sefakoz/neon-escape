using System.Collections.Generic;
using UnityEngine;

/// <summary>Procedural connected neon maze. Add to the same object as NeonEscapeGame.
/// Builds a deterministic grid of corridors, walls, glowing trim, and a goal portal.</summary>
public class NeonMazeWorld : MonoBehaviour
{
    public int width = 13;
    public int height = 13;
    public int seed = 747;
    public float cellSize = 6f;
    public float wallHeight = 4.5f;

    bool[,] open;
    Transform root;
    Material floorMaterial, wallMaterial, cyanMaterial, magentaMaterial;
    Vector2Int goal;
    readonly List<Vector2Int> cells = new List<Vector2Int>();
    static readonly Vector2Int[] Directions = {
        Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left
    };

    void Awake()
    {
        width = Mathf.Max(5, width | 1);
        height = Mathf.Max(5, height | 1);
        Generate();
    }

    Material MakeMaterial(Color baseColor, Color emission)
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (!shader) shader = Shader.Find("Standard");
        var mat = new Material(shader);
        mat.color = baseColor;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", baseColor);
        if (emission.maxColorComponent > 0)
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", emission);
        }
        return mat;
    }

    void Generate()
    {
        root = new GameObject("NEON MAZE - Generated").transform;
        root.SetParent(transform, false);
        floorMaterial = MakeMaterial(new Color(.018f, .027f, .07f), Color.black);
        wallMaterial = MakeMaterial(new Color(.045f, .055f, .13f), Color.black);
        cyanMaterial = MakeMaterial(new Color(.05f, .75f, 1f), new Color(0f, 1.5f, 3f));
        magentaMaterial = MakeMaterial(new Color(1f, .08f, .7f), new Color(2f, .03f, 1.4f));

        open = new bool[width, height];
        var rng = new System.Random(seed);
        var stack = new Stack<Vector2Int>();
        var start = new Vector2Int(width / 2 | 1, height / 2 | 1);
        if (start.x >= width - 1) start.x -= 2;
        if (start.y >= height - 1) start.y -= 2;
        open[start.x, start.y] = true;
        stack.Push(start);
        while (stack.Count > 0)
        {
            var current = stack.Peek();
            var options = new List<Vector2Int>();
            foreach (var dir in Directions)
            {
                var next = current + dir * 2;
                if (next.x > 0 && next.y > 0 && next.x < width - 1 && next.y < height - 1 && !open[next.x, next.y])
                    options.Add(dir);
            }
            if (options.Count == 0) { stack.Pop(); continue; }
            var chosen = options[rng.Next(options.Count)];
            var destination = current + chosen * 2;
            open[current.x + chosen.x, current.y + chosen.y] = true;
            open[destination.x, destination.y] = true;
            stack.Push(destination);
        }
        // Extra loops make the maze less linear while preserving connectivity.
        for (int x = 2; x < width - 2; x++)
        for (int y = 2; y < height - 2; y++)
        {
            if (open[x,y] || rng.NextDouble() > .13) continue;
            if ((open[x-1,y] && open[x+1,y]) || (open[x,y-1] && open[x,y+1])) open[x,y] = true;
        }
        float ox = -(width - 1) * cellSize * .5f;
        float oz = -(height - 1) * cellSize * .5f;
        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
        {
            var pos = new Vector3(ox + x * cellSize, 0, oz + y * cellSize);
            if (open[x,y])
            {
                cells.Add(new Vector2Int(x,y));
                Box("Floor", pos + Vector3.down * .16f, new Vector3(cellSize, .3f, cellSize), floorMaterial);
                if ((x + y) % 3 == 0)
                    Box("Neon floor stripe", pos + new Vector3(0, .012f, -cellSize * .4f),
                        new Vector3(cellSize * .65f, .025f, .08f), cyanMaterial);
            }
            else
            {
                Box("Wall", pos + Vector3.up * (wallHeight * .5f), new Vector3(cellSize, wallHeight, cellSize), wallMaterial);
                if (HasOpenNeighbor(x,y))
                {
                    Box("Neon wall crown", pos + Vector3.up * (wallHeight - .2f),
                        new Vector3(cellSize, .11f, cellSize), (x+y)%2==0 ? cyanMaterial : magentaMaterial);
                }
            }
        }
        // Portal sits at the farthest reachable corridor from the player start.
        goal = FindFarthest(start);
        var portal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        portal.name = "EXIT PORTAL - prototype";
        portal.transform.SetParent(root, false);
        portal.transform.position = GridToWorld(goal) + Vector3.up * 1.5f;
        portal.transform.localScale = new Vector3(1.5f, .09f, 1.5f);
        portal.GetComponent<Renderer>().sharedMaterial = magentaMaterial;
        Destroy(portal.GetComponent<Collider>());
    }

    bool HasOpenNeighbor(int x, int y)
    {
        foreach (var d in Directions)
        {
            int nx = x + d.x, ny = y + d.y;
            if (nx >= 0 && ny >= 0 && nx < width && ny < height && open[nx,ny]) return true;
        }
        return false;
    }

    Vector2Int FindFarthest(Vector2Int start)
    {
        var queue = new Queue<Vector2Int>();
        var seen = new HashSet<Vector2Int> { start };
        queue.Enqueue(start);
        var last = start;
        while (queue.Count > 0)
        {
            var p = queue.Dequeue();
            last = p;
            foreach (var d in Directions)
            {
                var n = p + d;
                if (n.x < 0 || n.y < 0 || n.x >= width || n.y >= height || !open[n.x,n.y] || !seen.Add(n)) continue;
                queue.Enqueue(n);
            }
        }
        return last;
    }

    GameObject Box(string label, Vector3 position, Vector3 scale, Material material)
    {
        var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = label;
        obj.transform.SetParent(root, false);
        obj.transform.position = position;
        obj.transform.localScale = scale;
        obj.GetComponent<Renderer>().sharedMaterial = material;
        return obj;
    }

    public Vector3 GridToWorld(Vector2Int p) =>
        new Vector3((p.x - (width - 1) * .5f) * cellSize, 0, (p.y - (height - 1) * .5f) * cellSize);

    public bool CanWalk(Vector3 position, float clearance = .65f)
    {
        // Four footprint samples prevent the hero from clipping wall corners.
        return IsFloor(position + new Vector3(clearance, 0, clearance))
            && IsFloor(position + new Vector3(-clearance, 0, clearance))
            && IsFloor(position + new Vector3(clearance, 0, -clearance))
            && IsFloor(position + new Vector3(-clearance, 0, -clearance));
    }

    bool IsFloor(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x / cellSize + (width - 1) * .5f);
        int y = Mathf.RoundToInt(position.z / cellSize + (height - 1) * .5f);
        return x >= 0 && y >= 0 && x < width && y < height && open[x,y]
            && Mathf.Abs(position.x - GridToWorld(new Vector2Int(x,y)).x) <= cellSize*.5f
            && Mathf.Abs(position.z - GridToWorld(new Vector2Int(x,y)).z) <= cellSize*.5f;
    }

    public Vector3 StartPosition
    {
        get
        {
            var p = new Vector2Int(width / 2 | 1, height / 2 | 1);
            if (p.x >= width - 1) p.x -= 2;
            if (p.y >= height - 1) p.y -= 2;
            return GridToWorld(p) + Vector3.up;
        }
    }

    public Vector3 RandomFloorPosition()
    {
        if (cells.Count == 0) return StartPosition;
        return GridToWorld(cells[Random.Range(0, cells.Count)]) + Vector3.up;
    }

    public Vector2Int WorldToCell(Vector3 position) =>
        new Vector2Int(Mathf.RoundToInt(position.x / cellSize + (width - 1) * .5f),
                       Mathf.RoundToInt(position.z / cellSize + (height - 1) * .5f));

    public bool IsCorridor(int x, int y) =>
        open != null && x >= 0 && y >= 0 && x < width && y < height && open[x,y];

    public bool ReachedGoal(Vector3 position) => Vector3.Distance(
        new Vector3(position.x, 0, position.z), GridToWorld(goal)) < 1.6f;
}
