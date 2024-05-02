using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class FlingDetector : MonoBehaviour
{
    [SerializeField] private Vector3 _flingDirection = Vector3.up;
    [SerializeField] private float _flingVelocity = 0.9f;
    [SerializeField] private float _flingDecelerateVelocity = 0.1f;

    public UnityEvent OnFling;

    private Rigidbody _rigidbody;
    private Collider _collider;
    private bool _flung;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        
        _flingDirection = _flingDirection.normalized;
    }

    void Update()
    {
        float velocityInFlingDirection = Vector3.Dot(_rigidbody.velocity, _flingDirection);
        if (_flung)
        {
            if (velocityInFlingDirection <= _flingDecelerateVelocity)
            {
                _rigidbody.isKinematic = true;
                
                OnFling?.Invoke();
                _flung = false;
            }
        }
        else if (velocityInFlingDirection >= _flingVelocity)
        {
            _flung = true;
        }
    }
}
