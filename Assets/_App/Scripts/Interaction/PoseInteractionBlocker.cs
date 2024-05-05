using Oculus.Interaction;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Blocks an interaction or deactivates a game object depending on the state of the ActiveStateSelector
/// </summary>
public class PoseInteractionBlocker : MonoBehaviour
{
    [SerializeField] private ActiveStateSelector activeStateSelector;

    public UnityEvent stateSelectionActive;
    public UnityEvent stateSelectionInactive;

    private int _poseBlockers;

    private void Awake()
    {
        activeStateSelector.WhenSelected += OnStateSelectorActive;
        activeStateSelector.WhenUnselected += OnStateSelectorInactive;
    }

    private void OnDestroy()
    {
        activeStateSelector.WhenSelected -= OnStateSelectorActive;
        activeStateSelector.WhenUnselected -= OnStateSelectorInactive;
    }

    private void OnStateSelectorActive()
    {
        stateSelectionActive?.Invoke();
    }

    private void OnStateSelectorInactive()
    {
        stateSelectionInactive?.Invoke();
    }
}
