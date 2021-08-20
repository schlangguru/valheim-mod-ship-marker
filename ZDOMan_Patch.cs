using HarmonyLib;
using System.Collections.Generic;

namespace Vehicle_Map_Marker
{
  class ZDOMan_Patch
  {
    [HarmonyPatch(typeof(ZDOMan), "Load")]
    [HarmonyPostfix]
    private static void Loaded()
    {
      Main.DestroyMarkers();
      List<ZDO> zdos = new List<ZDO>();
      foreach (MarkerType markerType in Main.MarkerTypes)
      {
        ZDOMan.instance.GetAllZDOsWithPrefab(markerType.Prefab, zdos);
        foreach (ZDO zdo in zdos)
        {
          MarkerData markerData = new MarkerData();
          markerData.ZDO = zdo;
          markerData.Type = markerType;
          Main.Markers.Add(zdo.m_uid, markerData);
        }
        zdos.Clear();
      }
    }

    [HarmonyPatch(typeof(ZNetScene), "OnZDODestroyed")]
    [HarmonyPrefix]
    private static void ZDODestroyed(ZDO zdo)
    {
      if (Main.Markers.ContainsKey(zdo.m_uid))
      {
        MarkerData data = Main.Markers[zdo.m_uid];
        data.ZDO = null;
        data.Type = null;
        if (data.Marker != null) {
          UnityEngine.Object.Destroy(data.Marker);
        }
        Main.Markers.Remove(zdo.m_uid);
      }
    }

    [HarmonyPatch(typeof(ZNetScene), "AddInstance")]
    [HarmonyPostfix]
    private static void ZDOAdded(ZDO zdo)
    {
      foreach (MarkerType type in Main.MarkerTypes)
      {
        if (zdo.GetPrefab() == type.Prefab.GetStableHashCode() && !Main.Markers.ContainsKey(zdo.m_uid))
        {
          MarkerData markerData = new MarkerData();
          markerData.ZDO = zdo;
          markerData.Type = type;
          Main.Markers.Add(zdo.m_uid, markerData);
          break;
        }
      }
    }
  }
}