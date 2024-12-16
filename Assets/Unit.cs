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
        Debug.Log($"{name} belongs to {planet.name}");
    }

    void Update()
    {
        // Rotate itself slightly
        transform.Rotate(new Vector3(0, 0, Time.deltaTime * RotationSpeed));

        transform.Translate(Vector3.up * Time.deltaTime);

        // Get distance from planet
        var distance = Vector2.Distance(transform.position, planet.transform.position) - planet.Size / 100f;
        var maxDistance = 1f;
        var minDistance = 0.2f;

        bool isTooFar = distance > maxDistance;
        bool isTooClose = distance < minDistance;

        // If incorrect distance, rotate
        if (isTooFar || isTooClose)
        {
            var directionToPlanet = (planet.transform.position - transform.position).normalized;
            var projectionOnRight = Vector3.Dot(directionToPlanet, transform.right);
            var planetOnRight = projectionOnRight < 0;

            var direction = planetOnRight == isTooFar ? 1 : -1;
            var turningSpeed = 200f;
            if (isTooClose) turningSpeed *= 3;
            else if (isTooFar) turningSpeed *= (distance - maxDistance);
            transform.Rotate(new Vector3(0, 0, direction * turningSpeed * Time.deltaTime));

            // var angle = Mathf.Atan2(transform.position.y, transform.position.x) * Mathf.Rad2Deg;
            // angle += Random.Range(-10, 10);
            // transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        planet.Tree.Points[TreeIndex] = transform.position;
    }

    public void OnDrawGizmos()
    {
        // Draw a line forward
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.up);
    }

    private void OnDestroy()
    {
        if (planet == null) return;
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