using UnityEngine;
using UnityEngine.UI;
using Highfly.Core;

namespace Highfly.Mobile
{
    /// <summary>Displays the currently selected touch walk/run speed mode.</summary>
    public sealed class HighflyFuryMoveModeLabel : MonoBehaviour
    {
        [SerializeField] private HighflyThirdPersonMotor motor;
        [SerializeField] private Text readout;
        private bool _lastMode;
        private bool _initialized;

        public void Configure(HighflyThirdPersonMotor value, Text text)
        {
            motor = value;
            readout = text;
            _initialized = false;
            Refresh();
        }

        private void OnEnable()
        {
            _initialized = false;
            Refresh();
        }

        private void Update() { Refresh(); }

        private void Refresh()
        {
            if (motor == null || readout == null) return;
            bool running = motor.IsRunningMode;
            if (_initialized && running == _lastMode) return;
            readout.text = running ? "CORRER\nTOCAR: CAMINAR" : "CAMINAR\nTOCAR: CORRER";
            _lastMode = running;
            _initialized = true;
        }
    }
}
