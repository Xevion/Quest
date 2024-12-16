using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DataStructures.ViliWonka.KDTree;

public class Planet : MonoBehaviour
{
    private MeshFilter _meshFilter;
    private LineRenderer _lineRenderer;
    private CircleCollider2D _circleCollider;
    private Network _network;
    public KDQuery Query => _query;
    private KDQuery _query;
    public KDTree Tree => _tree;
    private KDTree _tree;
    private List<Unit> _units;

    [SerializeField] public HashSet<Planet> neighbors;

    [HideInInspector] public float Size; // 1.0 -
    [HideInInspector] public int Bulbs; // 0 - 5+
    public Color edgeColor = Color.white;
    public Color fillColor = Color.white;
    [HideInInspector] public float edgeWidth = 0.2f;

    void Awake()
    {
        _lineRenderer = gameObject.GetComponent<LineRenderer>();
        _network = FindObjectOfType<Network>();
        _units = new List<Unit>();
        _query = new KDQuery();
        _tree = new KDTree();
    }

    void Start()
    {
        while (_units.Count < 16)
            SpawnUnit();
    }

    void OnMouseDown()
    {
        SpawnUnit();
    }

    public Vector2 GetSurfacePosition(float angle, float distance = 0)
    {
        var radius = Size / 100f + distance;
        var theta = angle * Mathf.Deg2Rad;
        return (Vector2)transform.position + new Vector2(Mathf.Cos(theta), Mathf.Sin(theta)) * radius;
    }

    public float GetAngle(Vector2 position)
    {
        var angle = Mathf.Atan2(position.y - transform.position.y, position.x - transform.position.x) * Mathf.Rad2Deg;
        return angle < 0 ? angle + 360 : angle;
    }

    void SpawnUnit()
    {
        var unitPrefab = Resources.Load<GameObject>("BaseUnit");
        var unitObject = Instantiate(unitPrefab);

        var unit = unitObject.GetComponent<Unit>();
        unit.transform.SetParent(transform);
        unit.transform.position = GetSurfacePosition(UnityEngine.Random.Range(0, 360), 0.3f);

        _units.Add(unit);
        unit.name = "Unit " + _units.Count;

        _tree.SetCount(_tree.Count + 1);
        var index = _tree.Count - 1;
        unit.TreeIndex = index;

        _tree.Points[index] = new Vector2(unit.transform.position.x, unit.transform.position.y);
        _tree.Rebuild();

    }

    void FixedUpdate()
    {
    }

    void ChangeUnits()
    {
        if (_units.Count < 50) return;

        // Delete 3 units
        for (int i = 0; i < 3; i++)
        {
            if (_units.Count == 0) break;
            var unit = _units[0];
            Destroy(unit.gameObject);
        }

        // Add 4 units
        for (int i = 0; i < 4; i++)
        {
            SpawnUnit();
        }

    }

    void OnDrawGizmos()
    {
        if (_units.Count == 0) return;

        var origin = _units[0];
        // Draw sphere on first unit
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(origin.transform.position, 0.1f);

        var results = new List<int>();
        var resultDistances = new List<float>();
        _query.KNearest(_tree, origin.transform.position, 2, results, resultDistances);

        if (results.Count < 2) return;

        var closestUnit = results[0];

        // Draw line to closest unit
        Gizmos.color = Color.green;
        Gizmos.DrawLine(origin.transform.position, _tree.Points[closestUnit]);
    }

    bool IsConnected(Planet other)
    {
        return neighbors.Contains(other);
    }

    private void OnDestroy()
    {
        _network.Destroyed(this);
    }

    public void UnitDestroyed(Unit unit)
    {
        _tree.Points[unit.TreeIndex] = new Vector2(float.MaxValue, float.MaxValue);
        _units.Remove(unit);
    }

    void Connect(params Planet[] others)
    {
        foreach (var other in others)
        {
            if (other == this) continue;
            neighbors.Add(other);
        }
    }

    public void Render()
    {
        _meshFilter = gameObject.GetComponent<MeshFilter>();
        _lineRenderer = gameObject.GetComponent<LineRenderer>();
        _circleCollider = gameObject.GetComponent<CircleCollider2D>();

        _lineRenderer.widthMultiplier = edgeWidth;
        _lineRenderer.positionCount = 0;
        _lineRenderer.loop = true;
        _lineRenderer.useWorldSpace = false;
        _lineRenderer.startColor = edgeColor;
        _lineRenderer.endColor = edgeColor;
        _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

        var radius = Size / 100f;
        const float segmentOffset = 40f;
        const float segmentMultiplier = 50f;
        var numSegments = (int)(radius * segmentMultiplier + segmentOffset);

        // Create an array of points around a circle
        var circleVertices = Enumerable.Range(0, numSegments)
            .Select(i =>
            {
                var theta = 2 * Mathf.PI * i / numSegments;
                return new Vector2(Mathf.Cos(theta), Mathf.Sin(theta)) * radius;
            })
            .ToArray();

        // Find all the triangles in the shape
        var triangles = new Triangulator(circleVertices).Triangulate();

        // Assign each vertex the fill color
        var colors = Enumerable.Repeat(fillColor, circleVertices.Length).ToArray();

        var mesh = new Mesh
        {
            name = "Circle",
            vertices = circleVertices.ToVector3(),
            triangles = triangles,
            colors = colors
        };

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();

        _circleCollider.radius = radius;

        _meshFilter.mesh = mesh;
        _lineRenderer.positionCount = mesh.vertices.Length;
        _lineRenderer.SetPositions(mesh.vertices);
    }
}