using System;

[Serializable]
public struct PlantData
{
    public Guid Uuid;
    public string Type;
    public DateTime CreatedAt;
    public float GrowValue;
}
