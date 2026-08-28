using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Highfly.Core;

namespace Highfly.Combat
{
    public sealed class HighflyDungeonObjective : MonoBehaviour
    {
        [SerializeField] private Text objectiveText;
        [SerializeField] private Transform trackingRoot;
        [SerializeField] private int enemyLayer = 8;
        [SerializeField] private string activeLabel = "CRIPTA F";
        [SerializeField] private string clearLabel = "CRIPTA LIMPIA";
        [SerializeField] private int clearExperienceReward = 120;
        [SerializeField] private int clearGoldReward = 75;

        private readonly List<HighflyHealth> _tracked = new List<HighflyHealth>();
        private int _defeated;
        private bool _clearRewardGranted;
        private HighflyHunterProgression _progression;
        private HighflyContractJournal _journal;

        public void ConfigureTrackingRoot(Transform root)
        {
            trackingRoot = root;
        }

        private void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _progression = player.GetComponent<HighflyHunterProgression>();
                _journal = player.GetComponent<HighflyContractJournal>();
            }

            HighflyHealth[] all = FindObjectsOfType<HighflyHealth>(true);
            for (int i = 0; i < all.Length; i++)
            {
                HighflyHealth health = all[i];
                if (health == null || health.gameObject.layer != enemyLayer)
                    continue;
                if (trackingRoot != null && !health.transform.IsChildOf(trackingRoot))
                    continue;

                _tracked.Add(health);
                health.Died += OnEnemyDied;
            }

            Refresh();
        }

        private void OnDestroy()
        {
            for (int i = 0; i < _tracked.Count; i++)
                if (_tracked[i] != null)
                    _tracked[i].Died -= OnEnemyDied;
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
                    if (_journal != null)
                        _journal.MarkCriptaFCleared();
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
