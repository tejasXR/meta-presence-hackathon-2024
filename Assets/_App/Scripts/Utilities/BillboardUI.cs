using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    [SerializeField] private Vector3 offsetBillboardPivot;
    [Space] 
    [SerializeField] private bool lockXAxis;
    [SerializeField] private bool lockYAxis;
    [SerializeField] private bool lockZAxis;
    [Space]
    [SerializeField] private bool flipBillboard;
        
    private Camera _mainCamera;

    private void Awake()
    {
        var allCameras = FindObjectsOfType<Camera>();
        foreach (var cam in allCameras)
        {
            if (!cam.enabled)
                continue;

            if (cam.CompareTag("MainCamera"))
                _mainCamera = cam;
        }
    }

    private void Update()
    {
        Billboard();
    }

    private void Billboard()
    {
        var headPosition = _mainCamera.transform.position + offsetBillboardPivot;
        var position = transform.position;

        var lookAtPosition = new Vector3
        (
            lockXAxis ? position.x : headPosition.x,
            lockYAxis ? position.y : headPosition.y,
            lockZAxis ? position.z : headPosition.z
        ) + offsetBillboardPivot;
            
        if (flipBillboard)
            transform.LookAt(position - (lookAtPosition - position));
        else
            transform.LookAt(lookAtPosition);
    }
}
