using Meta.XR.MRUtilityKit;
using UnityEngine;

public class GardenManager : MonoBehaviour
{
    [SerializeField] private GardenPersistenceManager _persistenceManager;
    [SerializeField] private Plants plants;

    void OnApplicationQuit()
    {
        Debug.Log($"[{nameof(GardenManager)}] {nameof(OnApplicationQuit)}");

        _persistenceManager.SaveGardenState();
    }

    public void InitGarden()
    {
        Debug.Log($"[{nameof(GardenManager)}] {nameof(InitGarden)}");

        _persistenceManager.InitGarden();
    }

    public void DestroyGarden()
    {
        Debug.Log($"[{nameof(GardenManager)}] {nameof(DestroyGarden)}");

        _persistenceManager.DestroyGarden();
    }

    public Plants.PlantType GetPlantFrom(SeedController seed)
    {
        // TODO(yola): Seed > Plant correlation
        return (Plants.PlantType)Random.Range(1, System.Enum.GetValues(typeof(Plants.PlantType)).Length);
    }

    public GameObject GetPlantPrefab(Plants.PlantType plant) => plants.GetPrefab(plant);

    public void OnSeedPopped(SeedController seed)
    {
        Debug.Log($"[{nameof(GardenManager)}] {nameof(OnSeedPopped)}: {nameof(seed)}={seed.gameObject.name}, {nameof(seed.Plant)}={seed.Plant}, {nameof(seed.PlantTargetPosition)}={seed.PlantTargetPosition}");
        _persistenceManager.CreateNewPlant(
            GetPlantPrefab(seed.Plant),
            seed.PlantTargetPosition,
            seed.PlantTargetRotation);
    }

    public static System.Tuple<Vector3, Quaternion> GetValidPlantPosition(GameObject plantPrefab)
    {
        Debug.Log($"[{nameof(GardenManager)}] {nameof(GetValidPlantPosition)}: {nameof(plantPrefab)}={plantPrefab.name}");

        System.Tuple<Vector3, Quaternion>[] validPositions = SpawnUtil.GetSpawnPositions(
                objectBounds: Utilities.GetPrefabBounds(plantPrefab),
                positionCount: 1,
                spawnLocation: FindSpawnPositions.SpawnLocation.HangingDown,
                labels: MRUKAnchor.SceneLabels.CEILING);

        Debug.Assert(validPositions.Length > 0, $"[{nameof(GardenManager)}] {nameof(GetValidPlantPosition)} error: invalid {nameof(validPositions)} array.");
        return validPositions[0];
    }
}
