using UnityEngine;
using UnityEngine.UI;

namespace Highfly.Combat
{
    /// <summary>
    /// Tiny runtime A/B panel for S23 WebGL visual-quality comparisons.
    /// UI presentation only: does not invoke cast, damage or locomotion.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HighflyGuinevereFxControls : MonoBehaviour
    {
        [SerializeField] private HighflyGuinevereVfx vfx;
        [SerializeField] private Text qualityLabel;
        [SerializeField] private Text premiumLabel;
        [SerializeField] private Text fpsLabel;
        private int _frames;
        private float _elapsed;

        public void Configure(HighflyGuinevereVfx visual, Text quality,
            Text premium, Text fps)
        {
            vfx = visual;
            qualityLabel = quality;
            premiumLabel = premium;
            fpsLabel = fps;
            Refresh();
        }

        public void CycleQuality()
        {
            if (vfx == null) return;
            vfx.SetQuality((vfx.CurrentQuality + 1) % 3);
            Refresh();
        }

        public void TogglePremium()
        {
            if (vfx == null) return;
            vfx.SetPremiumEnabled(!vfx.PremiumEnabled);
            Refresh();
        }

        private void Update()
        {
            _elapsed += Time.unscaledDeltaTime;
            _frames++;
            if (_elapsed < .8f) return;
            if (fpsLabel != null && _elapsed > .001f)
                fpsLabel.text = "FPS " + Mathf.RoundToInt(_frames / _elapsed);
            _frames = 0;
            _elapsed = 0f;
        }

        private void Refresh()
        {
            if (vfx == null) return;
            if (qualityLabel != null)
            {
                string level = vfx.CurrentQuality == 0 ? "FLUIDO" :
                    vfx.CurrentQuality == 1 ? "EQUIL." : "ULTRA";
                qualityLabel.text = "FX " + level;
            }
            if (premiumLabel != null)
                premiumLabel.text = vfx.PremiumEnabled ? "R06 ACTIVO" : "FX BASE";
        }
    }
}
