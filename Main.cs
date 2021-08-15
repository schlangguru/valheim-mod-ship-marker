using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;


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

    public static Sprite ShipMarkerSprite;


    void Awake()
    {
      ShipMarkerSprite = LoadShipMarkerSprite();
      _harmony = new Harmony("VH_Ship_Marker_Mod");
      _harmony.PatchAll(typeof(ZDOMan_Patch));
      _harmony.PatchAll(typeof(Minimap_Patch));
    }


    public Sprite LoadShipMarkerSprite()
    {
      Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("VH_Ship_Marker_Mod.Resources.mapicon_anchor.png");
      byte[] buffer = new byte[manifestResourceStream.Length];
      manifestResourceStream.Read(buffer, 0, (int)manifestResourceStream.Length);
      // Create a texture. Texture size does not matter, since LoadImage will replace with with incoming image size.
      Texture2D texture = new Texture2D(2, 2);
      texture.LoadImage(buffer);
      texture.Apply();

      return Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.0f, 0.0f), 50f);
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
