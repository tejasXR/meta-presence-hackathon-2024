using UnityEngine;

public static class SaveDataManager
{
    private const string GARDEN_DATA_FILENAME = "GardenData.dat";

    public static void SaveGarden(GardenData gardenData)
    {
        if (FileManager.WriteToFile(GARDEN_DATA_FILENAME, gardenData.ToJson()))
        {
            Debug.Log("Save successful");
        }
    }

    public static GardenData LoadGarden()
    {
        if (FileManager.LoadFromFile(GARDEN_DATA_FILENAME, out var json))
        {
            GardenData gardenData = GardenData.LoadFromJson(json);
            return gardenData;
        }

        return null;
    }
}
