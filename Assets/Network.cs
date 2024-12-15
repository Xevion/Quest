using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Network : MonoBehaviour
{
    private List<Planet> _planets;
    public Planet planetPrefab;

    public void EnsureReady()
    {
        if (_planets == null)
        {
            _planets = new List<Planet>(FindObjectsOfType<Planet>());
        }
        else
        {
            _planets.Clear();
            _planets.AddRange(FindObjectsOfType<Planet>());
        }
    }

    public void Awake()
    {
        EnsureReady();
    }

    public void AddPlanet()
    {
        EnsureReady();
        var planet = Instantiate(planetPrefab);
        planet.Render();
        planet.name = $"Planet {_planets.Count + 1}";
        planet.transform.parent = transform;
        _planets.Add(planet);
    }

    /// <summary>
    /// Signal that a planet has been destroyed
    /// </summary>
    /// <param name="destroyed"></param>
    public void Destroyed(Planet destroyed)
    {
        _planets.Remove(destroyed);
        foreach (var planet in _planets)
        {
            if (planet != destroyed) continue;
            planet.neighbors.Remove(destroyed);
        }
    }

    void Start()
    {
        _planets = new List<Planet>(FindObjectsOfType<Planet>());

        AddPlanet();
    }

    void Update()
    {
    }
}
