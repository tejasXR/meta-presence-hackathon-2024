using Meta.XR.MRUtilityKit;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class GardenManager : MonoBehaviour
{
    [SerializeField] private GardenPersistenceManager _persistenceManager;
    [SerializeField] private SeedSpawner _seedSpawner;
    [SerializeField] private Islands _islands;
    [SerializeField] private Plants _plants;

    void OnApplicationQuit() => _persistenceManager.SaveGardenState();

    public void Initialize() => _persistenceManager.InitGarden();

    public void DestroyGarden() => _persistenceManager.DestroyGarden();

    public SeedController.Target GetSeedTarget(SeedController seed)
    {
        // TODO(yola): Implement logic to select existing island or create new one based on probability.
        Islands.IslandType randomIslandType = (Islands.IslandType)Random.Range(1, Enum.GetValues(typeof(Islands.IslandType)).Length);
        if (_islands.TryGetPrefab(randomIslandType, out GameObject islandPrefab))
        {
            if (_plants.TryGetPrefab(GetPlantFrom(seed), out GameObject plantPrefab))
            {
                Tuple<Vector3, Quaternion> islandPosition = GetValidPlantPosition(plantPrefab);
                return new()
                {
                    Island = islandPrefab,
                    InstantiateIsland = true,
                    Plant = plantPrefab,
                    Position = islandPosition.Item1,
                    Rotation = islandPosition.Item2
                };
            }
        }
        return SeedController.Target.Invalid;
    }

    public bool TryGetPlantPrefab(Plants.PlantType plant, out GameObject prefab) => _plants.TryGetPrefab(plant, out prefab);

    public void OnSeedPopped(SeedController seed)
    {
        if (seed.CurrentTarget.IsValid)
        {
            Quaternion randomYAxisRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            if (seed.CurrentTarget.InstantiateIsland)
            {
                _persistenceManager.CreateNewIsland(seed.CurrentTarget.Island, seed.CurrentTarget.Position, randomYAxisRotation * seed.CurrentTarget.Rotation);
            }

            _persistenceManager.CreateNewPlant(seed.CurrentTarget.Plant, seed.CurrentTarget.Position, randomYAxisRotation * seed.CurrentTarget.Rotation);
        }
    }

    public void OnNewIslandCreated(IslandData _, IslandController __)
    {
        //controller.SeedSpawningTriggered.AddListener(OnPlantFullyGrown);
        //controller.StartGrowing();
    }

    public void OnIslandLoaded(IslandData _, IslandController __)
    {
        // controller.SeedSpawningTriggered.AddListener(OnPlantFullyGrown);
        // controller.ResumeGrowing(data.Growth, _persistenceManager.TimeSinceLastGardenVisit);
    }

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

    private Tuple<Vector3, Quaternion> GetValidPlantPosition(GameObject prefab)
    {
        Tuple<Vector3, Quaternion>[] validPositions = SpawnUtil.GetSpawnPositions(
            objectBounds: Utilities.GetPrefabBounds(prefab),
            positionCount: 1,
            spawnLocation: FindSpawnPositions.SpawnLocation.HangingDown,
            labels: MRUKAnchor.SceneLabels.CEILING);

        Debug.Assert(validPositions.Length > 0, $"[{nameof(GardenDataManager)}] {nameof(GetValidPlantPosition)} error: invalid {nameof(validPositions)} array.");
        return validPositions[0];
    }

    private Plants.PlantType GetPlantFrom(SeedController seed)
    {
        // TODO(yola): Seed > Plant correlation
        return (Plants.PlantType)Random.Range(1, Enum.GetValues(typeof(Plants.PlantType)).Length);
    }
}
