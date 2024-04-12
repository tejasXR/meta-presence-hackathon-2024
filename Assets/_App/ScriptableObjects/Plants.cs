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
        SimplePlantA,
        SimplePlantB
    }

    [SerializeField] private List<Plant> _plants;

    private Dictionary<PlantType, Plant> _plantsByType;

    public GameObject GetPrefab(PlantType type)
    {
        if (_plantsByType == null)
        {
            _plantsByType = new();
            foreach (var plant in _plants)
            {
                _plantsByType[plant.Type] = plant;
            }
        }
        return _plantsByType[type].Prefab;
    }
}
