using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HandInteractor))]
public class HandInteractorExample : MonoBehaviour
{
    [SerializeField] private GameObject _interactorPrefab;

    private readonly Pooler<GameObject> _interactorPooler = new();
    private readonly Dictionary<int, GameObject> _interactorGameObjects = new();

    private HandInteractor _interactibleRoomSurface;

    void Awake()
    {
        _interactorPooler.Initialize(InstantiateInteractor, 0);

        _interactibleRoomSurface = GetComponent<HandInteractor>();
        _interactibleRoomSurface.InteractionStarted += OnInteractionStarted;
        _interactibleRoomSurface.InteractionEnded += OnInteractionEnded;
    }

    void Update()
    {
        foreach (var interactorKVPair in _interactorGameObjects)
        {
            if (_interactibleRoomSurface.CurrentInteractors.TryGetValue(interactorKVPair.Key, out Transform interactor))
            {
                interactorKVPair.Value.transform.position = interactor.position;
            }
            else
            {
                DisposeInteractor(interactorKVPair.Key, interactorKVPair.Value);
            }
        }
    }

    private void OnInteractionStarted(int interactorId, Transform _)
    {
        if (!_interactorGameObjects.TryGetValue(interactorId, out GameObject interactor))
        {
            interactor = _interactorPooler.BorrowItem();
            _interactorGameObjects[interactorId] = interactor;
        }
        interactor.SetActive(true);
    }

    private void OnInteractionEnded(int interactorId, Transform _)
    {
        if (_interactorGameObjects.TryGetValue(interactorId, out GameObject interactor))
        {
            DisposeInteractor(interactorId, interactor);
        }
    }

    private GameObject InstantiateInteractor(int index)
    {
        GameObject go = Instantiate(_interactorPrefab, transform.parent.parent);
        go.name = $"Interactor_{index}";
        go.SetActive(false);
        return go;
    }

    private void DisposeInteractor(int interactorId, GameObject interactor)
    {
        interactor.SetActive(false);
        _interactorPooler.ReturnItem(interactor);
        _interactorGameObjects.Remove(interactorId);
    }
}
