using UnityEngine;
using UnityEngine.EventSystems;

namespace Highfly.Mobile
{
    /// <summary>
    /// Dedicated camera touch zone. Put this RectTransform over the RIGHT side
    /// of the screen. Movement/joystick touches never reach this component.
    /// </summary>
    public sealed class HighflyCameraLookArea : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [SerializeField] private float sensitivity = 0.18f;
        [SerializeField] private bool invertY;

        private Vector2 _accumulatedDelta;
        private int _activePointerId = int.MinValue;

        public bool IsDragging { get; private set; }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (IsDragging)
                return;

            IsDragging = true;
            _activePointerId = eventData.pointerId;
            _accumulatedDelta = Vector2.zero;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!IsDragging || eventData.pointerId != _activePointerId)
                return;

            Vector2 delta = eventData.delta * sensitivity;
            if (invertY)
                delta.y *= -1f;

            _accumulatedDelta += delta;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId != _activePointerId)
                return;

            IsDragging = false;
            _activePointerId = int.MinValue;
        }

        public Vector2 ConsumeDelta()
        {
            Vector2 result = _accumulatedDelta;
            _accumulatedDelta = Vector2.zero;
            return result;
        }
    }
}
