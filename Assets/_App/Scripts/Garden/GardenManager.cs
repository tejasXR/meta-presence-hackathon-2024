using Meta.XR.MRUtilityKit;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public struct Planting
{
    public SeedController Seed;
    public GameObject PlantPrefab;
    public Vector3 PlantSpawnPosition;
    public Quaternion PlantSpawnRotation;

    public readonly bool IsValid => PlantPrefab != null && PlantSpawnPosition.IsValid();
    public static Planting Invalid => new() { PlantSpawnPosition = Vector3.negativeInfinity };

    public override readonly string ToString() => $"{nameof(Planting)}[ {nameof(Seed.Uuid)}={Seed.Uuid}, {nameof(PlantPrefab)}={PlantPrefab.name}, {nameof(PlantSpawnPosition)}={PlantSpawnPosition}, {nameof(PlantSpawnRotation)}={PlantSpawnRotation} ]";
}

public class GardenManager : MonoBehaviour
{
    [SerializeField] private GardenPersistenceManager _persistenceManager;
    [SerializeField] private SeedSpawner _seedSpawner;
    [SerializeField] private Islands _islands;
    [SerializeField] private Plants _plants;
    [SerializeField] private LayerMask _islandLayerMask;

    private readonly Dictionary<Guid, Planting> _plantingMap = new();
    private readonly Queue<Planting> _newPlantQueue = new();

    void OnApplicationQuit() => _persistenceManager.SaveGardenState();

    public void Initialize() => _persistenceManager.InitGarden();

    public void DestroyGarden() => _persistenceManager.DestroyGarden();

    public bool TryCreateNewPlant(SeedController seed, out Vector3 seedTargetDestination)
    {
        seedTargetDestination = Vector3.negativeInfinity;
        Planting planting = Planting.Invalid;

        if (_plants.TryGetPrefab(GetPlantFrom(seed), out GameObject plantPrefab))
        {
            Tuple<Vector3, Quaternion> validPlantSpawnPoint = GetValidPlantSpawnPoint(plantPrefab);
            if (validPlantSpawnPoint == null)
            {
                Debug.LogError($"[{nameof(GardenManager)}] {nameof(TryCreateNewPlant)} failed: couldn't find a valid spawn point.");
                return false;
            }

            seedTargetDestination = validPlantSpawnPoint.Item1;

            planting = new()
            {
                Seed = seed,
                PlantPrefab = plantPrefab,
                PlantSpawnPosition = validPlantSpawnPoint.Item1,
                PlantSpawnRotation = validPlantSpawnPoint.Item2,
            };
            _plantingMap[seed.Uuid] = planting;

            Debug.Log($"[{nameof(GardenManager)}] {nameof(TryCreateNewPlant)} New {nameof(Planting)} created: {planting}");
        }
        return planting.IsValid;
    }

    public void OnSeedPopped(SeedController seed, Vector3 position, bool isIsland)
    {
        if (_plantingMap.TryGetValue(seed.Uuid, out Planting planting))
        {
            if (planting.IsValid)
            {
                Quaternion randomYAxisRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

                if (isIsland)
                {
                    Debug.Log($"[{nameof(GardenManager)}] {nameof(OnSeedPopped)} Seed popped on an existing island!");
                    _persistenceManager.CreateNewPlant(planting.PlantPrefab, position, randomYAxisRotation * planting.PlantSpawnRotation);
                    return;
                }

                Islands.IslandType randomIslandType = (Islands.IslandType)Random.Range(1, Enum.GetValues(typeof(Islands.IslandType)).Length);
                if (_islands.TryGetPrefab(randomIslandType, out GameObject islandPrefab))
                {
                    Debug.Log($"[{nameof(GardenManager)}] {nameof(OnSeedPopped)} Island not found! Spawn a new one: {nameof(islandPrefab)}={islandPrefab.name}");
                    _persistenceManager.CreateNewIsland(islandPrefab, position, randomYAxisRotation * planting.PlantSpawnRotation);

                    _plantingMap.Remove(seed.Uuid);
                    _newPlantQueue.Enqueue(planting);
                    return;
                }
            }
        }

        Debug.LogError($"[{nameof(GardenManager)}] {nameof(OnSeedPopped)} error.");
    }

    public void OnNewIslandCreated(IslandData _, IslandController controller)
    {
        if (_newPlantQueue.Count > 0)
        {
            Vector3 raycastOrigin = new(controller.transform.position.x, 0f, controller.transform.position.z);

            if (Physics.Raycast(raycastOrigin, Vector3.up, out RaycastHit hit, Mathf.Infinity, ~0 & ~_islandLayerMask)) // Ignore everything expect islands.
            {
                if (hit.collider.CompareTag("Island"))
                {
                    Planting planting = _newPlantQueue.Dequeue();
                    Quaternion randomYAxisRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                    _persistenceManager.CreateNewPlant(planting.PlantPrefab, hit.point, randomYAxisRotation * planting.PlantSpawnRotation);
                }
            }
            else Debug.LogError($"[{nameof(GardenManager)}] {nameof(OnNewIslandCreated)} Island spawned successfully, but raycast failed!");
        }
        else Debug.LogError($"[{nameof(GardenManager)}] {nameof(OnNewIslandCreated)} Island spawned successfully, but planting queue is empty!");
    }

    public void OnIslandLoaded(IslandData _, IslandController __) { }

    public void OnNewPlantCreated(PlantData _, PlantController controller)
    {
        controller.SeedSpawningTriggered.AddListener(OnPlantFullyGrown);
        controller.StartGrowing();
    }

    public void OnPlantLoaded(PlantData data, PlantController controller)
    {
        controller.SeedSpawningTriggered.AddListener(OnPlantFullyGrown);
        controller.ResumeGrowing(data.Growth, _persistenceManager.TimeSinceLastGardenVisit);
    }

    private void OnPlantFullyGrown(PlantController plant)
    {
        _seedSpawner.SpawnFullyGrownPlantSeeds(plant.MinLoot, plant.LootSpawnPointsRoot);
    }

    private Tuple<Vector3, Quaternion> GetValidPlantSpawnPoint(GameObject plantPrefab)
    {
        Tuple<Vector3, Quaternion>[] validPositions = SpawnUtil.GetSpawnPositions(
            objectBounds: Utilities.GetPrefabBounds(plantPrefab),
            positionCount: 1,
            spawnLocation: FindSpawnPositions.SpawnLocation.HangingDown,
            labels: MRUKAnchor.SceneLabels.CEILING,
            layerMask: _islandLayerMask);

        Debug.Assert(validPositions.Length > 0, $"[{nameof(GardenDataManager)}] {nameof(GetValidPlantSpawnPoint)} error: invalid {nameof(validPositions)} array.");
        return validPositions[0];
    }

    private Plants.PlantType GetPlantFrom(SeedController seed)
    {
        // TODO(yola): Seed > Plant correlation
        return (Plants.PlantType)Random.Range(1, Enum.GetValues(typeof(Plants.PlantType)).Length);
    }
}
