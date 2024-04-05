using UnityEngine;

[CreateAssetMenu(fileName = "New Blob", menuName = "MQPP Hackathon/New Blob")]
public class Blob : ScriptableObject
{
    [SerializeField] private GameObject _prefab;

    [SerializeField] private Seed _seed;

    public GameObject Prefab => _prefab;

    public Seed Seed => _seed;
}
