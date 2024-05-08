using UnityEngine;

public class PokeInteractableDestroyGardenController : MonoBehaviour
{
    private GardenManager _gardenManager;

    private void Awake()
    {
        _gardenManager = FindObjectOfType<GardenManager>();
    }

    public void OnSelect()
    {
        _gardenManager.DestroyGarden();
    } 
}
