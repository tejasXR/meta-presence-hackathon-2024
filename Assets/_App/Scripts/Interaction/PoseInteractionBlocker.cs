using Oculus.Interaction;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Blocks an interaction or deactivates a game object depending on the state of the ActiveStateSelector
/// </summary>
public class PoseInteractionBlocker : MonoBehaviour
{
    [SerializeField] private HandPoseActivator handPoseActivator;

    public UnityEvent stateSelectionActive;
    public UnityEvent stateSelectionInactive;

    private void Awake()
    {
        handPoseActivator.PoseActivated += OnStateSelectorActive;
        handPoseActivator.PoseDeactivated += OnStateSelectorInactive;
    }

    private void OnDestroy()
    {
        if (handPoseActivator)
        {
            handPoseActivator.PoseActivated -= OnStateSelectorActive;
            handPoseActivator.PoseDeactivated -= OnStateSelectorInactive;
        }
    }
    
    

    private void OnStateSelectorActive(HandPoseActivator handPoseActivator, Transform transform1)
    {
        stateSelectionActive?.Invoke();
    }

    private void OnStateSelectorInactive(HandPoseActivator handPoseActivator)
    {
        stateSelectionInactive?.Invoke();
    }
}
