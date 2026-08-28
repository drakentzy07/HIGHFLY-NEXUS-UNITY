using UnityEngine;

namespace Highfly.World
{
    /// <summary>
    /// Authored, validated location where a living Gate is allowed to appear.
    /// Keeps portals away from doors, roads and random scenery.
    /// </summary>
    public sealed class HighflyGateAnchor : MonoBehaviour
    {
        [SerializeField] private string biome = "LLANURAS";
        [SerializeField] private string allowedRanks = "F,E,D";
        [SerializeField] private GameObject breachRoot;

        public string Biome => biome;
        public GameObject BreachRoot => breachRoot;

        public void Configure(string biomeName, string ranks, GameObject breachEnemies)
        {
            biome = string.IsNullOrEmpty(biomeName) ? "MUNDO" : biomeName;
            allowedRanks = string.IsNullOrEmpty(ranks) ? "F" : ranks;
            breachRoot = breachEnemies;
        }

        public string PickRank()
        {
            string[] ranks = allowedRanks.Split(',');
            if (ranks.Length == 0)
                return "F";

            int index = Random.Range(0, ranks.Length);
            string rank = ranks[index].Trim().ToUpperInvariant();
            return string.IsNullOrEmpty(rank) ? "F" : rank;
        }
    }
}
