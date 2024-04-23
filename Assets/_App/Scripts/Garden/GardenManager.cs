using UnityEngine;

public class GardenManager : MonoBehaviour
{
    [SerializeField] private GardenPersistenceManager _persistenceManager;
    [SerializeField] private Plants _plants;

    void OnApplicationQuit() => _persistenceManager.SaveGardenState();

    public void InitGarden() => _persistenceManager.InitGarden();

    public void DestroyGarden() => _persistenceManager.DestroyGarden();

    public Plants.PlantType GetPlantFrom(SeedController seed)
    {
        // TODO(yola): Seed > Plant correlation
        return (Plants.PlantType)Random.Range(1, System.Enum.GetValues(typeof(Plants.PlantType)).Length);
    }

    public bool TryGetPlantPrefab(Plants.PlantType plant, out GameObject prefab) => _plants.TryGetPrefab(plant, out prefab);

    public void OnSeedPopped(SeedController seed)
    {
        Debug.Log($"[{nameof(GardenManager)}] {nameof(OnSeedPopped)}: {nameof(seed)}={seed.gameObject.name}, {nameof(seed.Plant)}={seed.Plant}, {nameof(seed.PlantTargetPosition)}={seed.PlantTargetPosition}");

        if (_plants.TryGetPrefab(seed.Plant, out GameObject prefab))
        {
            _persistenceManager.CreateNewPlant(prefab, seed.PlantTargetPosition, seed.PlantTargetRotation);
        }
    }
}
