using System.IO;
using UnityEngine;

public static class GardenDataManager
{
    private static readonly string APP_DATA_PATH = Application.persistentDataPath;
    private const string GARDEN_DATA_FILENAME = "GardenData.dat";

    public static void SaveGarden(GardenData gardenData)
    {
        if (FileManager.TryWriteToFile(Path.Combine(APP_DATA_PATH, GARDEN_DATA_FILENAME), gardenData.ToJson()))
        {
            Debug.Log("Save successful");
        }
    }

    public static GardenData LoadGarden()
    {
        if (FileManager.TryLoadFromFile(Path.Combine(APP_DATA_PATH, GARDEN_DATA_FILENAME), out var json))
        {
            GardenData gardenData = GardenData.LoadFromJson(json);
            return gardenData;
        }

        return null;
    }
}
