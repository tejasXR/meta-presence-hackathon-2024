using UnityEngine;

[CreateAssetMenu(fileName = "New Seed", menuName = "MQPP Hackathon/New Seed")]
public class Seed : ScriptableObject
{
    [Tooltip("The kind of plant this seed will give origin to.")]
    [SerializeField] private GameObject _plantPrefab;

    [Tooltip("The growth speed of this plant.")]
    [SerializeField] private float _growthSpeed = 1f;

    public GameObject PlantPrefab => _plantPrefab;
}
