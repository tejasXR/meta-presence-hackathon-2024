using System;
using Meta.XR.MRUtilityKit;
using UnityEngine;

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
            PopRandomSeed();
        }
#endif
    }

    public void Initialize()
    {
        _seedPooler.Initialize(OnPoolerBorrowedItem, maxSeedsToSpawn);
        SpawnSeeds();
    }
    
    public void PopRandomSeed()
    {
        if (!enableRandomSeedPopping)
            return;
        
        if (_seedPooler.BorrowedCount == 0)
        {
            Debug.Log($"[{nameof(SeedSpawner)}] {nameof(PopRandomSeed)}: There are no seeds to pop!");
            return;
        }
        _seedPooler.BorrowedObjects[UnityEngine.Random.Range(0, _seedPooler.BorrowedCount - 1)].Pop();
    }

    private void SpawnSeeds()
    {
        // Generate bound information
        var entireRoomBounds = MRUK.Instance.GetCurrentRoom().GetRoomBounds();
        var keyWallAnchor = MRUK.Instance.GetCurrentRoom().GetKeyWall(out Vector2 keyWallScale);
        var keyWallCenter = keyWallAnchor.GetAnchorCenter();
        var keyWallBounds = new Bounds(keyWallCenter, keyWallScale);  
        
        // Get spawn position information
        var getSpawnPositions = SpawnUtil.GetSpawnPositions
        (
            // TEJAS: I original thought that the objectBounds arg was the bounds where to place objects, not the physical bouding dimensions of an object :')
            // Leaving this comment in because my intent was to bound the spawning of objects by specific bounds, which we may do later in development
            // var spawnBounds = spawnSurfaceAccessibility == SpawnSurfaceAccessibilityEnum.SpawnNearAllWalls 
            //    ? GenerateBoundsFromReference(entireRoomBounds) : GenerateBoundsFromReference(keyWallBounds);
            // objectBounds: spawnBounds,
            
            objectBounds: Utilities.GetPrefabBounds(seedPrefab.gameObject),
            positionCount: maxSeedsToSpawn,
            spawnLocation:  spawnSurfaceAccessibility == SpawnSurfaceAccessibilityEnum.VerticalSurfaces ? FindSpawnPositions.SpawnLocation.AnySurface : FindSpawnPositions.SpawnLocation.Floating,
            labels: spawnSurfaces == SpawnSurfacesEnum.Walls ? MRUKAnchor.SceneLabels.WALL_FACE | MRUKAnchor.SceneLabels.WINDOW_FRAME | MRUKAnchor.SceneLabels.WALL_ART : ~(MRUKAnchor.SceneLabels)0,
            surfaceClearanceDistance: SurfaceClearingDistance
        );
      
        // Generate pooled objects
        foreach (var tupleVector3Quaternion in getSpawnPositions)
        {
            var pooledSeed = _seedPooler.BorrowItem();
            pooledSeed.transform.SetParent(transform);
            pooledSeed.transform.position = tupleVector3Quaternion.Item1; 
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

    private SeedController OnPoolerBorrowedItem(int numberOfTotalItemsBorrowed)
    {
        SeedController seed = Instantiate(seedPrefab);
        seed.OnSeedFlung.AddListener(OnSeedFlung);
        seed.OnSeedPopped.AddListener(OnSeedPopped);
        return seed;
    }

    private void OnSeedFlung(SeedController seed)
    {
        Plants.PlantType plant = _gardenManager.GetPlantFrom(seed);
        GameObject plantPrefab = _gardenManager.GetPlantPrefab(plant);
        Tuple<Vector3, Quaternion> validPlantPosition = _gardenManager.GetValidPlantPosition(plantPrefab);

        seed.SetTarget(validPlantPosition.Item1);
    }

    private void OnSeedPopped(SeedController seed)
    {
        _gardenManager.OnSeedPopped(seed);

        // Deactivate and return to pool.
        seed.OnSeedPopped.RemoveListener(OnSeedPopped);
        seed.gameObject.SetActive(false);
        seed.ResetTarget();
        _seedPooler.ReturnItem(seed);
    }
}
