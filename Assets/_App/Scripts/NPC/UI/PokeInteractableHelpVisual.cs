using UnityEngine;

public class PokeInteractableHelpVisual : MonoBehaviour
{
    [SerializeField] private GameObject helpVisual;

    private void OnDisable()
    {
        HideVisual();
    }

    public void OnWhenSelect()
    {
        helpVisual.SetActive(!helpVisual.activeSelf);
    }

    private void HideVisual()
    {
        helpVisual.SetActive(false);
    }
}
