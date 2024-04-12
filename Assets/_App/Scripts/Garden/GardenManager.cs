using Meta.XR.MRUtilityKit;
using UnityEngine;

public class GardenManager : MonoBehaviour
{
    [SerializeField] private GardenPersistenceManager _persistenceManager;
    [SerializeField] private Plants _plants;

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

    public void OnSeedPopped(SeedController seed)
    {
        Debug.Log($"[{nameof(GardenManager)}] {nameof(OnSeedPopped)}: {nameof(seed)}={seed.gameObject.name}");

        // TODO(yola): Seed > Plant correlation
        Plants.PlantType randomPlantType = (Plants.PlantType)Random.Range(1, System.Enum.GetValues(typeof(Plants.PlantType)).Length);
        GameObject randomPlantPrefab = _plants.GetPrefab(randomPlantType);

        _persistenceManager.CreateNewPlant(randomPlantPrefab, GetValidPlantPosition(randomPlantPrefab));
    }

    private System.Tuple<Vector3, Quaternion> GetValidPlantPosition(GameObject plantPrefab)
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
