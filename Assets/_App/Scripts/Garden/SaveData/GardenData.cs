using Newtonsoft.Json;
using System;
using System.Collections.Generic;

[Serializable]
public class GardenData
{
    [Serializable]
    public struct PlantData
    {
        public Guid Uuid;
        public string Type;
        public DateTime CreatedAt;
        public float GrowValue;
    }

    public Dictionary<Guid, PlantData> Map = new();
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
}
