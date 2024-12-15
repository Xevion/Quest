using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Planet : MonoBehaviour
{
    private MeshFilter _meshFilter;
    private LineRenderer _lineRenderer;
    private Network _network;

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
    }

    void OnEnable()
    {
    }

    bool IsConnected(Planet other)
    {
        return neighbors.Contains(other);
    }

    private void OnDestroy()
    {
        _network.Destroyed(this);
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

        _lineRenderer.widthMultiplier = edgeWidth;
        _lineRenderer.positionCount = 0;
        _lineRenderer.loop = true;
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

        Debug.Log($"{triangles.Length} triangles");

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();

        _meshFilter.mesh = mesh;
        _lineRenderer.positionCount = mesh.vertices.Length;
        _lineRenderer.SetPositions(mesh.vertices);
    }
}