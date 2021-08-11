using HarmonyLib;
using System.Linq;
using System.Collections.Generic;

namespace VH_Ship_Marker_Mod
{
  class ZDOMan_Patch
  {
    [HarmonyPatch(typeof(ZDOMan), "Load")]
    [HarmonyPostfix]
    private static void Loaded()
    {
      Main._shipMarkers.Clear();
      List<ZDO> shipZDOs = new List<ZDO>();
      foreach (string prefab in Main.shipTypes)
      {
        ZDOMan.instance.GetAllZDOsWithPrefab(prefab, shipZDOs);
        foreach (ZDO zdo in shipZDOs)
        {
          ShipMarkerData markerData = new ShipMarkerData();
          markerData.ZDO = zdo;
          markerData.Type = prefab;
          Main._shipMarkers.Add(zdo.m_uid, markerData);
        }
        shipZDOs.Clear();
      }
    }

    [HarmonyPatch(typeof(ZNetScene), "OnZDODestroyed")]
    [HarmonyPostfix]
    private static void ZDODestroyed(ZDO zdo)
    {
      if (Main._shipMarkers.ContainsKey(zdo.m_uid))
      {
        Main._shipMarkers.Remove(zdo.m_uid);
      }
    }

    [HarmonyPatch(typeof(ZNetScene), "AddInstance")]
    [HarmonyPostfix]
    private static void ZDOAdded(ZDO zdo)
    {
      IEnumerable<int> prefabHashes = Main.shipTypes.Select(str => str.GetStableHashCode());
      if (prefabHashes.Contains(zdo.GetPrefab()))
      {
        ShipMarkerData markerData = new ShipMarkerData();
        markerData.ZDO = zdo;
        Main._shipMarkers.Add(zdo.m_uid, markerData);
      }
    }
  }
}