using Meta.XR.MRUtilityKit;
using UnityEngine;

public class SeedSpawner : MonoBehaviour
{
    [SerializeField] private SeedController seedPrefab;
    [SerializeField] private SpawnSurfacesEnum spawnSurfaces;
    
    [Space(5)]
    [SerializeField] private int maxSeedsToSpawn = 10;
    [SerializeField] private float maxHeightToSpawn = 1.5F;

    private Pooler<SeedController> _seedPooler;

    private enum SpawnSurfacesEnum
    {
        Walls,
        AllSurfaces
    }

    public void Initialize()
    {
        _seedPooler = new Pooler<SeedController>();
        _seedPooler.Initialize(OnPoolerBorrowedItem, maxSeedsToSpawn);
        SpawnSeeds();
    }

    private Bounds CreateSpawnBounds()
    {
        Bounds roomBounds = MRUK.Instance.GetCurrentRoom().GetRoomBounds();
        
        // Make the center of the room half the spawn height
        var customCenter = roomBounds.center;
        customCenter.y = maxHeightToSpawn / 2;
        roomBounds.center = customCenter;
        
        // Modify the size of the bounds to reflect max spawn height 
        var customSize = roomBounds.size;
        customSize.y = maxHeightToSpawn;
        roomBounds.size = customSize;

        return roomBounds;
    }

    private void SpawnSeeds()
    {
        // Get spawn position information
        var spawnBounds = CreateSpawnBounds();
        var getSpawnPositions = SpawnUtil.GetSpawnPositions
        (
            objectBounds: spawnBounds,
            positionCount: maxSeedsToSpawn,
            spawnLocation: FindSpawnPositions.SpawnLocation.VerticalSurfaces,
            labels: spawnSurfaces == SpawnSurfacesEnum.Walls ? MRUKAnchor.SceneLabels.WALL_FACE | MRUKAnchor.SceneLabels.WINDOW_FRAME | MRUKAnchor.SceneLabels.WALL_ART : ~(MRUKAnchor.SceneLabels)0,
            surfaceClearanceDistance: .5F
        );

        // Generate pooled objects
        foreach (var tupleVector3Quaternion in getSpawnPositions)
        {
            var pooledSeed = _seedPooler.BorrowItem();
            pooledSeed.transform.SetParent(transform);
            pooledSeed.transform.SetPositionAndRotation(tupleVector3Quaternion.Item1, tupleVector3Quaternion.Item2); 
        }
    }

    private SeedController OnPoolerBorrowedItem(int numberOfTotalItemsBorrowed)
    {
        return Instantiate(seedPrefab);
    }
}
