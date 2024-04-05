using System;
using System.Collections.Generic;
using UnityEngine;

public class HandInteractor : MonoBehaviour
{
    private readonly Dictionary<int, Transform> _knownInteractors = new();
    private readonly Dictionary<int, Transform> _currentInteractors = new();

    public Dictionary<int, Transform> CurrentInteractors => _currentInteractors;
    public event Action<int, Transform> InteractionStarted;
    public event Action<int, Transform> InteractionEnded;

    void OnTriggerEnter(Collider collider)
    {
        Debug.Log($"({transform.parent.name})[{nameof(HandInteractor)}] {nameof(OnTriggerEnter)}: {collider.gameObject.name}");
        HandleColliderEnter(collider);
    }

    void OnTriggerExit(Collider collider)
    {
        Debug.Log($"({transform.parent.name})[{nameof(HandInteractor)}] {nameof(OnTriggerExit)}: {collider.gameObject.name}");
        HandleColliderExit(collider);
    }

    private void HandleColliderEnter(Collider collider)
    {
        int interactorId = collider.transform.GetInstanceID();
        bool newInteractor = false;

        if (!_knownInteractors.TryGetValue(interactorId, out Transform interactor))
        {
            newInteractor = true;
            if (collider.BelongsToOVRSkeleton(out OVRSkeleton ovrSkeleton))
            {
                interactor = collider.GetOVRBone(ovrSkeleton).Transform;
            }
            else if (collider.TryGetComponent(out BlobController blob))
            {
                interactor = collider.transform;

                // DIRTY !!!!!
                Instantiate(blob.Blob.Seed.PlantPrefab, interactor.position, Quaternion.identity);
            }
        }

        if (interactor != null)
        {
            _currentInteractors[interactorId] = interactor;
            InteractionStarted?.Invoke(interactorId, interactor);

            if (newInteractor)
            {
                _knownInteractors[interactorId] = interactor;
            }
        }
    }

    private void HandleColliderExit(Collider collider)
    {
        int interactorId = collider.transform.GetInstanceID();

        if (_currentInteractors.TryGetValue(interactorId, out Transform interactingTransform))
        {
            _currentInteractors.Remove(interactorId);
            InteractionEnded?.Invoke(interactorId, interactingTransform);
        }
    }
}
