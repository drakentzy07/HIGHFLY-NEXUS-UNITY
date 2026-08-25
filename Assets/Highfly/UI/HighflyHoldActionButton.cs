using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Highfly.UI
{
    public sealed class HighflyHoldActionButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField] private UnityEvent onPressed;
        [SerializeField] private UnityEvent onReleased;

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
