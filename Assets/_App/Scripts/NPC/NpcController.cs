using System;
using System.Collections;
using UnityEngine;

public class NpcController : MonoBehaviour
{
    public event Action NpcSummoned;
    public event Action NpcNotSummoned;
    
    [SerializeField] private NpcOptionsMenu optionsMenu;
    [SerializeField] private float moveSpeed;
    [SerializeField] private ParticleSystem npcParticles;
    [SerializeField] private AudioSource npcAudioSource;

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
    private IEnumerator _npcAudioFadeRoutine;
    private float _npcAudioVolume;

    private void Awake()
    {
        _mainCamera = Camera.main;
        
        _npcAudioVolume = npcAudioSource.volume;
        npcAudioSource.volume = 0;
        npcAudioSource.Play();
        
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
        
        npcParticles.Play();

        SetNPCAudioVolume(_npcAudioVolume);
        
        optionsMenu.Hide();
    }

    public void CancelMovement(bool hideDialogueOptions)
    {
        _shouldMove = false;
        _movementType = MovementTypeEnum.Idle;
        
        npcParticles.Stop();

        SetNPCAudioVolume(0);
        
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

    private void SetNPCAudioVolume(float volume)
    {
        if (_npcAudioFadeRoutine != null)
            StopCoroutine(_npcAudioFadeRoutine);

        _npcAudioFadeRoutine = AudioUtils.FadeToVolume(npcAudioSource, volume, 1F);
        StartCoroutine(_npcAudioFadeRoutine);
    }
}


