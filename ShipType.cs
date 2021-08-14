namespace VH_Ship_Marker_Mod
{

  public class ShipType
  {
    public string Prefab { get; }
    public string Name  { get; }

    public ShipType(string prefab, string name) {
      this.Prefab = prefab;
      this.Name = name;
    }
  }

}