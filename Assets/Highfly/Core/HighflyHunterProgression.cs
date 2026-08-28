using System;
using UnityEngine;

namespace Highfly.Core
{
    /// <summary>
    /// Persistent hunter progression. Levels/ranks do not grant combat stats:
    /// HIGHFLY physical stats remain owned by the real-training layer.
    /// </summary>
    public sealed class HighflyHunterProgression : MonoBehaviour
    {
        private const string LevelKey = "HIGHFLY_LEVEL";
        private const string ExperienceKey = "HIGHFLY_XP";
        private const string GoldKey = "HIGHFLY_GOLD";
        private const string GateKeysKey = "HIGHFLY_GATE_KEYS";

        public int Level { get; private set; } = 1;
        public int Experience { get; private set; }
        public int Gold { get; private set; }
        public int GateKeys { get; private set; }

        public int ExperienceToNextLevel => GetExperienceRequired(Level);
        public float ExperienceNormalized => Level >= 100 ? 1f :
            Mathf.Clamp01((float)Experience / Mathf.Max(1, ExperienceToNextLevel));

        public string Rank
        {
            get
            {
                if (Level >= 100) return "NACIONAL";
                if (Level >= 90) return "SSS";
                if (Level >= 80) return "SS";
                if (Level >= 70) return "S";
                if (Level >= 60) return "A";
                if (Level >= 50) return "B";
                if (Level >= 40) return "C";
                if (Level >= 30) return "D";
                if (Level >= 20) return "E";
                return "F";
            }
        }

        public event Action Changed;
        public event Action<int> LevelUp;

        private void Awake()
        {
            Level = Mathf.Clamp(PlayerPrefs.GetInt(LevelKey, 1), 1, 100);
            Experience = Mathf.Max(0, PlayerPrefs.GetInt(ExperienceKey, 0));
            Gold = Mathf.Max(0, PlayerPrefs.GetInt(GoldKey, 0));
            GateKeys = Mathf.Max(0, PlayerPrefs.GetInt(GateKeysKey, 0));

            if (Level >= 100)
                Experience = 0;
        }

        public void AddExperience(int amount)
        {
            if (amount <= 0 || Level >= 100)
                return;

            Experience += amount;

            while (Level < 100)
            {
                int required = GetExperienceRequired(Level);
                if (Experience < required)
                    break;

                Experience -= required;
                Level++;
                LevelUp?.Invoke(Level);
            }

            if (Level >= 100)
                Experience = 0;

            Save();
        }

        public void AddGold(int amount)
        {
            if (amount <= 0)
                return;

            Gold = Mathf.Max(0, Gold + amount);
            Save();
        }

        public void AddGateKeys(int amount)
        {
            if (amount <= 0)
                return;

            GateKeys = Mathf.Max(0, GateKeys + amount);
            Save();
        }

        private static int GetExperienceRequired(int level)
        {
            level = Mathf.Clamp(level, 1, 99);
            return 100 + ((level - 1) * 35);
        }

        private void Save()
        {
            PlayerPrefs.SetInt(LevelKey, Level);
            PlayerPrefs.SetInt(ExperienceKey, Experience);
            PlayerPrefs.SetInt(GoldKey, Gold);
            PlayerPrefs.SetInt(GateKeysKey, GateKeys);
            PlayerPrefs.Save();
            Changed?.Invoke();
        }
    }
}
