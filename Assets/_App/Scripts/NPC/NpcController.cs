using System;
using UnityEngine;

public class NpcController : MonoBehaviour
{
    [SerializeField] private NpcOptionsMenu optionsMenu;
    [SerializeField] private float moveSpeed;

    public enum MovementTypeEnum
    {
        Idle,
        MovingRandomly,
        MovingToPlayer
    }

    private const float SHOW_NPC_OPTION_DISTANCE_THRESHOLD = .025F;
    
    private MovementTypeEnum _movementType;
    private GameObject _orientedDialogueOption;
    private Transform _destinationTransform;
    private bool _shouldMove;
    private Camera _mainCamera;
    private NpcCaller.PoseOrientation _currentPoseOrientation;

    private void Awake()
    {
        _mainCamera = Camera.main;
        SetNpcMenuOrientation(NpcCaller.PoseOrientation.LeftHand);
    }

    private void Update()
    {
        if (_shouldMove)
        {
            transform.position = Vector3.Lerp(transform.position, _destinationTransform.position, Time.deltaTime * moveSpeed);
            
            if (_movementType == MovementTypeEnum.MovingToPlayer)
            {
                if (Vector3.Distance(transform.position, _destinationTransform.position) < SHOW_NPC_OPTION_DISTANCE_THRESHOLD)
                {
                    optionsMenu.Show(_currentPoseOrientation);
                }
            }
        }
        
        // Currently, we have the NPC looking towards the player at all times
        transform.LookAt(_mainCamera.transform);
    }

    public void MoveToPoint(Transform destPoint, MovementTypeEnum movementType)
    {
        _shouldMove = true;
        _destinationTransform = destPoint;
        _movementType = movementType;
        
        optionsMenu.Hide();
    }

    public void CancelMovement(bool hideDialogueOptions)
    {
        _shouldMove = false;
        _movementType = MovementTypeEnum.Idle;
        
        if (hideDialogueOptions)
            optionsMenu.Hide();
    }

    public void SetNpcMenuOrientation(NpcCaller.PoseOrientation poseOrientation, bool toggleOnOptions = false)
    {
        optionsMenu.Hide();

        if (toggleOnOptions)
            optionsMenu.Show(poseOrientation);

        _currentPoseOrientation = poseOrientation;
    }
}


