using System;
using Behaviour.UI.Main;
using Behaviour.UI.ShipCarousel;
using Behaviour.UI.Spacestation;
using Behaviour.UI.Spacestation.Location;
using HarmonyLib;
using Source.Player;
using UnityEngine;
using UnityEngine.UI;
using VGHangar.UI;

namespace VGHangar.Patches;

[HarmonyPatch(typeof(PersonalHangar), "ShowShips")]
public static class HangarUIPatches
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

            Transform parent = SpaceStationInterior.instance?.transform!;
            if (parent == null) { parent = __instance.transform; }

            Transform existingPanel = parent.Find("VGHangarUIPanel");
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
        catch (Exception e) { Plugin.Log.LogError($"VGHangar: {e}"); }
    }
}

// As of game 0.8.1.4, ShipCarousel.ShowShip re-enables the prev/next arrows
// whenever the player owns more than one ship — and SetPlayerShips queues a
// DelayedShowShip coroutine that calls ShowShip one frame after our ShowShips
// postfix runs. So a one-shot hide no longer sticks; re-hide the arrows after
// every ShowShip, but only for the carousel our list panel is driving (the
// Shipyard shares this class and must keep its own arrows).
[HarmonyPatch(typeof(ShipCarousel), nameof(ShipCarousel.ShowShip))]
public static class ShipCarouselShowShipPatch
{
    public static void Postfix(ShipCarousel __instance)
    {
        try
        {
            if (__instance != HangarUIController.ActiveCarousel) return;
            Traverse.Create(__instance).Field<Button>("previousButton").Value?.gameObject.SetActive(false);
            Traverse.Create(__instance).Field<Button>("nextButton").Value?.gameObject.SetActive(false);
        }
        catch (Exception e) { Plugin.Log.LogError($"VGHangar ShowShip postfix: {e}"); }
    }
}
