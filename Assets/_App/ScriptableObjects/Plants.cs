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

    public GameObject GetPrefab(PlantType type)
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
            return plant.Prefab;
        }

        Debug.LogWarning($"[{nameof(Plants)}] {nameof(GetPrefab)}: No prefab for plant of {nameof(type)} {type}");
        return null;
    }
}
