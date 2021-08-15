using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace VH_Ship_Marker_Mod
{
  class Minimap_Patch
  {

    [HarmonyPatch(typeof(Minimap), "UpdatePins")]
    [HarmonyPostfix]
    private static void UpdatePins(float ___m_largeZoom)
    {
      if (Main.ShipMarkers.Count > 0)
      {
        RawImage rawImage = Minimap.instance.m_largeRoot.activeSelf ? Minimap.instance.m_mapImageLarge : Minimap.instance.m_mapImageSmall;
        float markerSize = Minimap.instance.m_largeRoot.activeSelf ? Minimap.instance.m_pinSizeLarge : Minimap.instance.m_pinSizeSmall;
        RectTransform rectTransform = Minimap.instance.m_largeRoot.activeSelf ? Minimap.instance.m_pinRootLarge : Minimap.instance.m_pinRootSmall;
        foreach (ShipMarkerData data in Main.ShipMarkers.Values)
        {
          Vector3 shipPos = data.ZDO.GetPosition();
          if (IsPointVisible(shipPos, rawImage) && !IsShipControlledByPlayer(data.ZDO))
          {
            DrawShipMarker(data, markerSize, rectTransform, rawImage, ___m_largeZoom);
          }
          else
          {
            if (data.Marker != null)
            {
              UnityEngine.Object.Destroy(data.Marker);
            }
          }
        }
      }
    }

    private static bool IsShipControlledByPlayer(ZDO zdo)
    {
      ZNetView zNetView = ZNetScene.instance.FindInstance(zdo);
      if (zNetView != null)
      {
        GameObject gameObject = zNetView.gameObject;
        if (gameObject != null)
        {
          Ship ship = gameObject.GetComponent<Ship>();
          if (ship != null)
          {
            return ship.HaveControllingPlayer();
          }
        }
      }

      return false;
    }

    private static void DrawShipMarker(ShipMarkerData data, float size, RectTransform parent, RawImage rawImage, float largeMapZoom)
    {
      GameObject gameObject = data.Marker;
      if (gameObject == null || gameObject.transform.parent != parent)
      {
        if (gameObject != null)
        {
          UnityEngine.Object.Destroy(gameObject);
        }

        gameObject = UnityEngine.Object.Instantiate<GameObject>(Minimap.instance.m_pinPrefab);
        data.Marker = gameObject;
        gameObject.GetComponent<Image>().sprite = Main.ShipMarkerSprite;
        gameObject.transform.SetParent(parent);
        (gameObject.transform as RectTransform).SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
        (gameObject.transform as RectTransform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
      }
      float mx;
      float my;
      WorldToMapPoint(data.ZDO.GetPosition(), out mx, out my);
      Vector2 anchoredPosition = MapPointToLocalGuiPos(mx, my, rawImage);
      (gameObject.transform as RectTransform).anchoredPosition = anchoredPosition;
      gameObject.transform.Find("Checked").gameObject.SetActive(false);
      Text text = gameObject.transform.Find("Name").GetComponent<Text>();

      if (Minimap.instance.m_largeRoot.activeSelf && largeMapZoom < Minimap.instance.m_showNamesZoom)
      {
        text.gameObject.SetActive(true);
        text.text = Localization.instance.Localize(data.Type.Name);
      }
      else
      {
        text.gameObject.SetActive(false);
      }
    }

    private static bool IsPointVisible(Vector3 p, RawImage map)
    {
      float num;
      float num2;
      WorldToMapPoint(p, out num, out num2);
      return num > map.uvRect.xMin && num < map.uvRect.xMax && num2 > map.uvRect.yMin && num2 < map.uvRect.yMax;
    }

    private static void WorldToMapPoint(Vector3 p, out float mx, out float my)
    {
      int num = Minimap.instance.m_textureSize / 2;
      mx = p.x / Minimap.instance.m_pixelSize + (float)num;
      my = p.z / Minimap.instance.m_pixelSize + (float)num;
      mx /= (float)Minimap.instance.m_textureSize;
      my /= (float)Minimap.instance.m_textureSize;
    }

    private static Vector2 MapPointToLocalGuiPos(float mx, float my, RawImage img)
    {
      Vector2 result = default(Vector2);
      result.x = (mx - img.uvRect.xMin) / img.uvRect.width;
      result.y = (my - img.uvRect.yMin) / img.uvRect.height;
      result.x *= img.rectTransform.rect.width;
      result.y *= img.rectTransform.rect.height;
      return result;
    }
  }
}