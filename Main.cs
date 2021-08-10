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

    public static List<string> shipTypes = new List<string>() {"Raft", "Karve", "Longship"};
    public static Dictionary<ZDOID, ShipMarkerData> _shipMarkers = new Dictionary<ZDOID, ShipMarkerData>();


    void Awake()
    {
      _harmony = new Harmony("VH_Ship_Marker_Mod");
      _harmony.PatchAll(typeof(ZDOMan_Patch));
      _harmony.PatchAll(typeof(Minimap_Patch));
    }

    public static void Log(string msg)
    {
      StreamWriter file = new StreamWriter("E:/Temp/log.txt", append: true);
      file.WriteLine(msg);
      file.Close();
    }

    private void OnDestroy()
    {
      this._harmony?.UnpatchSelf();
      foreach (ShipMarkerData data in _shipMarkers.Values)
      {
        if (data.Marker != null)
        {
          UnityEngine.Object.Destroy(data.Marker);
        }
      }
      _shipMarkers.Clear();
    }
  }
}
