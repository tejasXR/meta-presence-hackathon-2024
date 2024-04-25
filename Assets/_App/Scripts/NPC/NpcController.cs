using UnityEngine;

public class NpcController : MonoBehaviour
{
    [SerializeField] private GameObject dialogueOptions;
    [SerializeField] private float moveSpeed;
    
    public enum MovementTypeEnum
    {
        Idle,
        MovingRandomly,
        MovingToPlayer
    }

    private const float MOVEMENT_THRESHOLD = .001F;
    
    private MovementTypeEnum _movementType;
    private Vector3 _destinationPosition;
    private Quaternion _destinationRotation;
    private bool _shouldMove;

    private void Update()
    {
        if (_shouldMove)
        {
            transform.position = Vector3.Lerp(transform.position, _destinationPosition, Time.deltaTime * moveSpeed);
            if (Vector3.Distance(transform.position, _destinationPosition) < MOVEMENT_THRESHOLD)
            {
                if (_movementType == MovementTypeEnum.MovingToPlayer)
                    ToggleDialogueOptions(true);
                
                CancelMovement(false);
            }
        }
        
        transform.rotation = Quaternion.Lerp(transform.rotation, _destinationRotation, Time.deltaTime * moveSpeed);
    }

    public void MoveToPoint(Transform destPoint, MovementTypeEnum movementType)
    {
        _shouldMove = true;
        _destinationPosition = destPoint.position;
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

    private void ToggleDialogueOptions(bool showDialogue)
    {
        dialogueOptions.SetActive(showDialogue);
    }
}
