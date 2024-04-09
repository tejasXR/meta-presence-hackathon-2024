using UnityEngine;

[CreateAssetMenu(fileName = "New Plant", menuName = "MQPP Hackathon/New Plant")]
public class Plant : ScriptableObject
{
    public enum GrowthType { Scale, Modular }

    public GameObject Prefab;
    public float LifeSpanInMinutes = 2f;
    public GrowthType Growth = GrowthType.Scale;
}
