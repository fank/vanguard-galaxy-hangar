using System.Linq;
using Behaviour.Equipment;
using HarmonyLib;
using Source.SpaceShip;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VGHangar.UI;

public static class HangarUIFactory
{
    public static GameObject CreateFilterPanel(Transform parent)
    {
        var panelGo = new GameObject("FilterPanel", typeof(RectTransform));
        panelGo.transform.SetParent(parent, false);
        var panelRect = panelGo.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1, 0);
        panelRect.anchorMax = new Vector2(1, 1);
        panelRect.pivot = new Vector2(0, 1);
        panelRect.sizeDelta = new Vector2(60, 0);
        panelRect.anchoredPosition = new Vector2(5, 0);

        var layoutGroup = panelGo.AddComponent<VerticalLayoutGroup>();
        layoutGroup.padding = new RectOffset(5, 5, 5, 5);
        layoutGroup.spacing = 5;
        layoutGroup.childControlHeight = true;
        layoutGroup.childControlWidth = true;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.childForceExpandWidth = false;

        return panelGo;
    }

    public static GameObject CreateRoleFilterButton(Transform parent, SpaceShipRole role, Sprite? iconSprite, UnityEngine.Events.UnityAction onClick)
    {
        var buttonGo = new GameObject($"FilterButton_{role}", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        buttonGo.transform.SetParent(parent, false);

        var layout = buttonGo.GetComponent<LayoutElement>();
        layout.preferredHeight = 30f;
        layout.minHeight = 30f;
        layout.preferredWidth = 30f;
        layout.flexibleWidth = 0;

        var buttonImage = buttonGo.GetComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        var button = buttonGo.GetComponent<Button>();
        button.onClick.AddListener(onClick);
        var colors = button.colors;
        colors.highlightedColor = new Color(0.4f, 0.5f, 0.6f, 0.7f);
        colors.pressedColor = new Color(0.5f, 0.6f, 0.7f, 0.8f);
        button.colors = colors;

        var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconGo.transform.SetParent(buttonGo.transform, false);
        var iconRect = iconGo.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.pivot = new Vector2(0.5f, 0.5f);
        iconRect.sizeDelta = new Vector2(15, 15);
        iconRect.anchoredPosition = Vector2.zero;

        var iconImage = iconGo.GetComponent<Image>();
        iconImage.sprite = iconSprite;
        iconImage.color = GetRoleColor(role);

        return buttonGo;
    }

    public static GameObject CreateFilterButton(Transform parent, string label, UnityEngine.Events.UnityAction onClick)
    {
        var buttonGo = new GameObject($"FilterButton_{label}", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        buttonGo.transform.SetParent(parent, false);

        var layout = buttonGo.GetComponent<LayoutElement>();
        layout.preferredHeight = 30f;
        layout.minHeight = 30f;
        layout.preferredWidth = 30f;
        layout.flexibleWidth = 0;

        var image = buttonGo.GetComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        var button = buttonGo.GetComponent<Button>();
        button.onClick.AddListener(onClick);
        var colors = button.colors;
        colors.highlightedColor = new Color(0.4f, 0.5f, 0.6f, 0.7f);
        colors.pressedColor = new Color(0.5f, 0.6f, 0.7f, 0.8f);
        button.colors = colors;

        var text = CreateText(buttonGo.transform, label, 10, TextAlignmentOptions.Center);
        var textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        return buttonGo;
    }

    public static GameObject CreateShipListPanel(Transform parent)
    {
        var panelGo = new GameObject("VGHangarUIPanel", typeof(RectTransform));
        panelGo.transform.SetParent(parent, false);
        var panelRect = panelGo.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0); panelRect.anchorMax = new Vector2(0, 0);
        panelRect.pivot = new Vector2(0, 0); panelRect.sizeDelta = new Vector2(300, 600);
        panelRect.anchoredPosition = new Vector2(0, 324);
        panelGo.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

        var scrollViewGo = CreateScrollView(panelGo.transform);
        var scrollRect = scrollViewGo.GetComponent<RectTransform>();
        scrollRect.anchorMin = Vector2.zero; scrollRect.anchorMax = Vector2.one;
        scrollRect.sizeDelta = Vector2.zero; scrollRect.anchoredPosition = Vector2.zero;
        return panelGo;
    }

    public static GameObject CreateShipListItem(RectTransform parent, SpaceShipData shipData, Sprite? roleIcon)
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

        string nameAndClass = $"<b>{shipData.GetShipName()}</b>\n<size=12>{shipData.shipClass.displayName} (Size: {GetShipSize(shipData)})</size>";
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

        var viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(Mask), typeof(Image));
        viewportGo.transform.SetParent(svGo.transform, false);
        viewportGo.GetComponent<Mask>().showMaskGraphic = false;
        var viewImg = viewportGo.GetComponent<Image>();
        viewImg.raycastTarget = false;
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

    public static GameObject CreateText(Transform parent, string content, int fontSize, TextAlignmentOptions alignment)
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

    private static string GetShipSize(SpaceShipData shipData)
    {
        SpaceShipType shipType = GetShipType(shipData);
        if (shipType == SpaceShipType.Drone)
        {
            return "Drone";
        }
        return ((int)shipType).ToString();
    }

    // The publicized stub exposes SpaceShipRoleType.spaceShipType at compile
    // time, but the runtime DLL keeps the field private — direct access throws
    // FieldAccessException under Mono. Reach it through Traverse (cached
    // reflection) so the call site reads cleanly.
    public static SpaceShipType GetShipType(SpaceShipData shipData)
    {
        return Traverse.Create(shipData.shipClass.shipRoleType)
            .Field<SpaceShipType>("spaceShipType").Value;
    }
}
