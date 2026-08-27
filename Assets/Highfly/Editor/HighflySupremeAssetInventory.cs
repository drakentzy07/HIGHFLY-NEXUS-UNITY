#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Highfly.Editor
{
    /// <summary>
    /// Non-blocking inventory for the HIGHFLY SUPREME asset stack.
    /// New packs can be introduced progressively while the proven KayKit
    /// vertical slice remains a safe fallback for Android builds.
    /// </summary>
    public static class HighflySupremeAssetInventory
    {
        private struct Pack
        {
            public string Name;
            public string Root;
            public bool RequiredNow;

            public Pack(string name, string root, bool requiredNow)
            {
                Name = name;
                Root = root;
                RequiredNow = requiredNow;
            }
        }

        private static readonly Pack[] Packs =
        {
            new Pack("KayKit Adventurers", "Assets/External/KayKit/Adventurers", true),
            new Pack("KayKit Skeletons", "Assets/External/KayKit/Skeletons", true),
            new Pack("KayKit Dungeon", "Assets/External/KayKit/Dungeon", true),
            new Pack("KayKit Medieval", "Assets/External/KayKit/Medieval", true),

            new Pack("Quaternius Universal Base Characters", "Assets/External/Quaternius/UniversalBaseCharacters", false),
            new Pack("Quaternius Modular Fantasy Outfits", "Assets/External/Quaternius/ModularFantasyOutfits", false),
            new Pack("Quaternius Universal Animation Library 2", "Assets/External/Quaternius/UAL2", false),
            new Pack("Quaternius Medieval Weapons", "Assets/External/Quaternius/MedievalWeapons", false),
            new Pack("Quaternius Ultimate Modular Characters", "Assets/External/Quaternius/ModularCharacters", false),
            new Pack("Quaternius Ultimate Animated Animals", "Assets/External/Quaternius/AnimatedAnimals", false),
            new Pack("Quaternius Monster Expansion", "Assets/External/Quaternius/Monsters", false)
        };

        [MenuItem("HIGHFLY/SUPREME/Report Asset Inventory")]
        public static void Report()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            List<string> present = new List<string>();
            List<string> pending = new List<string>();
            List<string> missingRequired = new List<string>();

            foreach (Pack pack in Packs)
            {
                bool exists = AssetDatabase.IsValidFolder(pack.Root) ||
                              AssetDatabase.FindAssets(string.Empty, new[] { pack.Root }).Any();

                if (exists)
                {
                    present.Add(pack.Name);
                }
                else
                {
                    pending.Add(pack.Name);
                    if (pack.RequiredNow)
                        missingRequired.Add(pack.Name);
                }
            }

            Debug.Log(
                "HIGHFLY SUPREME asset inventory\n" +
                "PRESENT: " + (present.Count == 0 ? "none" : string.Join(", ", present)) + "\n" +
                "PENDING: " + (pending.Count == 0 ? "none" : string.Join(", ", pending)) + "\n" +
                "REQUIRED FALLBACK MISSING: " +
                (missingRequired.Count == 0 ? "none" : string.Join(", ", missingRequired)));

            // This inventory is intentionally non-blocking. Supreme packs are
            // introduced in layers; the stable KayKit slice must remain buildable.
        }
    }
}
#endif
