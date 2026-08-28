using UnityEngine;
using UnityEngine.Rendering;

namespace Highfly.World
{
    /// <summary>
    /// Mobile-friendly day/night ambience: one directional light and global ambient/fog changes.
    /// </summary>
    public sealed class HighflyDayNightCycle : MonoBehaviour
    {
        [SerializeField] private Light sun;
        [SerializeField] private float fullDaySeconds = 720f;
        [SerializeField] private float startHour = 10.5f;

        private float _time01;

        public void Configure(Light directionalLight, float startAtHour)
        {
            sun = directionalLight;
            startHour = Mathf.Repeat(startAtHour, 24f);
        }

        private void Start()
        {
            _time01 = Mathf.Repeat(startHour / 24f, 1f);
        }

        private void Update()
        {
            if (sun == null)
                return;

            _time01 = Mathf.Repeat(_time01 + Time.deltaTime / Mathf.Max(120f, fullDaySeconds), 1f);

            float hour = _time01 * 24f;
            float sunAngle = _time01 * 360f - 90f;
            sun.transform.rotation = Quaternion.Euler(sunAngle, -32f, 0f);

            float daylight = Mathf.Clamp01(Mathf.Sin((_time01 - 0.25f) * Mathf.PI * 2f) * 0.9f + 0.25f);
            sun.intensity = Mathf.Lerp(0.18f, 1.05f, daylight);
            sun.color = Color.Lerp(
                new Color(0.42f, 0.53f, 0.90f, 1f),
                new Color(1f, 0.91f, 0.74f, 1f),
                daylight);

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = Color.Lerp(
                new Color(0.035f, 0.055f, 0.12f, 1f),
                new Color(0.38f, 0.43f, 0.50f, 1f),
                daylight);
            RenderSettings.fogColor = Color.Lerp(
                new Color(0.02f, 0.03f, 0.075f, 1f),
                new Color(0.48f, 0.63f, 0.72f, 1f),
                daylight);

            // Avoid an unused local warning while keeping hour available in debugger.
            if (hour < 0f)
                Debug.Log(hour);
        }
    }
}
