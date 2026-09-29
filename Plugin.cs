using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoSortChests
{
    // Dodaje przycisk "Sortuj" do okna otwartej skrzyni, obok istniejacych Take All / Stack All
    // (klon m_stackAllButton, wstawiony wyśrodkowany między nimi - pozycje zweryfikowane w logu gry).
    // Klik: laczy identyczne stosy (ta sama logika co ItemData.IsSameType uzywana przez gre),
    // potem uklada przedmioty wg typu + nazwy w kolejnosci siatki (wiersz po wierszu).
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class AutoSortChestsPlugin : BaseUnityPlugin
    {
        public const string PluginGUID = "com.michal.valheim.autosortchests";
        public const string PluginName = "Auto Sort Chests";
        public const string PluginVersion = "1.0.1";

        internal static ManualLogSource Log;

        private static readonly FieldInfo CurrentContainerField =
            AccessTools.Field(typeof(InventoryGui), "m_currentContainer");

        private void Awake()
        {
            Log = Logger;
            // Gra prosi mody o ustawienie tej flagi: w menu pojawia sie napis, ze gra jest
            // zmodowana (Iron Gate wymaga oznaczania modow jako nieoficjalnych).
            Game.isModded = true;
            new Harmony(PluginGUID).PatchAll(typeof(AutoSortChestsPlugin).Assembly);
        }

        [HarmonyPatch(typeof(InventoryGui), "Awake")]
        private static class InventoryGui_Awake_Patch
        {
            private static void Postfix(InventoryGui __instance)
            {
                try
                {
                    AddSortButton(__instance);
                }
                catch (System.Exception e)
                {
                    Log.LogError($"Failed to add sort button: {e}");
                }
            }
        }

        private static void AddSortButton(InventoryGui gui)
        {
            Button template = gui.m_stackAllButton;
            GameObject clone = UnityEngine.Object.Instantiate(template.gameObject, template.transform.parent);
            clone.name = "SortAll";

            RectTransform rt = clone.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0f, 150.5f);

            Button button = clone.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => SortCurrentContainer(gui));

            TextMeshProUGUI text = clone.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
                text.text = "Sort";

            // klon kopiuje tez dziecko z podpowiedzia dla pada od StackAll - wylaczamy,
            // bo nie mamy dla tego przycisku wpiecia do inputu gamepada.
            foreach (Transform child in clone.transform)
            {
                if (child.name.StartsWith("gamepad_hint"))
                    child.gameObject.SetActive(false);
            }
        }

        private static void SortCurrentContainer(InventoryGui gui)
        {
            var container = CurrentContainerField.GetValue(gui) as Container;
            if (container == null)
            {
                Log.LogWarning("No open container, nothing to sort.");
                return;
            }

            Inventory inventory = container.GetInventory();
            if (inventory == null)
                return;

            MergeStacks(inventory);
            ReorderByTypeAndName(inventory);

            inventory.m_onChanged?.Invoke();
            Log.LogInfo($"Sorted container '{container.name}'.");
        }

        private static void MergeStacks(Inventory inventory)
        {
            var items = inventory.GetAllItems();
            for (int i = 0; i < items.Count; i++)
            {
                ItemDrop.ItemData target = items[i];
                if (target.m_stack >= target.m_shared.m_maxStackSize)
                    continue;

                for (int j = items.Count - 1; j > i; j--)
                {
                    ItemDrop.ItemData source = items[j];
                    if (!target.IsSameType(source))
                        continue;

                    int space = target.m_shared.m_maxStackSize - target.m_stack;
                    if (space <= 0)
                        break;

                    int move = Mathf.Min(space, source.m_stack);
                    target.m_stack += move;
                    source.m_stack -= move;

                    if (source.m_stack <= 0)
                        items.RemoveAt(j);
                }
            }
        }

        private static void ReorderByTypeAndName(Inventory inventory)
        {
            var sorted = inventory.GetAllItems()
                .OrderBy(i => i.m_shared.m_itemType)
                .ThenBy(i => i.m_shared.m_name)
                .ToList();

            int width = inventory.GetWidth();
            for (int i = 0; i < sorted.Count; i++)
            {
                sorted[i].m_gridPos = new Vector2i(i % width, i / width);
            }
        }
    }
}
