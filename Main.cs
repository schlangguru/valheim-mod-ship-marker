using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;


namespace VH_Ship_Marker_Mod
{

  [BepInPlugin("de.schlangguru.vh_ship_marker_mod", "Valheim Ship Marker Mod", "1.1.0")]
  [BepInProcess("valheim.exe")]
  public class Main : BaseUnityPlugin
  {

    private Harmony _harmony = null;
    public static Configuration ModConfig;
    public static List<MarkerType> MarkerTypes = new List<MarkerType>();
    public static Dictionary<ZDOID, MarkerData> Markers = new Dictionary<ZDOID, MarkerData>();

    void Awake()
    {
      ModConfig = new Configuration(Config);

      Sprite shipMarkerSprite = LoadSprite("mapicon_anchor.png");
      Sprite cartMarkerSprite = LoadSprite("mapicon_cart.png");

      MarkerTypes.Add(new MarkerType("Raft", shipMarkerSprite,  "$ship_raft", ModConfig.ShowShips));
      MarkerTypes.Add(new MarkerType("Karve", shipMarkerSprite, "$ship_karve", ModConfig.ShowShips));
      MarkerTypes.Add(new MarkerType("VikingShip", shipMarkerSprite, "$ship_longship", ModConfig.ShowShips));
      MarkerTypes.Add(new MarkerType("Cart", cartMarkerSprite, "$tool_cart", ModConfig.ShowCarts));

      _harmony = new Harmony("VH_Ship_Marker_Mod");
      _harmony.PatchAll(typeof(ZDOMan_Patch));
      _harmony.PatchAll(typeof(Minimap_Patch));
    }

    public static Sprite LoadSprite(string name)
    {
      Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("VH_Ship_Marker_Mod.Resources." + name);
      byte[] buffer = new byte[manifestResourceStream.Length];
      manifestResourceStream.Read(buffer, 0, (int)manifestResourceStream.Length);
      // Create a texture. Texture size does not matter, since LoadImage will replace with with incoming image size.
      Texture2D texture = new Texture2D(2, 2);
      texture.LoadImage(buffer);
      texture.Apply();

      return Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.0f, 0.0f), 50f);
    }

    public static void DestroyMarkers()
    {
      foreach (MarkerData data in Markers.Values)
      {
        if (data.Marker != null)
        {
          UnityEngine.Object.Destroy(data.Marker);
        }
      }
      Markers.Clear();
    }

    private void OnDestroy()
    {
      this._harmony?.UnpatchSelf();
      DestroyMarkers();
    }
  }
}
