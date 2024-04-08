using Oculus.Interaction;
using UnityEngine;

public class NPCDialogueController : MonoBehaviour
{
    [SerializeField] private PointableElement pointableElement;
    [SerializeField] private GameObject dialogueOptions;
    
    private void Awake()
    {
        pointableElement.WhenPointerEventRaised += OnPointerEventRaised;
    }

    private void OnPointerEventRaised(PointerEvent pointerEvent)
    {
        if (pointerEvent.Type == PointerEventType.Hover)
        {
            dialogueOptions.SetActive(true);
        }

        if (pointerEvent.Type == PointerEventType.Unhover)
        {
            dialogueOptions.SetActive(false);
        }
    }
}
