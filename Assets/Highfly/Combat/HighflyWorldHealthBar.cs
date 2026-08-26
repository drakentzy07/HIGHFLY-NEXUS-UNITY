using UnityEngine;
using UnityEngine.UI;

namespace Highfly.Combat
{
    public sealed class HighflyWorldHealthBar : MonoBehaviour
    {
        [SerializeField] private HighflyHealth health;
        [SerializeField] private Image fill;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private bool hideWhenFull;

        private void Start()
        {
            Refresh();
        }

        private void OnEnable()
        {
            if (health != null)
            {
                health.HealthChanged += OnHealthChanged;
                health.Died += OnDied;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.HealthChanged -= OnHealthChanged;
                health.Died -= OnDied;
            }
        }

        private void LateUpdate()
        {
            Camera camera = Camera.main;
            if (camera != null)
                transform.rotation = Quaternion.LookRotation(transform.position - camera.transform.position, Vector3.up);
        }

        private void OnHealthChanged(float current, float max)
        {
            Refresh();
        }

        private void OnDied(HighflyHealth value)
        {
            if (canvasGroup != null)
                canvasGroup.alpha = 0f;
        }

        private void Refresh()
        {
            if (health == null)
                return;

            if (fill != null)
                fill.fillAmount = health.Normalized;

            if (canvasGroup != null)
                canvasGroup.alpha = hideWhenFull && health.Normalized >= 0.999f ? 0f : 1f;
        }
    }
}
