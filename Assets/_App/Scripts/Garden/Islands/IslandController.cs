using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using UnityEngine;

public class IslandController : MonoBehaviour
{
    [SerializeField] private Islands.IslandType _type;
    [SerializeField] private Transform _plantSpawnPointsRoot;

    public Islands.IslandType Type => _type;


    private List<Transform> _plantSpawnPoints;
    public List<Transform> PlantSpawnPoints
    {
        get
        {
            if (_plantSpawnPoints == null)
            {
                _plantSpawnPoints = new();
                foreach (Transform spawnPoint in _plantSpawnPointsRoot)
                {
                    if (MRUK.Instance.GetCurrentRoom().IsPositionInRoom(spawnPoint.position, testVerticalBounds: false))
                    {
                        _plantSpawnPoints.Add(spawnPoint);
                    }
                }
            }
            return _plantSpawnPoints;
        }
    }

    public Transform GetAvailableSpawnPoint()
    {
        // TODO(yola): Check if spawn point is occupied.
        return PlantSpawnPoints[Random.Range(0, PlantSpawnPoints.Count)];
    }
}
