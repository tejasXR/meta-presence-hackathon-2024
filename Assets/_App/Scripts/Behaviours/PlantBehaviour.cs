using Meta.XR.MRUtilityKit;
using UnityEngine;

/// <summary>
/// Add to any game object you wish to disappear into the ceiling
/// </summary>
public class PlantBehaviour : MonoBehaviour
{
    [SerializeField] private float _speed = 0.8f;

    private Transform _cameraTransform;
    private float _ceilingHeight = -1f;

    void Awake()
    {
        _cameraTransform = Camera.main.transform;
    }

    void Update() => UpdatePosition();

    private void UpdatePosition()
    {
        if (_ceilingHeight < 0f)
        {
            var room = MRUK.Instance.GetCurrentRoom();
            if (room != null)
            {
                _ceilingHeight = room.GetCeilingAnchor().transform.position.y;
            }
        }

        Vector3 currentPosition = transform.position;
        Vector3 targetPosition = transform.up * (_ceilingHeight + 1f);

        transform.position = Vector3.Slerp(currentPosition, targetPosition, _speed * Time.deltaTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"[{nameof(PlantBehaviour)}] {nameof(OnCollisionEnter)}: {nameof(collision)}={collision.gameObject.name}");
    }
}
