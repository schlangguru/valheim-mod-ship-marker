using System;
using BepInEx;
using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;
using System.Collections.Generic;


namespace VH_Ship_Marker_Mod {

    [BepInPlugin("de.schlangguru.vh_ship_marker_mod", "Valheim Ship Marker Mod", "0.0.1")]
    [BepInProcess("valheim.exe")]
    public class Main : BaseUnityPlugin {

        private Harmony _harmony = null;

        private static Main _pluginInstance;
        private static Dictionary<int, ShipMarkerData> _shipMarkers = new Dictionary<int, ShipMarkerData>();


        void Awake() {
            _pluginInstance = this;
             _harmony = Harmony.CreateAndPatchAll(typeof(Main));
        }

        [HarmonyPatch(typeof(Ship), "Awake")]
        [HarmonyPostfix]
        private static void ShipAwake(Ship __instance) {
            _pluginInstance.Logger.LogDebug("Ship awoken: " + __instance.GetInstanceID());

            ShipMarkerData data = new ShipMarkerData();
            data.Ship = __instance;
            data.Marker = null;
            _shipMarkers[__instance.GetInstanceID()] = data;
        }


        [HarmonyPatch(typeof(Ship), "OnDestroyed")]
        [HarmonyPostfix]
        private static void ShipDestroy(Ship __instance) {
            _pluginInstance.Logger.LogDebug("Ship destroyed: " + __instance.GetInstanceID());

            int id = __instance.GetInstanceID();
            if (_shipMarkers.ContainsKey(id)) {
                ShipMarkerData data = _shipMarkers[id];
                data.Ship = null;
                if (data.Marker != null) {
                    UnityEngine.Object.Destroy(data.Marker);
                }
                data.Marker = null;
                _shipMarkers.Remove(__instance.GetInstanceID());
            }
        }

        [HarmonyPatch(typeof(Minimap), "UpdatePins")]
        [HarmonyPostfix]
        private static void UpdatePins(float ___m_largeZoom) {
            if (_shipMarkers.Count > 0) {
                RawImage rawImage = Minimap.instance.m_largeRoot.activeSelf ? Minimap.instance.m_mapImageLarge : Minimap.instance.m_mapImageSmall;
                float markerSize = Minimap.instance.m_largeRoot.activeSelf ? Minimap.instance.m_pinSizeLarge : Minimap.instance.m_pinSizeSmall;
			    RectTransform rectTransform = Minimap.instance.m_largeRoot.activeSelf ? Minimap.instance.m_pinRootLarge : Minimap.instance.m_pinRootSmall;
                foreach (ShipMarkerData data in _shipMarkers.Values) {
                    Vector3 shipPos = data.Ship.transform.position;
                    if (IsPointVisible(shipPos, rawImage) && !data.Ship.HasPlayerOnboard()) {
                        DrawShipMarker(data, markerSize, rectTransform, rawImage, ___m_largeZoom);
                    } else {
                        if (data.Marker != null) {
                            UnityEngine.Object.Destroy(data.Marker);
                        }
                    }
                }
            }
        }

        private static void DrawShipMarker(ShipMarkerData data, float size, RectTransform parent, RawImage rawImage, float largeMapZoom) {
            GameObject gameObject = data.Marker;
            if (gameObject == null || gameObject.transform.parent != parent) {
                if (gameObject != null) {
                    UnityEngine.Object.Destroy(gameObject);
                }

                gameObject = UnityEngine.Object.Instantiate<GameObject>(Minimap.instance.m_pinPrefab);
                data.Marker = gameObject;
				gameObject.GetComponent<Image>().sprite = GetShipMarkerSprite();
                gameObject.GetComponent<Image>().color = new Color(0.4f, 0.57f, 1f, 1f);
                gameObject.transform.SetParent(parent);
                (gameObject.transform as RectTransform).SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
                (gameObject.transform as RectTransform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
            }
            float mx;
            float my;
            WorldToMapPoint(data.Ship.transform.position, out mx, out my);
            Vector2 anchoredPosition = MapPointToLocalGuiPos(mx, my, rawImage);
            (gameObject.transform as RectTransform).anchoredPosition = anchoredPosition;
            gameObject.transform.Find("Checked").gameObject.SetActive(false);
            Text text = gameObject.transform.Find("Name").GetComponent<Text>();

            if (Minimap.instance.m_largeRoot.activeSelf && largeMapZoom < Minimap.instance.m_showNamesZoom) {
                text.gameObject.SetActive(true);
                text.text = Localization.instance.Localize(data.Ship.GetComponentInParent<Piece>().m_name);
            } else {
                text.gameObject.SetActive(false);
            }
        }

        private static Sprite GetShipMarkerSprite() {
            return Minimap.instance.m_icons.Find((Minimap.SpriteData x) => x.m_name == Minimap.PinType.Icon2).m_icon;
        }

        private static bool IsPointVisible(Vector3 p, RawImage map) {
            float num;
            float num2;
            WorldToMapPoint(p, out num, out num2);
            return num > map.uvRect.xMin && num < map.uvRect.xMax && num2 > map.uvRect.yMin && num2 < map.uvRect.yMax;
        }

        private static void WorldToMapPoint(Vector3 p, out float mx, out float my) {
            int num = Minimap.instance.m_textureSize / 2;
            mx = p.x / Minimap.instance.m_pixelSize + (float)num;
            my = p.z / Minimap.instance.m_pixelSize + (float)num;
            mx /= (float)Minimap.instance.m_textureSize;
            my /= (float)Minimap.instance.m_textureSize;
        }

        private static Vector2 MapPointToLocalGuiPos(float mx, float my, RawImage img) {
			Vector2 result = default(Vector2);
			result.x = (mx - img.uvRect.xMin) / img.uvRect.width;
			result.y = (my - img.uvRect.yMin) / img.uvRect.height;
			result.x *= img.rectTransform.rect.width;
			result.y *= img.rectTransform.rect.height;
			return result;
		}

        private void OnDestroy() {
            this._harmony?.UnpatchSelf();
            foreach (ShipMarkerData data in _shipMarkers.Values) {
                if (data.Marker != null) {
                    UnityEngine.Object.Destroy(data.Marker);
                }
            }
            _shipMarkers.Clear();
        }
    }
}
