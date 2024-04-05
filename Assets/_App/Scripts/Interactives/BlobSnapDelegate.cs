using System;
using Oculus.Interaction;
using UnityEngine;

public class BlobSnapDelegate : MonoBehaviour, ISnapPoseDelegate
{
    public event Action Snapped;
    public event Action Unsnapped;
    
    [SerializeField] private LineRenderer lineRenderer;

    public bool IsSnapped { get; private set; }

    private void Awake()
    {
        lineRenderer.enabled = false;
    }

    public void TrackElement(int id, Pose p) { }

    public void UntrackElement(int id) { }

    public void SnapElement(int id, Pose pose)
    {
        lineRenderer.enabled = true;
        
        IsSnapped = true;
        Snapped?.Invoke();
    }

    public void UnsnapElement(int id)
    {
        lineRenderer.enabled = false;
        
        IsSnapped = false;
        Unsnapped?.Invoke();
    }

    public void MoveTrackedElement(int id, Pose p) { }

    public bool SnapPoseForElement(int id, Pose pose, out Pose result)
    {
        result = new Pose()
        {
            position = transform.position,
            rotation = transform.rotation
        };

        return true;
    }
}
