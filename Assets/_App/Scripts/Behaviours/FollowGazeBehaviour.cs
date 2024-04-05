using UnityEngine;

/// <summary>
/// Add to any game object you wish to follow the player's gaze.
/// </summary>
public class FollowGazeBehaviour : MonoBehaviour
{
    [SerializeField] private float _followSpeed = 1.15f;
    [SerializeField] private float _distance = 0.95f;

    private Transform _cameraTransform;

    void Awake()
    {
        _cameraTransform = Camera.main.transform;
    }

    void Update() => UpdatePosition();

    private void UpdatePosition()
    {
        Vector3 currentPosition = transform.position;
        Vector3 targetPosition = _cameraTransform.position + _cameraTransform.forward * _distance;

        transform.position = Vector3.Slerp(currentPosition, targetPosition, _followSpeed * Time.deltaTime);
    }
}
