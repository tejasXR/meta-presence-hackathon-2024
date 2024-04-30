using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Islands", menuName = "MQPP Hackathon/New Islands")]
public class Islands : ScriptableObject
{
    [Serializable]
    public class Island
    {
        public IslandType Type;
        public GameObject Prefab;
    }

    public enum IslandType
    {
        Unknown,
        BigIsland
    }

    [SerializeField] private List<Island> _islands;

    private Dictionary<IslandType, Island> _islandsByType;

    public bool TryGetPrefab(IslandType type, out GameObject prefab)
    {
        if (_islandsByType == null)
        {
            _islandsByType = new();
            foreach (Island newIsland in _islands)
            {
                _islandsByType[newIsland.Type] = newIsland;
            }
        }
        if (_islandsByType.TryGetValue(type, out Island island))
        {
            prefab = island.Prefab;
            return true;
        }

        Debug.LogWarning($"[{nameof(Islands)}] {nameof(TryGetPrefab)}: No prefab for island of {nameof(type)} {type}");
        prefab = null;
        return false;
    }
}
