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
