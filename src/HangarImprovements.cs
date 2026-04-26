
using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Behaviour.UI.Spacestation.Location;
using Behaviour.UI.Main;
using Source.SpaceShip;
using Behaviour.Item;
using Behaviour.Unit;
using Behaviour.Equipment;
using Source.Player;
using Behaviour.UI.Side_Menu;
using Behaviour.UI.Spacestation;
using Behaviour.UI.ShipCarousel;
using UnityEngine.EventSystems;

namespace HangarImprovements
{
    [BepInPlugin("com.yourname.hangarimprovements", "Hangar Improvements", "0.1.0")]
    public class HangarImprovementsPlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            Logger.LogInfo("Hangar Improvements v2.0 (Reverted) loading!");
            var harmony = new Harmony("com.yourname.hangarimprovements");
            harmony.PatchAll(typeof(HangarPatch));
        }
    }

    [HarmonyPatch(typeof(PersonalHangar), "ShowShips")]
    public static class HangarPatch
    {
        public static void Postfix(PersonalHangar __instance)
        {
            try
            {
                var carousel = __instance.shipSelect;
                if (carousel == null) return;

                var previousButton = Traverse.Create(carousel).Field<Button>("previousButton").Value;
                var nextButton = Traverse.Create(carousel).Field<Button>("nextButton").Value;
                if (previousButton != null) previousButton.gameObject.SetActive(false);
                if (nextButton != null) nextButton.gameObject.SetActive(false);

                Transform parent = SpaceStationInterior.instance?.transform;
                if (parent == null) { parent = __instance.transform; }

                Transform existingPanel = parent.Find("HangarImprovementsUIPanel");
                if (existingPanel != null)
                {
                    existingPanel.gameObject.SetActive(true);
                    existingPanel.GetComponent<HangarUIController>()?.PopulateShipList(carousel, GamePlayer.current.spaceShips);
                }
                else
                {
                    GameObject panelObject = HangarUIFactory.CreateShipListPanel(parent);
                    panelObject.AddComponent<HangarUIController>().PopulateShipList(carousel, GamePlayer.current.spaceShips);
                }
            }
            catch (Exception e) { Debug.LogError($"Hangar Improvements Error: {e}"); }
        }
    }
    
    public class HangarUIController : MonoBehaviour
    {
        private ShipCarousel _carousel;
        private List<GameObject> _shipListItems = new List<GameObject>();
        private RectTransform _contentArea;
        private Sprite _roleIconSprite;

        void Update()
        {
            if (!gameObject.activeSelf) return;
            if (_carousel == null || !_carousel.gameObject.activeInHierarchy)
            {
                gameObject.SetActive(false);
            }
        }

        public void PopulateShipList(ShipCarousel carousel, List<SpaceShipData> ships)
        {
            _carousel = carousel;
            CacheRoleIcon();
            if (_contentArea == null) _contentArea = transform.Find("Scroll View/Viewport/Content").GetComponent<RectTransform>();
            RedrawShipList(ships.OrderBy(s => s.GetShipName()).ToList());
        }
        
        private void CacheRoleIcon()
        {
            if (_carousel != null && _roleIconSprite == null)
            {
                var hullBonusBadgePrefab = Traverse.Create(_carousel).Field<Badge>("hullBonusBadgePrefab").Value;
                if (hullBonusBadgePrefab != null)
                {
                    Image iconImage = hullBonusBadgePrefab.GetComponentInChildren<Image>();
                    if (iconImage != null) _roleIconSprite = iconImage.sprite;
                }
            }
        }

        private void RedrawShipList(List<SpaceShipData> ships)
        {
            _shipListItems.ForEach(Destroy);
            _shipListItems.Clear();
            // ContentSizeFitter will handle height, so we just set min width to parent
            _contentArea.sizeDelta = new Vector2(0, 0); 
            
            foreach (var shipData in ships)
            {
                // No yPos needed, layout group handles it
                GameObject itemGo = HangarUIFactory.CreateShipListItem(_contentArea, shipData, _roleIconSprite);
                itemGo.GetComponent<Button>().onClick.AddListener(() => OnShipSelected(shipData));
                _shipListItems.Add(itemGo);
            }
            OnShipSelected(_carousel.selectedShipData);
        }

        private void OnShipSelected(SpaceShipData selectedShip)
        {
            if (_carousel == null || selectedShip == null) return;
            int newIndex = _carousel.ships.FindIndex(s => s.guid == selectedShip.guid);
            if (newIndex != -1 && (_carousel.selectedIndex != newIndex || _carousel.selectedShipData.guid != selectedShip.guid))
            {
                _carousel.selectedIndex = newIndex;
                _carousel.ShowShip(true);
            }
            foreach(var item in _shipListItems)
            {
                var controller = item.GetComponent<ShipListItemController>();
                if(controller != null)
                {
                    item.GetComponent<Image>().color = controller.ShipData.guid == selectedShip.guid
                        ? new Color(0.3f, 0.4f, 0.5f, 0.7f) : new Color(0.2f, 0.2f, 0.2f, 0.7f);
                }
            }
        }
    }

    public class ShipListItemController : MonoBehaviour { public SpaceShipData ShipData { get; set; } }

    public static class HangarUIFactory
    {
        public static GameObject CreateShipListPanel(Transform parent)
        {
            var panelGo = new GameObject("HangarImprovementsUIPanel", typeof(RectTransform));
            panelGo.transform.SetParent(parent, false);
            var panelRect = panelGo.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 0); panelRect.anchorMax = new Vector2(0, 0);
            panelRect.pivot = new Vector2(0, 0); panelRect.sizeDelta = new Vector2(300, 600);
            panelRect.anchoredPosition = new Vector2(0, 316);
            panelGo.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            
            var scrollViewGo = CreateScrollView(panelGo.transform);
            var scrollRect = scrollViewGo.GetComponent<RectTransform>();
            scrollRect.anchorMin = Vector2.zero; scrollRect.anchorMax = Vector2.one;
            scrollRect.sizeDelta = Vector2.zero; scrollRect.anchoredPosition = Vector2.zero;
            return panelGo;
        }

        public static GameObject CreateShipListItem(RectTransform parent, SpaceShipData shipData, Sprite roleIcon)
        {
            var itemGo = new GameObject($"ShipItem_{shipData.GetShipName()}", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            itemGo.transform.SetParent(parent, false);
            itemGo.AddComponent<ShipListItemController>().ShipData = shipData;
            
            var layout = itemGo.GetComponent<LayoutElement>();
            layout.preferredHeight = 60;
            layout.minHeight = 60;
            layout.flexibleWidth = 1; // Allow item to stretch horizontally

            var colors = itemGo.GetComponent<Button>().colors;
            colors.highlightedColor = new Color(0.4f, 0.5f, 0.6f, 0.7f);
            itemGo.GetComponent<Button>().colors = colors;

            string nameAndClass = $"<b>{shipData.GetShipName()}</b>\n<size=12>{shipData.shipClass.displayName}</size>";
            var nameText = CreateText(itemGo.transform, nameAndClass, 14, TextAlignmentOptions.TopLeft);
            var nameRect = nameText.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0); nameRect.anchorMax = new Vector2(1, 1);
            nameRect.pivot = new Vector2(0, 1);
            nameRect.anchoredPosition = new Vector2(30, -8);
            nameRect.sizeDelta = new Vector2(-40, -10);

            var (avgTurretLvl, avgModuleLvl) = CalculateAvgGearLevel(shipData);
            var gearText = CreateText(itemGo.transform, $"T-Lvl: {avgTurretLvl:F0} | M-Lvl: {avgModuleLvl:F0}", 12, TextAlignmentOptions.BottomRight);
            var gearRect = gearText.GetComponent<RectTransform>();
            gearRect.anchorMin = new Vector2(0, 0); gearRect.anchorMax = new Vector2(1, 1);
            gearRect.pivot = new Vector2(1, 0);
            gearRect.anchoredPosition = new Vector2(-10, 10);
            gearRect.sizeDelta = new Vector2(-15, -40);
            
            var roleIconGo = new GameObject("RoleIcon", typeof(RectTransform), typeof(Image));
            roleIconGo.transform.SetParent(itemGo.transform, false);
            var roleRect = roleIconGo.GetComponent<RectTransform>();
            roleRect.anchorMin = new Vector2(0, 1); roleRect.anchorMax = new Vector2(0, 1);
            roleRect.pivot = new Vector2(0, 1);
            roleRect.sizeDelta = new Vector2(18, 18);
            roleRect.anchoredPosition = new Vector2(8, -8);
            var roleImg = roleIconGo.GetComponent<Image>();
            roleImg.sprite = roleIcon;
            roleImg.color = GetRoleColor(shipData.shipClass.shipRoleType.GetRole());

            return itemGo;
        }

        private static (float, float) CalculateAvgGearLevel(SpaceShipData shipData)
        {
            var turrets = shipData.hardpoints.Where(h => h != null).ToList();
            var modules = shipData.equippedModules.Where(m => m != null && m.GetComponent<AbstractEquipment>()?.slot.ToString() != "Reactor").ToList();
            float avgTurretLvl = turrets.Any() ? (float)turrets.Average(t => t.itemLevel) : 0;
            float avgModuleLvl = modules.Any() ? (float)modules.Average(m => m.itemLevel) : 0;
            return (avgTurretLvl, avgModuleLvl);
        }

        private static GameObject CreateScrollView(Transform parent)
        {
            var svGo = new GameObject("Scroll View", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
            svGo.transform.SetParent(parent, false);
            var scrollRect = svGo.GetComponent<ScrollRect>();
            scrollRect.scrollSensitivity = 35f;
            var bgImg = svGo.GetComponent<Image>();
            bgImg.color = new Color(0.05f, 0.05f, 0.05f, 0.9f);
            bgImg.raycastTarget = true;
            
            var viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(Mask));
            viewportGo.transform.SetParent(svGo.transform, false);
            viewportGo.GetComponent<Mask>().showMaskGraphic = false;
            var viewRect = viewportGo.GetComponent<RectTransform>();
            viewRect.pivot = new Vector2(0, 1); viewRect.anchorMin = Vector2.zero; viewRect.anchorMax = Vector2.one;
            viewRect.sizeDelta = Vector2.zero;
            
            var contentGo = new GameObject("Content", typeof(RectTransform));
            contentGo.transform.SetParent(viewportGo.transform, false);
            var contentRect = contentGo.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1); contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            
            var layoutGroup = contentGo.AddComponent<VerticalLayoutGroup>();
            layoutGroup.padding = new RectOffset(5, 5, 5, 5);
            layoutGroup.spacing = 5;
            layoutGroup.childControlHeight = true;
            layoutGroup.childControlWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childForceExpandWidth = true;

            var sizeFitter = contentGo.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            scrollRect.viewport = viewRect; scrollRect.content = contentRect;
            scrollRect.vertical = true; scrollRect.horizontal = false;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            return svGo;
        }

        private static GameObject CreateText(Transform parent, string content, int fontSize, TextAlignmentOptions alignment)
        {
            var textGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(parent, false);
            var textComp = textGo.GetComponent<TextMeshProUGUI>();
            textComp.text = content; textComp.fontSize = fontSize;
            textComp.alignment = alignment; textComp.color = Color.white;
            textComp.raycastTarget = false;
            return textGo;
        }
        
        private static Color GetRoleColor(SpaceShipRole role)
        {
            switch (role)
            {
                case SpaceShipRole.Combat: return new Color(0.9f, 0.3f, 0.3f);
                case SpaceShipRole.Mining: return new Color(0.4f, 0.7f, 1f);
                case SpaceShipRole.Salvaging: return new Color(1f, 0.8f, 0.4f);
                case SpaceShipRole.Cargo: return new Color(0.5f, 0.9f, 0.5f);
                default: return Color.grey;
            }
        }

        private static string GetSizeString(SpaceShipType type)
        {
            switch(type)
            {
                case SpaceShipType.Size1: return "Corvette"; case SpaceShipType.Size2: return "Frigate";
                case SpaceShipType.Size3: return "Destroyer"; case SpaceShipType.Size4: return "Cruiser";
                case SpaceShipType.Size5: return "Battlecruiser"; case SpaceShipType.Size6: return "Battleship";
                case SpaceShipType.Size7: return "Dreadnought"; case SpaceShipType.Size8: return "Carrier";
                case SpaceShipType.Drone: return "Drone"; default: return "Unknown";
            }
        }
    }
}
