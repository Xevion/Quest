using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(Network))]
public class NetworkEditor : Editor
{
    Network network;

    void OnEnable()
    {
        network = target as Network;
    }

    //     public override VisualElement CreateInspectorGUI()
    //     {
    //         VisualElement root = new();

    //         addPlanetButton = new Button(() => { network.AddPlanet(); }) { text = "Add Planet" };
    //         // var planetList = new TreeView() { style = { flexGrow = 1 } };
    //         // planetList.Bind()

    //         // planetList.makeItem = () => new Label();
    //         // planetList.bindItem = (element, i) => (element as Label).text = planetList.itemsSource[i].ToString();

    //         // root.Add(planetList);
    //         root.Add(addPlanetButton);

    //         return root;

    public override void OnInspectorGUI()
    {

        if (GUILayout.Button("Add Planet"))
        {
            network.AddPlanet();
        }

        DrawDefaultInspector();
    }
}