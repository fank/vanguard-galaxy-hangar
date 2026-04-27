using System.Collections.Generic;
using System.Linq;
using Behaviour.UI.Main;
using Behaviour.UI.ShipCarousel;
using HarmonyLib;
using Source.SpaceShip;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VGHangar.UI;

public class HangarUIController : MonoBehaviour
{
    private ShipCarousel _carousel = null!;
    private List<GameObject> _shipListItems = new();
    private RectTransform _contentArea = null!;
    private Sprite? _roleIconSprite;

    private RectTransform? _filterPanel;
    private HashSet<SpaceShipRole> _activeRoleFilters = new();
    private Dictionary<SpaceShipRole, GameObject> _roleFilterButtons = new();
    private HashSet<int> _activeSizeFilters = new();
    private Dictionary<int, GameObject> _sizeFilterButtons = new();

    private List<SpaceShipData> _allShips = new();

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
        _allShips = ships.OrderBy(s => s.GetShipName()).ToList();
        CacheRoleIcon();
        if (_contentArea == null) _contentArea = transform.Find("Scroll View/Viewport/Content").GetComponent<RectTransform>();

        SetupFilterPanel();

        RedrawShipList();
    }

    private void SetupFilterPanel()
    {
        if (_filterPanel == null)
        {
            _filterPanel = HangarUIFactory.CreateFilterPanel(transform).GetComponent<RectTransform>();
        }

        foreach (var btn in _roleFilterButtons.Values) Destroy(btn);
        _roleFilterButtons.Clear();
        foreach (var btn in _sizeFilterButtons.Values) Destroy(btn);
        _sizeFilterButtons.Clear();

        var roles = _allShips.Select(s => s.shipClass.shipRoleType.GetRole()).Distinct().ToList();
        var sizes = _allShips.Select(s => (int)s.shipClass.shipRoleType.spaceShipType).Where(s => s > 0).Distinct().OrderBy(s => s).ToList();

        _activeRoleFilters = new HashSet<SpaceShipRole>(roles);
        _activeSizeFilters = new HashSet<int>(sizes);

        HangarUIFactory.CreateText(_filterPanel, "Roles", 10, TextAlignmentOptions.Center);
        foreach (var role in roles)
        {
            var buttonGo = HangarUIFactory.CreateRoleFilterButton(_filterPanel, role, _roleIconSprite, () => OnFilterButtonClicked(role, null));
            _roleFilterButtons[role] = buttonGo;
        }

        HangarUIFactory.CreateText(_filterPanel, "Sizes", 10, TextAlignmentOptions.Center);
        foreach (var size in sizes)
        {
            var buttonGo = HangarUIFactory.CreateFilterButton(_filterPanel, size.ToString(), () => OnFilterButtonClicked(null, size));
            _sizeFilterButtons[size] = buttonGo;
        }

        UpdateFilterButtonColors();
    }

    private void OnFilterButtonClicked(SpaceShipRole? role, int? size)
    {
        if (role.HasValue)
        {
            if (_activeRoleFilters.Contains(role.Value)) _activeRoleFilters.Remove(role.Value);
            else _activeRoleFilters.Add(role.Value);
        }
        if (size.HasValue)
        {
            if (_activeSizeFilters.Contains(size.Value)) _activeSizeFilters.Remove(size.Value);
            else _activeSizeFilters.Add(size.Value);
        }

        UpdateFilterButtonColors();
        RedrawShipList();
    }

    private void UpdateFilterButtonColors()
    {
        foreach (var kvp in _roleFilterButtons)
        {
            kvp.Value.GetComponent<Image>().color = _activeRoleFilters.Contains(kvp.Key)
                ? new Color(0.3f, 0.4f, 0.5f, 0.7f)
                : new Color(0.2f, 0.2f, 0.2f, 0.7f);
        }
        foreach (var kvp in _sizeFilterButtons)
        {
            kvp.Value.GetComponent<Image>().color = _activeSizeFilters.Contains(kvp.Key)
                ? new Color(0.3f, 0.4f, 0.5f, 0.7f)
                : new Color(0.2f, 0.2f, 0.2f, 0.7f);
        }
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

    private void RedrawShipList()
    {
        _shipListItems.ForEach(Destroy);
        _shipListItems.Clear();

        _contentArea.sizeDelta = new Vector2(0, 0);

        var filteredShips = _allShips
            .Where(s => _activeRoleFilters.Contains(s.shipClass.shipRoleType.GetRole()) &&
                        (_activeSizeFilters.Contains((int)s.shipClass.shipRoleType.spaceShipType) || (int)s.shipClass.shipRoleType.spaceShipType == 0)) // include drones
            .ToList();

        foreach (var shipData in filteredShips)
        {
            GameObject itemGo = HangarUIFactory.CreateShipListItem(_contentArea, shipData, _roleIconSprite);
            itemGo.GetComponent<Button>().onClick.AddListener(() => OnShipSelected(shipData));
            _shipListItems.Add(itemGo);
        }

        if (_carousel.selectedShipData != null && filteredShips.All(s => s.guid != _carousel.selectedShipData.guid))
        {
            OnShipSelected(filteredShips.FirstOrDefault());
        }
        else
        {
            OnShipSelected(_carousel.selectedShipData);
        }
    }

    private void OnShipSelected(SpaceShipData? selectedShip)
    {
        if (_carousel == null) return;
        if (selectedShip == null && _shipListItems.Any())
        {
            selectedShip = _shipListItems.First().GetComponent<ShipListItemController>().ShipData;
        }
        if (selectedShip == null) return;

        int newIndex = _carousel.ships.FindIndex(s => s.guid == selectedShip.guid);
        if (newIndex != -1 && (_carousel.selectedIndex != newIndex || _carousel.selectedShipData.guid != selectedShip.guid))
        {
            _carousel.selectedIndex = newIndex;
            _carousel.ShowShip(true);
        }
        foreach (var item in _shipListItems)
        {
            var controller = item.GetComponent<ShipListItemController>();
            if (controller != null)
            {
                item.GetComponent<Image>().color = controller.ShipData.guid == selectedShip.guid
                    ? new Color(0.3f, 0.4f, 0.5f, 0.7f) : new Color(0.2f, 0.2f, 0.2f, 0.7f);
            }
        }
    }
}

public class ShipListItemController : MonoBehaviour
{
    public SpaceShipData ShipData { get; set; } = null!;
}
