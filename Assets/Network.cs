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
        var first = _planets.Count == 0;

        EnsureReady();
        var planet = Instantiate(planetPrefab);
        if (!first) planet.Size *= Random.Range(0.6f, 1.35f);
        planet.Render();
        planet.name = $"Planet {_planets.Count + 1}";
        planet.transform.parent = transform;
        _planets.Add(planet);

        if (first) return;

        var minimumDistance = 2.25f;
        var maximumDistance = 2.75f;

        // Attempt to find a suitable position for the new planet
        int indexOffset = Random.Range(0, _planets.Count);
        for (int i = 0; i < _planets.Count; i++)
        {
            Planet potentialNeighbor = _planets[(i + indexOffset) % _planets.Count];
            if (potentialNeighbor == planet) continue;

            // Pick a random angle
            float angle = Random.Range(0, 360) * Mathf.Deg2Rad;
            float distance = (potentialNeighbor.Size + planet.Size) * Random.Range(minimumDistance, maximumDistance);

            // Calculate the position
            Vector3 position = potentialNeighbor.transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * (distance / 100f);

            // Check if the position is valid
            bool valid = true;
            foreach (var other in _planets)
            {
                if (other == potentialNeighbor) continue;
                if (Vector3.Distance(position, other.transform.position) < (other.Size + planet.Size) * minimumDistance / 100f)
                {
                    valid = false;
                    break;
                }
            }

            if (valid)
            {
                planet.transform.position = position;
                // planet.neighbors.Add(potentialNeighbor);
                // potentialNeighbor.neighbors.Add(planet);
                return;
            }
        }

        // All attempts failed
        Debug.LogWarning("Failed to find a suitable position for the new planet");
        Destroy(planet.gameObject);
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

        if (_planets.Count == 0)
        {
            for (int i = 0; i < 5; i++)
                AddPlanet();
        }
    }
}
