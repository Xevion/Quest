using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
    private PolygonCollider2D _collider;
    private LineRenderer _lineRenderer;
    private Network _network;

    [SerializeField] public HashSet<Planet> neighbors;

    [HideInInspector] public float Size; // 1.0 -
    [HideInInspector] public int Bulbs; // 0 - 5+
    public Color edgeColor = Color.white;
    [HideInInspector] public float edgeWidth = 0.2f;

    void Awake()
    {
        _collider = gameObject.GetComponent<PolygonCollider2D>();
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

    public void GenerateLine()
    {
        _lineRenderer = gameObject.GetComponent<LineRenderer>();

        _lineRenderer.widthMultiplier = edgeWidth;
        _lineRenderer.positionCount = 0;
        _lineRenderer.loop = true;
        _lineRenderer.startColor = edgeColor;
        _lineRenderer.endColor = edgeColor;
        _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

        // Calculate points for circle
        int pointCount = Math.Max((int)Size, 16);
        _lineRenderer.positionCount = pointCount;
        var points = new Vector3[pointCount];

        // Generate points
        for (var i = 0; i < pointCount; i++)
        {
            var rad = Mathf.Deg2Rad * (i * 360f / pointCount);
            points[i] = new Vector3(Mathf.Sin(rad) * Size / 100f, Mathf.Cos(rad) * Size / 100f, 0);
        }

        // Add points to LineRenderer
        _lineRenderer.SetPositions(points);

        // Use the line-renderer's Vector3's to create vector2 collider path
        var path = new Vector2[_lineRenderer.positionCount + 1];
        for (var i = 0; i < pointCount; i++)
        {
            // Convert Vector3 to Vector2
            path[i] = points[i];
        }

        path[_lineRenderer.positionCount] = path[0];
        // _collider.SetPath(0, path);
    }
}