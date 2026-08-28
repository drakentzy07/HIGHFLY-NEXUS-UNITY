#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Highfly.Editor
{
    /// <summary>
    /// Asset gate for HIGHFLY SUPREME.
    /// Required foundation packs must be genuinely present before Android builds.
    /// Optional Supreme packs can still be introduced progressively.
    /// </summary>
    public static class HighflySupremeAssetInventory
    {
        private struct Pack
        {
            public string Name;
            public string Root;
            public string AnchorAsset;
            public bool RequiredNow;

            public Pack(string name, string root, string anchorAsset, bool requiredNow)
            {
                Name = name;
                Root = root;
                AnchorAsset = anchorAsset;
                RequiredNow = requiredNow;
            }
        }

        private static readonly Pack[] Packs =
        {
            new Pack(
                "KayKit Adventurers",
                "Assets/External/KayKit/Adventurers",
                "Assets/External/KayKit/Adventurers/addons/kaykit_character_pack_adventures/Characters/fbx/RogueHooded.fbx",
                true),
            new Pack(
                "KayKit Skeletons",
                "Assets/External/KayKit/Skeletons",
                "Assets/External/KayKit/Skeletons/addons/kaykit_character_pack_skeletons/Characters/fbx/Skeleton_Warrior.fbx",
                true),
            new Pack(
                "KayKit Dungeon",
                "Assets/External/KayKit/Dungeon",
                "Assets/External/KayKit/Dungeon/addons/kaykit_dungeon_remastered/Assets/fbx/floor_tile_large.fbx",
                true),
            new Pack(
                "KayKit Medieval",
                "Assets/External/KayKit/Medieval",
                "Assets/External/KayKit/Medieval/addons/kaykit_medieval_hexagon_pack/Assets/fbx/buildings/blue/building_tavern_blue.fbx",
                true),

            new Pack("Quaternius Universal Base Characters", "Assets/External/Quaternius/UniversalBaseCharacters", null, false),
            new Pack("Quaternius Modular Fantasy Outfits", "Assets/External/Quaternius/ModularFantasyOutfits", null, false),
            new Pack("Quaternius Universal Animation Library 2", "Assets/External/Quaternius/UAL2", null, false),
            new Pack("Quaternius Medieval Weapons", "Assets/External/Quaternius/MedievalWeapons", null, false),
            new Pack("Quaternius Ultimate Modular Characters", "Assets/External/Quaternius/ModularCharacters", null, false),
            new Pack("Quaternius Ultimate Animated Animals", "Assets/External/Quaternius/AnimatedAnimals", null, false),
            new Pack("Quaternius Monster Expansion", "Assets/External/Quaternius/Monsters", null, false)
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
                bool rootExists = AssetDatabase.IsValidFolder(pack.Root);
                bool anchorExists = string.IsNullOrEmpty(pack.AnchorAsset) ||
                                    AssetDatabase.LoadMainAssetAtPath(pack.AnchorAsset) != null;
                bool exists = rootExists && anchorExists;

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
                "REQUIRED FOUNDATION MISSING: " +
                (missingRequired.Count == 0 ? "none" : string.Join(", ", missingRequired)));

            if (missingRequired.Count > 0)
            {
                throw new BuildFailedException(
                    "HIGHFLY SUPREME refused to build with fallback capsules. Missing required foundation: " +
                    string.Join(", ", missingRequired));
            }
        }
    }
}
#endif
