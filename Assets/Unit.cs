using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class Unit : MonoBehaviour
{
    private Planet planet;
    public float edgeWidth;
    public Color fillColor = Color.white;
    public Color edgeColor = Color.white;
    public float Size;
    public int TreeIndex;
    private Vector2 _velocity = Vector3.up;
    private float timeOffset;

    void Start()
    {
        Render();

        _velocity = Random.insideUnitCircle.normalized;
        planet = GetComponentInParent<Planet>();
        timeOffset = Random.Range(0, (float)(2 * Math.PI));
    }

    private Vector2 SteerTowards(Vector2 vector)
    {
        Vector2 v = vector.normalized * 1 - _velocity;
        var clamped = Vector2.ClampMagnitude(v, 1);
        Debug.Log($"SteerTowards {planet.name}/{name} Vector: {vector}, Modified Vector: {v}, Cur Velocity: {_velocity}, Clamped Vector: {clamped}");
        return clamped;
    }

    void Update()
    {
        var acceleration = Vector2.zero;

        // var upcomingAngle = planet.GetAngle(transform.position + (Vector3)_velocity);
        // var angleTarget = planet.GetSurfacePosition(upcomingAngle, 0.4f);
        // acceleration -= angleTarget;

        var targetDistance = (Mathf.Sin(Time.time * 0.5f + timeOffset) + 1) / 2f;
        var surfaceTarget = planet.GetSurfacePosition(planet.GetAngle((Vector2)transform.position + _velocity), targetDistance);
        var surfaceTargetVector = ((Vector2)transform.position - surfaceTarget).normalized;
        var distance = Vector2.Distance(transform.position, planet.transform.position) - planet.Size / 100f;
        // var vectorMultiplier = Mathf.LerpUnclamped(1f, 1.5f, 2f * Math.Abs(targetDistance - distance) / targetDistance);
        // var vectorMultiplier = Mathf.LerpUnclamped(0.5f, 1f, distance < targetDistance ? 0.3f / distance : distance / 0.2f);
        surfaceTargetVector *= 1.5f;
        acceleration -= surfaceTargetVector;

        if (TreeIndex == 0)
        {
            Debug.Log($"{planet.name} Acceleration: {acceleration}, Velocity: {_velocity}");
        }

        Vector2 newVelocity = _velocity + acceleration * Time.deltaTime;
        float speed = newVelocity.magnitude;
        Vector2 dir = newVelocity / speed;
        speed = Mathf.Clamp(speed, 0.5f, 1.5f);
        _velocity = dir * speed;

        var results = new List<int>();
        planet.Query.Radius(planet.Tree, transform.position, 0.1f, results);
        Vector2 away = Vector2.zero;
        foreach (var unitIndex in results)
        {
            if (unitIndex == TreeIndex) continue;
            var diff = (Vector2)(planet.Tree.Points[unitIndex] - transform.position);
            away -= diff.normalized;
        }
        acceleration += away * 0.5f;

        transform.eulerAngles = new Vector3(0, 0, Mathf.Rad2Deg * -Mathf.Atan2(_velocity.x, _velocity.y));
        transform.position += (Vector3)_velocity * Time.deltaTime;
        planet.Tree.Points[TreeIndex] = transform.position;

        // var angle = planet.GetAngle(transform.position);
        // var forwardAngle = planet.GetAngle(transform.forward);

        // Get distance from planet
        // var distance = Vector2.Distance(transform.position, planet.transform.position) - planet.Size / 100f;
        // var maxDistance = 1f;
        // var minDistance = 0.2f;

        // bool isTooFar = distance > maxDistance;
        // bool isTooClose = distance < minDistance;

        // var steerTarget = Vector3.zero;

        // If incorrect distance, rotate
        // if (isTooFar || isTooClose)
        // {
        // var directionToPlanet = (planet.transform.position - transform.position).normalized;
        // var projectionOnRight = Vector3.Dot(directionToPlanet, transform.right);
        // var planetOnRight = projectionOnRight < 0;

        // steerTarget = planetOnRight ? transform.right : -transform.right;

        // var direction = planetOnRight == isTooFar ? 1 : -1;
        // var turningSpeed = 200f;
        // if (isTooClose) turningSpeed *= 3;
        // else if (isTooFar) turningSpeed *= (distance - maxDistance);
        // transform.Rotate(new Vector3(0, 0, direction * turningSpeed * Time.deltaTime));

        // if (steerTarget == Vector3.zero)
        //     steerTarget = transform.up;


        // var angle = new Vector3(0, 0, Mathf.Rad2Deg * -Mathf.Atan2(steerTarget.x, steerTarget.y));
        // transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, angle, Time.deltaTime * 2f);

        // transform.position += transform.up * Time.deltaTime;
    }

    public void OnDrawGizmos()
    {
        var surfaceTarget = planet.GetSurfacePosition(planet.GetAngle((Vector2)transform.position + _velocity), 0.4f);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, surfaceTarget);

        var velocityPosition = transform.position + (Vector3)_velocity;
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, velocityPosition);

        // Draw the angle from the planet center to the unit
        // var angle = planet.GetAngle(transform.position);
        // // Draw the angle from the unit's immediate forward to the planet center
        // var forwardAngle = planet.GetAngle(transform.position - transform.up);
        // var distance = Vector2.Distance(transform.position, planet.transform.position) - planet.Size / 100f;

        // Gizmos.color = Color.red;
        // Gizmos.DrawLine(planet.transform.position, planet.GetSurfacePosition(angle, distance));
        // Gizmos.color = Color.green;
        // Gizmos.DrawLine(planet.transform.position, planet.GetSurfacePosition(forwardAngle, distance));

        // // Draw a line forward
        // Gizmos.color = Color.red;
        // Gizmos.DrawLine(transform.position, transform.position + transform.up);
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
            new Vector2(-0.5f, 0),
            new Vector2(0f, 1.4f),
            new Vector2(0.5f, 0)
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