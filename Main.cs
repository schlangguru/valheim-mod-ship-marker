using System.IO;
using BepInEx;
using HarmonyLib;
using System.Collections.Generic;


namespace VH_Ship_Marker_Mod
{

  [BepInPlugin("de.schlangguru.vh_ship_marker_mod", "Valheim Ship Marker Mod", "1.0.0")]
  [BepInProcess("valheim.exe")]
  public class Main : BaseUnityPlugin
  {

    private Harmony _harmony = null;

    public static List<ShipType> ShipTypes = new List<ShipType>() {
      new ShipType("Raft", "$ship_raft"),
      new ShipType("Karve", "$ship_karve"),
      new ShipType("VikingShip", "$ship_longship")
    };
    public static Dictionary<ZDOID, ShipMarkerData> ShipMarkers = new Dictionary<ZDOID, ShipMarkerData>();


    void Awake()
    {
      _harmony = new Harmony("VH_Ship_Marker_Mod");
      _harmony.PatchAll(typeof(ZDOMan_Patch));
      _harmony.PatchAll(typeof(Minimap_Patch));
    }


    private void OnDestroy()
    {
      this._harmony?.UnpatchSelf();
      foreach (ShipMarkerData data in ShipMarkers.Values)
      {
        if (data.Marker != null)
        {
          UnityEngine.Object.Destroy(data.Marker);
        }
      }
      ShipMarkers.Clear();
    }
  }
}
