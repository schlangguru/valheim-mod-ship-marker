using BepInEx.Configuration;
using UnityEngine;

namespace Vehicle_Map_Marker
{
  public class MarkerType
  {
    public string Prefab { get; }

    public Sprite Sprite { get; }
    public string Name { get; }
    private ConfigEntry<bool> _showConifgEntry { get; }

    public bool Show
    {
      get
      {
        return _showConifgEntry.Value;
      }
    }


    public MarkerType(string prefab, Sprite sprite, string name, ConfigEntry<bool> show)
    {
      this.Prefab = prefab;
      this.Sprite = sprite;
      this.Name = name;
      this._showConifgEntry = show;
    }
  }
}