using Meta.XR.MRUtilityKit;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(GardenManager))]
public class SeedSpawner : MonoBehaviour
{
    [SerializeField] private SeedController seedPrefab;
    [Space(5)]
    [SerializeField] private SpawnSurfacesEnum spawnSurfaces;
    [SerializeField] private SpawnSurfaceAccessibilityEnum spawnSurfaceAccessibility;
    [Space(5)]
    [SerializeField] private int maxSeedsToSpawn = 10;
    [SerializeField] private float maxHeightToSpawn = 1.5F;
    [Space]
    [SerializeField] private bool enableRandomSeedPopping;

    private GardenManager _gardenManager;

    private const float SurfaceClearingDistance = .75F;  // The clearance distance required in front of the surface in order for it to be considered a valid spawn position
    private readonly Pooler<SeedController> _seedPooler = new();

    private enum SpawnSurfacesEnum
    {
        Walls,
        AllSurfaces
    }

    private enum SpawnSurfaceAccessibilityEnum
    {
        Floating,
        VerticalSurfaces
    }

    void Awake()
    {
        _gardenManager = GetComponent<GardenManager>();
    }

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.P))
        {
            PopRandomSeed(force: true);
        }
#endif
    }

    public void PopRandomSeed(bool force = false)
    {
        if (!enableRandomSeedPopping && !force)
        {
            Debug.LogWarning($"Trying to pop random seed but the {nameof(enableRandomSeedPopping)} bool is set to false");
            return;
        }

        // if (_seedPooler.BorrowedCount == 0)
        // {
        //     Debug.Log($"[{nameof(SeedSpawner)}] {nameof(PopRandomSeed)}: There are no seeds to pop!");
        //     return;
        // }

        _seedPooler.BorrowItem().FlungTowardsCeiling();
    }

    public void SpawnSeedsOnRoomWalls()
    {
        // Generate bound information
        // var entireRoomBounds = MRUK.Instance.GetCurrentRoom().GetRoomBounds();
        // var keyWallAnchor = MRUK.Instance.GetCurrentRoom().GetKeyWall(out Vector2 keyWallScale);
        // var keyWallCenter = keyWallAnchor.GetAnchorCenter();
        // var keyWallBounds = new Bounds(keyWallCenter, keyWallScale);

        // Get spawn position information
        var getSpawnPositions = SpawnUtil.GetSpawnPositions
        (
            // TEJAS: I original thought that the objectBounds arg was the bounds where to place objects, not the physical bouding dimensions of an object :')
            // Leaving this comment in because my intent was to bound the spawning of objects by specific bounds, which we may do later in development
            // var spawnBounds = spawnSurfaceAccessibility == SpawnSurfaceAccessibilityEnum.SpawnNearAllWalls 
            //    ? GenerateBoundsFromReference(entireRoomBounds) : GenerateBoundsFromReference(keyWallBounds);
            // objectBounds: spawnBounds,

            objectBounds: Utilities.GetPrefabBounds(seedPrefab.gameObject),
            positionCount: Random.Range(3, maxSeedsToSpawn + 1),
            spawnLocation: spawnSurfaceAccessibility == SpawnSurfaceAccessibilityEnum.VerticalSurfaces ? FindSpawnPositions.SpawnLocation.AnySurface : FindSpawnPositions.SpawnLocation.Floating,
            labels: spawnSurfaces == SpawnSurfacesEnum.Walls ? MRUKAnchor.SceneLabels.WALL_FACE | MRUKAnchor.SceneLabels.WINDOW_FRAME | MRUKAnchor.SceneLabels.WALL_ART : ~(MRUKAnchor.SceneLabels)0,
            surfaceClearanceDistance: SurfaceClearingDistance
        );

        // Generate pooled objects
        if (!_seedPooler.IsInitialized)
        {
            _seedPooler.Initialize(InstantiateNewSeed, maxSeedsToSpawn);
        }
        foreach (var tupleVector3Quaternion in getSpawnPositions)
        {
            var pooledSeed = _seedPooler.BorrowItem();
            pooledSeed.transform.SetParent(transform);
            pooledSeed.transform.position = tupleVector3Quaternion.Item1;
        }
    }

    public void SpawnFullyGrownPlantSeeds(int minNumberOfSeeds, Transform seedSpawnPositionsRoot)
    {
        int numberOfSeeds = Random.Range(minNumberOfSeeds, seedSpawnPositionsRoot.childCount + 1);

        List<Transform> randomSpawnPoints = new();
        List<Transform> allSpawnPoints = new();
        foreach (Transform child in seedSpawnPositionsRoot)
        {
            allSpawnPoints.Add(child);
        }

        for (int i = 0; i < numberOfSeeds; i++)
        {
            int randomIndex = Random.Range(0, allSpawnPoints.Count);
            randomSpawnPoints.Add(allSpawnPoints[randomIndex]);
            allSpawnPoints.RemoveAt(randomIndex);
        }

        if (!_seedPooler.IsInitialized)
        {
            _seedPooler.Initialize(InstantiateNewSeed, maxSeedsToSpawn);
        }
        foreach (Transform selectedTransform in randomSpawnPoints)
        {
            var pooledSeed = _seedPooler.BorrowItem();
            pooledSeed.transform.SetParent(transform);
            pooledSeed.transform.position = selectedTransform.position;
        }
    }

    private Bounds GenerateBoundsFromReference(Bounds referenceBound)
    {
        // Make the center of the room half the spawn height
        var customCenter = referenceBound.center;
        customCenter.y = maxHeightToSpawn / 2;
        referenceBound.center = customCenter;

        // Modify the size of the bounds to reflect max spawn height 
        var customSize = referenceBound.size;
        customSize.y = maxHeightToSpawn;
        referenceBound.size = customSize;

        return referenceBound;
    }

    private SeedController InstantiateNewSeed(int numberOfTotalItemsBorrowed)
    {
        SeedController seed = Instantiate(seedPrefab);
        seed.OnSeedFlung.AddListener(OnSeedFlung);
        seed.OnSeedPopped.AddListener(OnSeedPopped);
        return seed;
    }

    private void OnSeedFlung(SeedController seed)
    {
        if (_gardenManager.TryCreateNewPlant(seed, out Vector3 seedTargetDestination))
        {
            Debug.Log($"[{nameof(SeedSpawner)}] {nameof(OnSeedFlung)}: {nameof(_gardenManager.TryCreateNewPlant)} succeeded, set seed target destination.");
            seed.SetTargetDestination(seedTargetDestination);
        }
        else
        {
            Debug.LogWarning($"[{nameof(SeedSpawner)}] {nameof(OnSeedFlung)}: {nameof(_gardenManager.TryCreateNewPlant)} failed.");
        }
    }

    private void OnSeedPopped(SeedController seed, Vector3 position, bool isIsland)
    {
        _gardenManager.OnSeedPopped(seed, position, isIsland);
        _seedPooler.ReturnItem(seed);
    }

    private Tuple<Vector3, Quaternion> GetValidPositionForPlanting(GameObject plantPrefab)
    {
        Debug.Log($"[{nameof(SeedSpawner)}] {nameof(GetValidPositionForPlanting)}: {nameof(plantPrefab)}={plantPrefab.name}");

        Tuple<Vector3, Quaternion>[] validPositions = SpawnUtil.GetSpawnPositions(
                objectBounds: Utilities.GetPrefabBounds(plantPrefab),
                positionCount: 1,
                spawnLocation: FindSpawnPositions.SpawnLocation.HangingDown,
                labels: MRUKAnchor.SceneLabels.CEILING);

        Debug.Assert(validPositions.Length > 0, $"[{nameof(SeedSpawner)}] {nameof(GetValidPositionForPlanting)} error: invalid {nameof(validPositions)} array.");
        return validPositions[0];
    }
}
