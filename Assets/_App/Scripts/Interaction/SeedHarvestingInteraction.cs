using System.Collections;
using UnityEngine;

public class SeedHarvestingInteraction : MonoBehaviour
{
    [SerializeField] private HandPoseActivator bothPalmsAwayPoseActivator;
    [Space]
    [SerializeField] private LayerMask plantMask;
    [SerializeField] private float raycastDistance = 3F;
    [SerializeField] private float raycastRadius = .1F;
    [Space]

    private IEnumerator _chargePlantRoutine;
    private IEnumerator _cancelChargePlantRoutine;
    private PlantController _detectedPlant;
    private Transform _cameraTransform;

    private void Awake()
    {
        bothPalmsAwayPoseActivator.PoseActivated += OnPoseActivated;
        
        bothPalmsAwayPoseActivator.PoseDeactivated += OnPoseDeactivated;
        
        _cameraTransform = Camera.main.transform;
    }

    private void OnDestroy()
    {
        if (bothPalmsAwayPoseActivator)
        {
            bothPalmsAwayPoseActivator.PoseActivated -= OnPoseActivated;
            bothPalmsAwayPoseActivator.PoseDeactivated -= OnPoseDeactivated;
        }
    }

    private void OnPoseActivated(HandPoseActivator handPoseActivator, Transform handPosePoint)
    {
        _detectedPlant = null;
        DetectPlant();
    }

    private void OnPoseDeactivated(HandPoseActivator handPoseActivator)
    {
        if (_chargePlantRoutine != null)
            StopCoroutine(_chargePlantRoutine);
        
        if (_detectedPlant)
        {
            _cancelChargePlantRoutine = _detectedPlant.CancelSeedSpawnCharging();
            StartCoroutine(_cancelChargePlantRoutine);
        }
    
        _detectedPlant = null;
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

            if (_cancelChargePlantRoutine != null)
                StopCoroutine(_cancelChargePlantRoutine);
            
            _detectedPlant = plantController;
            _chargePlantRoutine = plantController.ChargeUpPlantForSeedSpawn();
            StartCoroutine(_chargePlantRoutine);
        }
    }
}
