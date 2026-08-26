using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Highfly.UI
{
    public sealed class HighflyHoldActionButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        // Unity does not automatically instantiate serialized UnityEvent fields when a
        // component is created programmatically via AddComponent in a headless build.
        // Keep them initialized so editor-time scene generation can safely register
        // persistent listeners for press/release actions.
        [SerializeField] private UnityEvent onPressed = new UnityEvent();
        [SerializeField] private UnityEvent onReleased = new UnityEvent();

        private bool _held;

        public UnityEvent OnPressed => onPressed;
        public UnityEvent OnReleased => onReleased;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_held)
                return;

            _held = true;
            onPressed?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Release();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_held)
                Release();
        }

        private void OnDisable()
        {
            if (_held)
                Release();
        }

        private void Release()
        {
            if (!_held)
                return;

            _held = false;
            onReleased?.Invoke();
        }
    }
}
