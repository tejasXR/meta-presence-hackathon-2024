using System;

[Serializable]
public struct IslandData
{
    public Guid Uuid;
    public string Type;

    public override readonly string ToString() => $"{nameof(IslandData)}[ {nameof(Uuid)}={Uuid}, {nameof(Type)}={Type} ]";
}
