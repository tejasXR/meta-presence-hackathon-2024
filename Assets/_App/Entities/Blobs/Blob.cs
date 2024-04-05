using UnityEngine;

[CreateAssetMenu(fileName = "New Blob", menuName = "MQPP Hackathon/New Blob")]
public class Blob : ScriptableObject
{
    [SerializeField] private GameObject _prefab;

    public GameObject Prefab => _prefab;
}
