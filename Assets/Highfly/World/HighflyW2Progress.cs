using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Highfly.World
{
    // W2 vertical slice. The save authority belongs to HIGHFLY, not the
    // ClaudeCraft client/server (which uses TypeScript and a separate backend).
    // Architecture follows the MIT-licensed item-ledger / quest-snapshot pattern.
    [DisallowMultipleComponent]
    public sealed class HighflyW2Progress : MonoBehaviour
    {
        [Serializable]
        public sealed class Stack
        {
            public string id;
            public int count;
        }

        [Serializable]
        private sealed class SaveData
        {
            public int version = 1;
            public int level = 1;
            public int xp;
            public int gold;
            public int questStage; // 0 available, 1 accepted, 2 turned in
            public int skeletonsKilled;
            public int look = -1;
            public int visualYaw = 90;
            public string weapon = "";
            public string armor = "";
            public List<Stack> items = new List<Stack>();
        }

        public struct Item
        {
            public readonly string Id;
            public readonly string Name;
            public readonly string Category;
            public readonly int Strength;
            public readonly int Vitality;

            public Item(string id, string name, string category,
                int strength = 0, int vitality = 0)
            {
                Id = id; Name = name; Category = category;
                Strength = strength; Vitality = vitality;
            }
        }

        public const string SaveKey = "HIGHFLY_W2_HUNTER_V1";
        private static readonly Item[] Catalog =
        {
            new Item("sword_rookie", "Espada del Caminante", "arma", 3),
            new Item("armor_rookie", "Armadura de Recluta", "armadura", 0, 3),
            new Item("bone_fragment", "Fragmento de esqueleto", "material"),
            new Item("wolf_hide", "Piel de lobo", "material")
        };

        private SaveData _data;
        private HighflyHunterAppearance _appearance;
        private HighflyW2WeaponVisual _weaponVisual;
        public event Action Changed;

        public int Level => _data != null ? _data.level : 1;
        public int Experience => _data != null ? _data.xp : 0;
        public int Gold => _data != null ? _data.gold : 0;
        public int QuestStage => _data != null ? _data.questStage : 0;
        public int SkeletonsKilled => _data != null ? _data.skeletonsKilled : 0;
        public string Weapon => _data != null ? _data.weapon : "";
        public string Armor => _data != null ? _data.armor : "";
        public int StrengthBonus => FindItem(Weapon).Strength;
        public int VitalityBonus => FindItem(Armor).Vitality;

        public static Item FindItem(string id)
        {
            foreach (Item item in Catalog)
                if (item.Id == id) return item;
            return new Item("", "", "");
        }

        private void Awake()
        {
            Load();
        }

        private IEnumerator Start()
        {
            _appearance = GetComponent<HighflyHunterAppearance>();
            _weaponVisual = GetComponent<HighflyW2WeaponVisual>();
            // Quaternius appearance has its own Start. Restore after it,
            // rather than racing the outfit's serialized default index.
            yield return null;
            if (_appearance != null)
            {
                _appearance.SetVisualYawOffset(_data.visualYaw);
                if (_data.look >= 0 && _data.look < _appearance.VariantCount)
                    _appearance.SetAppearance(_data.look);
                _appearance.AppearanceChanged += OnLookChanged;
                _appearance.FacingOffsetChanged += OnFacingChanged;
            }
            RefreshWeapon();
            Changed?.Invoke();
        }

        private void OnDestroy()
        {
            if (_appearance != null)
            {
                _appearance.AppearanceChanged -= OnLookChanged;
                _appearance.FacingOffsetChanged -= OnFacingChanged;
            }
        }

        private void OnFacingChanged(int degrees)
        {
            _data.visualYaw = degrees;
            Save();
        }

        private void OnLookChanged(int index)
        {
            _data.look = index;
            Save();
        }

        private void Load()
        {
            _data = null;
            string json = PlayerPrefs.GetString(SaveKey, "");
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    SaveData saved = JsonUtility.FromJson<SaveData>(json);
                    if (saved != null && saved.version == 1)
                        _data = saved;
                }
                catch (Exception e)
                {
                    Debug.LogWarning("HIGHFLY W2: save not readable; " + e.Message);
                }
            }
            if (_data == null)
            {
                _data = new SaveData();
                _data.items.Add(new Stack { id = "sword_rookie", count = 1 });
                _data.weapon = "sword_rookie";
                Save();
            }
            if (_data.items == null) _data.items = new List<Stack>();
            _data.level = Mathf.Clamp(_data.level, 1, 20);
            _data.gold = Mathf.Max(0, _data.gold);
        }

        public void Save()
        {
            if (_data == null) return;
            try
            {
                PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(_data));
                PlayerPrefs.Save();
                Changed?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError("HIGHFLY W2 save failed: " + e.Message);
            }
        }

        public int Count(string itemId)
        {
            if (_data == null || _data.items == null) return 0;
            foreach (Stack item in _data.items)
                if (item.id == itemId) return item.count;
            return 0;
        }

        public string GetItemIdAtSlot(int index)
        {
            if (_data == null || _data.items == null ||
                index < 0 || index >= _data.items.Count) return "";
            Stack stack = _data.items[index];
            return stack.count > 0 ? stack.id : "";
        }

        public int GetStackCountAtSlot(int index)
        {
            if (_data == null || _data.items == null ||
                index < 0 || index >= _data.items.Count) return 0;
            return Mathf.Max(0, _data.items[index].count);
        }

        public string InventoryText()
        {
            if (_data == null || _data.items == null) return "Inventario no disponible";
            if (_data.items.Count == 0) return "La mochila está vacía.";
            string result = "";
            foreach (Stack entry in _data.items)
            {
                if (entry.count <= 0) continue;
                Item item = FindItem(entry.id);
                if (string.IsNullOrEmpty(item.Id)) continue;
                result += item.Name + " x" + entry.count + "\n";
            }
            return string.IsNullOrEmpty(result) ? "La mochila está vacía." : result;
        }

        public void AddItem(string itemId, int quantity)
        {
            Item item = FindItem(itemId);
            if (string.IsNullOrEmpty(item.Id) || quantity <= 0) return;
            foreach (Stack entry in _data.items)
            {
                if (entry.id != itemId) continue;
                entry.count = Mathf.Min(9999, entry.count + quantity);
                Save();
                return;
            }
            _data.items.Add(new Stack { id = itemId, count = quantity });
            Save();
        }

        public void AddGold(int amount)
        {
            if (amount <= 0) return;
            _data.gold = Mathf.Min(9999999, _data.gold + amount);
            Save();
        }

        public bool Equip(string itemId)
        {
            Item item = FindItem(itemId);
            if (Count(itemId) <= 0 || string.IsNullOrEmpty(item.Id)) return false;
            if (item.Category == "arma") _data.weapon = itemId;
            else if (item.Category == "armadura") _data.armor = itemId;
            else return false;
            RefreshWeapon();
            Save();
            return true;
        }

        public void UnequipWeapon()
        {
            _data.weapon = "";
            RefreshWeapon();
            Save();
        }

        private void RefreshWeapon()
        {
            if (_weaponVisual == null)
                _weaponVisual = GetComponent<HighflyW2WeaponVisual>();
            if (_weaponVisual != null)
                _weaponVisual.SetEquipped(!string.IsNullOrEmpty(_data.weapon));
            Highfly.Combat.HighflyHealth health =
                GetComponent<Highfly.Combat.HighflyHealth>();
            if (health != null)
                health.SetEquipmentHealthBonus(VitalityBonus * 10f);
        }

        public void OnSkeletonDefeated()
        {
            if (_data.questStage != 1 || _data.skeletonsKilled >= 2) return;
            _data.skeletonsKilled++;
            Save();
        }

        public string TalkToSerin()
        {
            if (_data.questStage == 0)
            {
                _data.questStage = 1;
                _data.skeletonsKilled = 0;
                Save();
                return "CONTRATO ACEPTADO: derrotá 2 esqueletos fuera de la ciudad. Volvé para cobrar.";
            }
            if (_data.questStage == 1 && _data.skeletonsKilled < 2)
                return "CONTRATO: esqueletos derrotados " + _data.skeletonsKilled + "/2. Buscá la zona del bosque.";
            if (_data.questStage == 1)
            {
                _data.questStage = 2;
                AddGold(75);
                AddItem("armor_rookie", 1);
                GrantExperience(80);
                Save();
                return "¡CONTRATO COMPLETADO! +75 oro, +80 XP y Armadura de Recluta (+3 VIT).";
            }
            return "¡Buen trabajo, Hunter! El próximo contrato llegará con WORLD W2.";
        }

        public string QuestText
        {
            get
            {
                if (_data == null) return "Sin datos";
                if (_data.questStage == 0) return "Hablá con Serin en la ciudad para aceptar el primer contrato.";
                if (_data.questStage == 1) return "PRIMER CONTRATO: esqueletos " +
                    _data.skeletonsKilled + "/2" +
                    (_data.skeletonsKilled >= 2 ? "\nVolvé a hablar con Serin." : "");
                return "PRIMER CONTRATO COMPLETADO. ¡El mundo comienza!";
            }
        }

        private void GrantExperience(int amount)
        {
            _data.xp = Mathf.Max(0, _data.xp + amount);
            while (_data.level < 20 && _data.xp >= _data.level * 100)
            {
                _data.xp -= _data.level * 100;
                _data.level++;
            }
            if (_data.level == 20) _data.xp = 0;
            Save();
        }
    }
}
