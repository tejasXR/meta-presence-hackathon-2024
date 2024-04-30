using UnityEngine;

public class IslandController : MonoBehaviour
{
    [SerializeField] private Islands.IslandType _type;

    public Islands.IslandType Type => _type;
}
