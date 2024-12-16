using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Unit : MonoBehaviour
{
    private Planet planet;
    private Vector2 planetaryVelocity; // X: rotational, Y: altitude
    public float edgeWidth;
    public Color fillColor = Color.white;
    public Color edgeColor = Color.white;
    public float Size;
    public int TreeIndex;
    private float RotationSpeed;
    private float BobbingOffset;

    void Start()
    {
        Render();

        planet = GetComponentInParent<Planet>();
        planetaryVelocity = new Vector2(0, 0)
        {
            x = (Random.value > 0.5f ? 1 : -1) * 32 * Random.Range(0.8f, 1.2f),
        };
        RotationSpeed = Random.value > 0.5f ? 1 : -1 * Random.Range(0.8f, 1.2f) * 4f;
        BobbingOffset = Random.Range(0, (float)(2 * Math.PI));

        transform.position = planet.GetSurfacePosition(Random.Range(0, 360), 0.3f);

    }

    void Update()
    {
        // Rotate itself slightly
        transform.Rotate(new Vector3(0, 0, Time.deltaTime * RotationSpeed));
        transform.RotateAround(planet.transform.position, Vector3.forward, planetaryVelocity.x * Time.deltaTime);

        // Bobbing motion
        var unitAngle = planet.GetUnitAngle(this);
        var targetDistance = (Mathf.Sin(BobbingOffset + Time.time) + 1) / 2;
        targetDistance = Mathf.Lerp(0.35f, 0.8f, targetDistance);
        if (TreeIndex == 0)
            Debug.Log(targetDistance);
        transform.position = planet.GetSurfacePosition(unitAngle, targetDistance);

        planet.Tree.Points[TreeIndex] = transform.position;
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