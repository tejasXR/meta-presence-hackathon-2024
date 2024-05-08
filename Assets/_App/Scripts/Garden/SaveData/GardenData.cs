using Newtonsoft.Json;
using System;
using System.Collections.Generic;

[Serializable]
public class GardenData
{
    public Dictionary<Guid, PlantData> PlantMap = new();
    public Dictionary<Guid, IslandData> IslandMap = new();
    public string DateTimeOfLastVisit;

    public TimeSpan? GetTimeSinceLastVisit()
    {
        DateTime.TryParse(DateTimeOfLastVisit, out DateTime parsedDateTime);
        if (parsedDateTime == default)
            return null;

        return DateTime.Now - parsedDateTime;
    }

    public string ToJson()
    {
        return JsonConvert.SerializeObject(this);
    }

    public static GardenData LoadFromJson(string json)
    {
        return JsonConvert.DeserializeObject<GardenData>(json);
    }

    public override string ToString()
    {
        return $"Last garden visit was {GetTimeSinceLastVisit()?.ToString(@"d\.hh\:mm\:ss") ?? "N/A"}";
    }
}
