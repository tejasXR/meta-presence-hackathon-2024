using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Plants", menuName = "MQPP Hackathon/New Plants")]
public class Plants : ScriptableObject
{
    [Serializable]
    public class Plant
    {
        public PlantType Type;
        public GameObject Prefab;
    }

    public enum PlantType
    {
        Unknown,
        KelpBoa,
        KelpBull,
        BranchA,
        BranchTrumpet
    }

    [SerializeField] private List<Plant> _plants;

    private Dictionary<PlantType, Plant> _plantsByType;

    public bool TryGetPrefab(PlantType type, out GameObject prefab)
    {
        if (_plantsByType == null)
        {
            _plantsByType = new();
            foreach (Plant newPlant in _plants)
            {
                _plantsByType[newPlant.Type] = newPlant;
            }
        }
        if (_plantsByType.TryGetValue(type, out Plant plant))
        {
            prefab = plant.Prefab;
            return true;
        }

        Debug.LogWarning($"[{nameof(Plants)}] {nameof(TryGetPrefab)}: No prefab for plant of {nameof(type)} {type}");
        prefab = null;
        return false;
    }
}
