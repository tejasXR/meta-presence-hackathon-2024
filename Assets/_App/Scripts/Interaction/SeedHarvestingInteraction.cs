using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeedHarvestingInteraction : MonoBehaviour
{
    [SerializeField] private HandPoseActivator leftFistPoseActivator;
    [SerializeField] private HandPoseActivator rightFistPoseActivator;
    [Space]
    [SerializeField] private LayerMask plantMask;
    [SerializeField] private float raycastDistance = 3F;
    [SerializeField] private float raycastRadius = .1F;

    private void Awake()
    {
        leftFistPoseActivator.PoseActivated += OnPoseActivated;
        rightFistPoseActivator.PoseActivated += OnPoseActivated;
        
        leftFistPoseActivator.PoseDeactivated += OnPoseDeactivated;
        rightFistPoseActivator.PoseDeactivated += OnPoseDeactivated;
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
        DetectPlant(handPosePoint);
    }

    private void OnPoseDeactivated(HandPoseActivator handPoseActivator)
    {
        // Nothing yet
    }

    private void DetectPlant(Transform originTransform)
    {
        // TEJAS: purposefully not using CapsuleCastAllNonAlloc
        var raycastCapsuleHits = Physics.CapsuleCastAll(originTransform.position, originTransform.forward * raycastDistance,
            raycastRadius, originTransform.forward, raycastDistance, plantMask, QueryTriggerInteraction.Collide);

        foreach (var hit in raycastCapsuleHits)
        {
            var hitCol = hit.collider; 
            if (hitCol == null)
                continue;

            hitCol.TryGetComponent(out PlantController plantController);
            if (plantController == null)
                continue;

            if (plantController.IsFullyGrown)
            {
                Debug.Log("After me made our pose, we successfully detected a fully grown plant!");
            }
        }
    }
}
