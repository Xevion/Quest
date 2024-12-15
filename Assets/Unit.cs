using System.Linq;
using UnityEngine;

public class Unit : MonoBehaviour
{
    private Planet planet;
    private Vector2 planetaryVelocity; // X: rotational, Y: altitude
    public float edgeWidth;
    public Color fillColor = Color.white;
    public Color edgeColor = Color.white;
    public float Size;

    void Start()
    {
        Render();

        planet = GetComponentInParent<Planet>();
        planetaryVelocity = new Vector2(0, 0)
        {
            x = (Random.value > 0.5f ? 1 : -1) * 32 * Random.Range(0.8f, 1.2f),
        };

        transform.position = planet.GetSurfacePosition(Random.Range(0, 360), 0.4f);

    }

    void Update()
    {
        transform.RotateAround(planet.transform.position, Vector3.forward, planetaryVelocity.x * Time.deltaTime);
        transform.position = Vector2.MoveTowards(transform.position, planet.transform.position, planetaryVelocity.y * Time.deltaTime);
    }

    private void OnDestroy()
    {
        planet.UnitDestroyed(this);
    }

    public override int GetHashCode()
    {
        return gameObject.GetHashCode();
    }

    public void Render()
    {
        var meshFilter = gameObject.GetComponent<MeshFilter>();
        var lineRenderer = gameObject.GetComponent<LineRenderer>();
        var meshCollider = gameObject.GetComponent<MeshCollider>();
        var meshRenderer = gameObject.GetComponent<MeshRenderer>();

        lineRenderer.widthMultiplier = edgeWidth;
        lineRenderer.positionCount = 0;
        lineRenderer.useWorldSpace = false;
        lineRenderer.loop = true;
        lineRenderer.startColor = edgeColor;
        lineRenderer.endColor = edgeColor;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        meshRenderer.material = new Material(Shader.Find("Sprites/Default"));

        // Unit is just a simple triangle
        var vertices = new[]
        {
            new Vector2(0, 0),
            new Vector2(0.5f, 1),
            new Vector2(1, 0)
        };

        vertices = vertices.Select(v => v * Size / 100f).ToArray();

        // Find all the triangles in the shape
        var triangles = new Triangulator(vertices).Triangulate();

        // Assign each vertex the fill color
        var colors = Enumerable.Repeat(fillColor, vertices.Length).ToArray();

        var mesh = new Mesh
        {
            name = "Triangle",
            vertices = vertices.ToVector3(),
            triangles = triangles,
            colors = colors
        };

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();

        meshCollider.sharedMesh = mesh;
        meshFilter.mesh = mesh;
        lineRenderer.positionCount = mesh.vertices.Length;
        lineRenderer.SetPositions(mesh.vertices);
    }
}