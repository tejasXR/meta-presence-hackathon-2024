using System.Collections;
using UnityEngine;

public class SeedHarvestingInteraction : MonoBehaviour
{
    [SerializeField] private HandPoseActivator leftFistPoseActivator;
    [SerializeField] private HandPoseActivator rightFistPoseActivator;
    [Space]
    [SerializeField] private LayerMask plantMask;
    [SerializeField] private float raycastDistance = 3F;
    [SerializeField] private float raycastRadius = .1F;
    [Space]
    [Range(0F, 5F)] [SerializeField] private float seedSpawnChargeSpeed = .3F;

    private IEnumerator _chargePlantRoutine;
    private PlantController _detectedPlant;
    private Transform _cameraTransform;


    private void Awake()
    {
        leftFistPoseActivator.PoseActivated += OnPoseActivated;
        rightFistPoseActivator.PoseActivated += OnPoseActivated;
        
        leftFistPoseActivator.PoseDeactivated += OnPoseDeactivated;
        rightFistPoseActivator.PoseDeactivated += OnPoseDeactivated;
        
        _cameraTransform = Camera.main.transform;
    }

    private void OnDestroy()
    {
        if (leftFistPoseActivator)
        {
            leftFistPoseActivator.PoseActivated -= OnPoseActivated;
            leftFistPoseActivator.PoseDeactivated -= OnPoseDeactivated;
        }

        if (rightFistPoseActivator)
        {
            rightFistPoseActivator.PoseActivated -= OnPoseActivated;
            rightFistPoseActivator.PoseDeactivated -= OnPoseDeactivated;
        }
    }

    private void OnPoseActivated(HandPoseActivator handPoseActivator, Transform handPosePoint)
    {
        if (leftFistPoseActivator.PoseActive && rightFistPoseActivator.PoseActive)
        {
            _detectedPlant = null;
            DetectPlant();
        }
    }

    private void OnPoseDeactivated(HandPoseActivator handPoseActivator)
    {
        if (_detectedPlant)
        {
            _detectedPlant.CancelSeedSpawnCharging();
        }
    
        _detectedPlant = null;
        
        if (_chargePlantRoutine != null)
            StopCoroutine(_chargePlantRoutine);
    }

    private void DetectPlant()
    {
        // TEJAS: purposefully not using CapsuleCastAllNonAlloc
        var raycastCapsuleHits = Physics.CapsuleCastAll(_cameraTransform.position, _cameraTransform.forward * raycastDistance,
            raycastRadius, _cameraTransform.forward, raycastDistance, plantMask, QueryTriggerInteraction.Collide);

        foreach (var hit in raycastCapsuleHits)
        {
            var hitCol = hit.collider; 
            if (hitCol == null)
                continue;

            hitCol.TryGetComponent(out PlantController plantController);
            if (plantController == null)
                continue;

            _detectedPlant = plantController;
            _chargePlantRoutine = ChargeUpPlantRoutine(plantController);
            StartCoroutine(_chargePlantRoutine);
        }
    }

    private IEnumerator ChargeUpPlantRoutine(PlantController plantController)
    {
        while (plantController.IsFullyGrown)
        {
            plantController.ChargeUpSeedSpawn(Time.deltaTime * seedSpawnChargeSpeed);
            yield return new WaitForEndOfFrame();
        }
    }
}
