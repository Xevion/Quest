using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(Planet))]
public class PlanetEditor : Editor
{
    Planet planet;

    void OnEnable()
    {
        planet = target as Planet;
    }

    public override void OnInspectorGUI()
    {

        if (GUILayout.Button("Delete Planet"))
        {
            DestroyImmediate(planet.gameObject);
            return;
        }

        // Check if size changed
        var newSize = EditorGUILayout.Slider("Size", planet.Size, 1f, 300f);
        if (newSize != planet.Size)
        {
            planet.Size = newSize;
            planet.GenerateLine();
        }

        // Check if bulbs changed
        var newBulbs = EditorGUILayout.IntSlider("Bulbs", planet.Bulbs, 0, 5);
        if (newBulbs != planet.Bulbs)
        {
            planet.Bulbs = newBulbs;
            planet.GenerateLine();
        }

        // Check if edge width changed
        var newEdgeWidth = EditorGUILayout.Slider("Edge Width", planet.edgeWidth, 0.001f, 0.2f);
        if (newEdgeWidth != planet.edgeWidth)
        {
            planet.edgeWidth = newEdgeWidth;
            planet.GenerateLine();
        }

        DrawDefaultInspector();
    }
}