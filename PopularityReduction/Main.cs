using BepInEx;
using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine.EventSystems;

namespace PopularityReduction
{
    [BepInPlugin("Aidanamite.PopularityReduction", "PopularityReduction", "1.0.0")]
    public class Main : BaseUnityPlugin
    {
        internal static Assembly modAssembly = Assembly.GetExecutingAssembly();
        internal static string modName = $"{modAssembly.GetName().Name}";
        internal static string modDir = $"{Environment.CurrentDirectory}\\BepInEx\\{modName}";
        static InventorySlotGUI selected;

        void Awake()
        {
            new Harmony($"com.Aidanamite.{modName}").PatchAll(modAssembly);
            Logger.LogInfo($"{modName} has loaded");
            InventorySlotGUI.OnSlotSelected += (x) => selected = x;
        }

        void Update()
        {
            if (GUIManager.Instance
                && GUIManager.Instance.input != null
                && GUIManager.Instance.input.DPadLeft.WasPressed
                && !InfoPopUp.Active
                && GUIManager.Instance.CurrentInventory
                && GUIManager.Instance.CurrentInventory is NotebookPanel
                && selected && selected.item != null
                && ItemPriceManager.Instance
                && ItemPriceManager.Instance.GetPopularity(selected.item) > ItemPriceInfo.Popularity.Low
            )
                InfoPopUp.Show(string.Join("\n", "Are you sure you want reduce this item's popularity?".Replace(".", ".\n").SplitLines(35)), (b) =>
                {
                    if (b)
                    {
                        ItemPriceManager.Instance.SetPopularity(selected.item, ItemPriceManager.Instance.GetPopularity(selected.item) - 1);
                        (GUIManager.Instance.CurrentInventory as NotebookPanel).OnInventorySlotSelected(selected);
                    }
                });

        }
    }

    [HarmonyPatch(typeof(GUIManager), "OnApplicationFocus")]
    class Patch_GUIFocusChange
    {
        static bool Prefix(bool hasFocus) => !hasFocus || !InfoPopUp.Active;
    }
}