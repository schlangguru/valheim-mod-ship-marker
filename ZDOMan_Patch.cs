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
      Main.ShipMarkers.Clear();
      List<ZDO> shipZDOs = new List<ZDO>();
      foreach (ShipType shipType in Main.ShipTypes)
      {
        ZDOMan.instance.GetAllZDOsWithPrefab(shipType.Prefab, shipZDOs);
        foreach (ZDO zdo in shipZDOs)
        {
          ShipMarkerData markerData = new ShipMarkerData();
          markerData.ZDO = zdo;
          markerData.Type = shipType;
          Main.ShipMarkers.Add(zdo.m_uid, markerData);
        }
        shipZDOs.Clear();
      }
    }

    [HarmonyPatch(typeof(ZNetScene), "OnZDODestroyed")]
    [HarmonyPrefix]
    private static void ZDODestroyed(ZDO zdo)
    {
      if (Main.ShipMarkers.ContainsKey(zdo.m_uid))
      {
        ShipMarkerData data = Main.ShipMarkers[zdo.m_uid];
        data.ZDO = null;
        data.Type = null;
        if (data.Marker != null) {
          UnityEngine.Object.Destroy(data.Marker);
        }
        Main.ShipMarkers.Remove(zdo.m_uid);
      }
    }

    [HarmonyPatch(typeof(ZNetScene), "AddInstance")]
    [HarmonyPostfix]
    private static void ZDOAdded(ZDO zdo)
    {
      foreach (ShipType type in Main.ShipTypes)
      {
        if (zdo.GetPrefab() == type.Prefab.GetStableHashCode() && !Main.ShipMarkers.ContainsKey(zdo.m_uid))
        {
          ShipMarkerData markerData = new ShipMarkerData();
          markerData.ZDO = zdo;
          markerData.Type = type;
          Main.ShipMarkers.Add(zdo.m_uid, markerData);
          break;
        }
      }
    }
  }
}