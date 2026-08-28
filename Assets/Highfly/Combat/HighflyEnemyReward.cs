using UnityEngine;
using Highfly.Core;

namespace Highfly.Combat
{
    [RequireComponent(typeof(HighflyHealth))]
    public sealed class HighflyEnemyReward : MonoBehaviour
    {
        [SerializeField] private HighflyHealth health;
        [SerializeField] private HighflyHunterProgression progression;
        [SerializeField] private int experienceReward = 30;
        [SerializeField] private int goldReward = 10;
        [SerializeField] private int gateKeyReward;

        private bool _awarded;

        private void Awake()
        {
            if (health == null)
                health = GetComponent<HighflyHealth>();

            if (progression == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    progression = player.GetComponent<HighflyHunterProgression>();
            }
        }

        private void OnEnable()
        {
            if (health != null)
                health.Died += OnDied;
        }

        private void OnDisable()
        {
            if (health != null)
                health.Died -= OnDied;
        }

        private void OnDied(HighflyHealth dead)
        {
            if (_awarded || progression == null)
                return;

            _awarded = true;
            progression.AddExperience(experienceReward);
            progression.AddGold(goldReward);
            progression.AddGateKeys(gateKeyReward);

            Debug.Log("HIGHFLY reward: +" + experienceReward + " XP, +" + goldReward +
                      " oro, +" + gateKeyReward + " llaves por " + gameObject.name);
        }
    }
}
