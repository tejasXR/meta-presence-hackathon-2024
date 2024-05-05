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
                Debug.LogError($"[{nameof(GardenManager)}] {nameof(TryCreateNewPlant)} failed: couldn't find a valid spawn point for {nameof(plantPrefab)}={plantPrefab.name}");
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

    public void OnSeedPoppedOnTheCeiling(SeedController seed, Vector3 position)
    {
        if (_plantingMap.TryGetValue(seed.Uuid, out Planting planting) && planting.IsValid)
        {
            Islands.IslandType randomIslandType = (Islands.IslandType)Random.Range(1, Enum.GetValues(typeof(Islands.IslandType)).Length);
            if (_islands.TryGetPrefab(randomIslandType, out GameObject islandPrefab))
            {
                Debug.Log($"[{nameof(GardenManager)}] {nameof(OnSeedPoppedOnTheCeiling)}: Spawn a new island!");

                SpawnNewIsland(islandPrefab, position, planting.PlantSpawnRotation);

                _newPlantQueue.Enqueue(planting);
                _plantingMap.Remove(seed.Uuid);
            }
        }
        else Debug.LogError($"[{nameof(GardenManager)}] {nameof(OnSeedPoppedOnTheCeiling)} but something went wrong.");
    }

    public void OnSeedPoppedOnIsland(SeedController seed, Vector3 position, Vector3 normal)
    {
        if (_plantingMap.TryGetValue(seed.Uuid, out Planting planting) && planting.IsValid)
        {
            SpawnNewPlant(planting.PlantPrefab, position, planting.PlantSpawnRotation, normal);
        }
        else Debug.LogError($"[{nameof(GardenManager)}] {nameof(OnSeedPoppedOnIsland)}: but something went wrong.");
    }

    public void OnSeedCombined(SeedController seed)
    {
        if (_plantingMap.TryGetValue(seed.Uuid, out Planting planting) && planting.IsValid)
        {
            _plantingMap.Remove(seed.Uuid);
        }
    }

    public void OnNewIslandCreated(IslandData _, IslandController island)
    {
        if (_newPlantQueue.Count > 0)
        {
            Planting planting = _newPlantQueue.Dequeue();
            island.StartEmerging(() => SpawnNewPlant(planting.PlantPrefab, island.OriginSpawnPoint.position, planting.PlantSpawnRotation, Vector3.down));
        }
        else Debug.LogError($"[{nameof(GardenManager)}] {nameof(OnNewIslandCreated)} Island spawned successfully, but planting queue is empty!");
    }

    public void OnIslandLoaded(IslandData _, IslandController island)
    {
        island.SetEmerged();
    }

    public void OnNewPlantCreated(PlantData plantData, PlantController plant)
    {
        plant.SeedSpawningTriggered.AddListener(OnPlantHarvested);
        plant.StartGrowing();
        plant.SetSpatialAnchorId(plantData.Uuid);
    }

    public void OnPlantLoaded(PlantData data, PlantController plant)
    {
        plant.SeedSpawningTriggered.AddListener(OnPlantHarvested);
        plant.ResumeGrowing(data.Growth, _persistenceManager.TimeSinceLastGardenVisit);
    }

    private void OnPlantHarvested(PlantController plant)
    {
        _seedSpawner.SpawnFullyGrownPlantSeeds(plant.MinLoot, plant.LootSpawnPointsRoot);
        _persistenceManager.DestroyPlant(plant.SpatialAnchorUuid);
    }

    private Tuple<Vector3, Quaternion> GetValidPlantSpawnPoint(GameObject plantPrefab)
    {
        Tuple<Vector3, Quaternion>[] validPositions = SpawnUtil.GetSpawnPositions(
            objectBounds: Utilities.GetPrefabBounds(plantPrefab),
            positionCount: 1,
            spawnLocation: FindSpawnPositions.SpawnLocation.HangingDown,
            labels: MRUKAnchor.SceneLabels.CEILING,
            layerMask: _islandLayerMask); // Ignore island collisions because we want to be able to spawn plants on islands.

        Debug.Assert(validPositions.Length > 0, $"[{nameof(GardenDataManager)}] {nameof(GetValidPlantSpawnPoint)} error: invalid {nameof(validPositions)} array.");
        return validPositions[0];
    }

    private Plants.PlantType GetPlantFrom(SeedController _)
    {
        // TODO(yola): Seed > Plant correlation
        return (Plants.PlantType)Random.Range(1, Enum.GetValues(typeof(Plants.PlantType)).Length);
    }

    private void SpawnNewIsland(GameObject prefab, Vector3 spawnPosition, Quaternion spawnRotation)
    {
        Quaternion randomYAxisRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        _persistenceManager.CreateNewIsland(prefab, spawnPosition, randomYAxisRotation * spawnRotation);
    }

    private void SpawnNewPlant(GameObject prefab, Vector3 spawnPosition, Quaternion spawnRotation, Vector3 spawnSurfaceNormal)
    {
        Quaternion randomYAxisRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        _persistenceManager.CreateNewPlant(prefab, spawnPosition, /* Quaternion.FromToRotation(Vector3.up, -spawnSurfaceNormal.normalized) * */randomYAxisRotation * spawnRotation);
    }
}
