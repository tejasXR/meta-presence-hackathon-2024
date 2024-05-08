using System;

[Serializable]
public struct PlantData
{
    public Guid Uuid;
    public string Type;
    public float Growth;

    public override readonly string ToString() => $"{nameof(PlantData)}[ {nameof(Uuid)}={Uuid}, {nameof(Type)}={Type}, {nameof(Growth)}={Growth} ]";
}
