using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Highfly.Combat
{
    public sealed class HighflyDungeonObjective : MonoBehaviour
    {
        [SerializeField] private Text objectiveText;
        [SerializeField] private int enemyLayer = 8;
        [SerializeField] private string activeLabel = "CRIPTA F";
        [SerializeField] private string clearLabel = "CRIPTA LIMPIA";

        private readonly List<HighflyHealth> _tracked = new List<HighflyHealth>();
        private int _defeated;

        private void Start()
        {
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
                objectiveText.text = clearLabel + "  •  GUARDIÁN DERROTADO";
            else
                objectiveText.text = activeLabel + "  •  ENEMIGOS " + remaining + " / " + total;
        }
    }
}
