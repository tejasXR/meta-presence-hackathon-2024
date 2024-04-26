using UnityEngine;

public class NpcController : MonoBehaviour
{
    [SerializeField] private GameObject leftDialogueOptions;
    [SerializeField] private GameObject rightDialogueOptions;
    [SerializeField] private float moveSpeed;

    public enum MovementTypeEnum
    {
        Idle,
        MovingRandomly,
        MovingToPlayer
    }

    private const float SHOW_DIALOGUE_OPTION_DISTANCE_THRESHOLD = .025F;
    
    private MovementTypeEnum _movementType;
    private GameObject _orientedDialogueOption;
    private Transform _destinationTransform;
    private bool _shouldMove;
    private Camera _mainCamera;

    private void Awake()
    {
        _mainCamera = Camera.main;
        SetDialogueOrientation(NpcCaller.PoseOrientation.LeftHand);
    }

    private void Update()
    {
        if (_shouldMove)
        {
            transform.position = Vector3.Lerp(transform.position, _destinationTransform.position, Time.deltaTime * moveSpeed);
            
            if (_movementType == MovementTypeEnum.MovingToPlayer)
            {
                if (Vector3.Distance(transform.position, _destinationTransform.position) < SHOW_DIALOGUE_OPTION_DISTANCE_THRESHOLD)
                {
                    ToggleDialogueOptions(true);
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
        
        ToggleDialogueOptions(false);
    }

    public void CancelMovement(bool hideDialogueOptions)
    {
        _shouldMove = false;
        _movementType = MovementTypeEnum.Idle;
        
        if (hideDialogueOptions)
            ToggleDialogueOptions(false);
    }

    public void SetDialogueOrientation(NpcCaller.PoseOrientation poseOrientation, bool toggleOnOptions = false)
    {
        ToggleDialogueOptions(false);
        
        _orientedDialogueOption = poseOrientation == NpcCaller.PoseOrientation.LeftHand
            ? leftDialogueOptions
            : rightDialogueOptions;

        if (toggleOnOptions)
            ToggleDialogueOptions(true);
    }

    private void ToggleDialogueOptions(bool showDialogue)
    {
        if (_orientedDialogueOption)
            _orientedDialogueOption.SetActive(showDialogue);
    }
}
