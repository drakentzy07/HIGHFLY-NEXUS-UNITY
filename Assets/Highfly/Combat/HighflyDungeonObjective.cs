using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Highfly.Core;

namespace Highfly.Combat
{
    public sealed class HighflyDungeonObjective : MonoBehaviour
    {
        [SerializeField] private Text objectiveText;
        [SerializeField] private int enemyLayer = 8;
        [SerializeField] private string activeLabel = "CRIPTA F";
        [SerializeField] private string clearLabel = "CRIPTA LIMPIA";
        [SerializeField] private int clearExperienceReward = 120;
        [SerializeField] private int clearGoldReward = 75;

        private readonly List<HighflyHealth> _tracked = new List<HighflyHealth>();
        private int _defeated;
        private bool _clearRewardGranted;
        private HighflyHunterProgression _progression;

        private void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                _progression = player.GetComponent<HighflyHunterProgression>();

            HighflyHealth[] all = FindObjectsOfType<HighflyHealth>(true);
            for (int i = 0; i < all.Length; i++)
            {
                HighflyHealth health = all[i];
                if (health == null || health.gameObject.layer != enemyLayer)
                    continue;

                _tracked.Add(health);
                health.Died += OnEnemyDied;
            }

            Refresh();
        }

        private void OnDestroy()
        {
            for (int i = 0; i < _tracked.Count; i++)
            {
                if (_tracked[i] != null)
                    _tracked[i].Died -= OnEnemyDied;
            }
        }

        private void OnEnemyDied(HighflyHealth health)
        {
            _defeated++;
            Refresh();
        }

        private void Refresh()
        {
            if (objectiveText == null)
                return;

            int total = _tracked.Count;
            int remaining = Mathf.Max(0, total - _defeated);

            if (total > 0 && remaining == 0)
            {
                if (!_clearRewardGranted)
                {
                    _clearRewardGranted = true;
                    if (_progression != null)
                    {
                        _progression.AddExperience(clearExperienceReward);
                        _progression.AddGold(clearGoldReward);
                    }
                }

                objectiveText.text =
                    clearLabel + "  •  GUARDIÁN DERROTADO" +
                    "  •  +" + clearExperienceReward + " XP" +
                    "  •  +" + clearGoldReward + " ORO";
            }
            else
            {
                objectiveText.text = activeLabel + "  •  ENEMIGOS " + remaining + " / " + total;
            }
        }
    }
}
