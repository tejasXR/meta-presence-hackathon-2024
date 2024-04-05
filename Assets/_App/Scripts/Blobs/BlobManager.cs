using System.Collections.Generic;
using UnityEngine;

public class BlobManager : MonoBehaviour
{
    [SerializeField] private List<Blob> Blobs;

    private Transform _cameraTransform;
    private RaycastHit[] _hitResults;
    private BlobController _currentBlob;

    void Awake()
    {
        _cameraTransform = Camera.main.transform;
    }

    public void OpenHandGestureDetected()
    {
        Debug.Log($"[{nameof(BlobManager)}] {nameof(OpenHandGestureDetected)}");

        if (_currentBlob != null)
        {
            Debug.Log($"[{nameof(BlobManager)}] {nameof(OpenHandGestureDetected)}: Bye Blob!");
            _currentBlob.WavedAt();
            _currentBlob = null;
            return;
        }

        _hitResults = Physics.CapsuleCastAll(point1: transform.position,
                                                   point2: transform.position + _cameraTransform.forward * 5f,
                                                   radius: 1f,
                                                   direction: _cameraTransform.forward,
                                                   maxDistance: 5f,
                                                   layerMask: ~0);
        if (_hitResults.Length > 0)
        {
            foreach (RaycastHit hit in _hitResults)
            {
                Debug.Log($"[{nameof(BlobManager)}] {nameof(OpenHandGestureDetected)}: {nameof(hit)}={hit.collider.gameObject.name}");

                if (hit.collider.gameObject.TryGetComponent(out BlobController blob))
                {
                    Debug.Log($"[{nameof(BlobManager)}] {nameof(OpenHandGestureDetected)}: Hi Blob!");
                    _currentBlob = blob;
                    _currentBlob.WavedAt();
                    return;
                }
            }
        }
    }
}
