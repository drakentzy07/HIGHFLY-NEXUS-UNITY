using UnityEngine;
using UnityEngine.EventSystems;

namespace Highfly.Mobile
{
    /// <summary>
    /// Android-first floating/fixed virtual joystick. It only reports movement;
    /// camera input is deliberately handled by HighflyCameraLookArea so touches
    /// on the left side can never rotate the camera.
    /// </summary>
    public sealed class HighflyVirtualJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [SerializeField] private RectTransform background;
        [SerializeField] private RectTransform handle;
        [SerializeField, Range(0.1f, 1f)] private float handleRange = 0.55f;
        [SerializeField, Range(0f, 0.5f)] private float deadZone = 0.08f;

        public Vector2 Value { get; private set; }
        public bool IsHeld { get; private set; }

        private Camera _uiCamera;

        private void Awake()
        {
            if (background == null)
                background = transform as RectTransform;

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                _uiCamera = canvas.worldCamera;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            IsHeld = true;
            UpdateValue(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (IsHeld)
                UpdateValue(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            IsHeld = false;
            Value = Vector2.zero;
            if (handle != null)
                handle.anchoredPosition = Vector2.zero;
        }

        private void UpdateValue(PointerEventData eventData)
        {
            if (background == null)
                return;

            Vector2 localPoint;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(background, eventData.position, _uiCamera, out localPoint))
                return;

            Vector2 halfSize = background.rect.size * 0.5f;
            if (halfSize.x <= 0f || halfSize.y <= 0f)
                return;

            Vector2 normalized = new Vector2(localPoint.x / halfSize.x, localPoint.y / halfSize.y);
            normalized = Vector2.ClampMagnitude(normalized, 1f);

            float magnitude = normalized.magnitude;
            if (magnitude < deadZone)
                normalized = Vector2.zero;
            else if (magnitude > 0f)
                normalized *= Mathf.InverseLerp(deadZone, 1f, magnitude) / magnitude;

            Value = normalized;

            if (handle != null)
            {
                float radius = Mathf.Min(halfSize.x, halfSize.y) * handleRange;
                handle.anchoredPosition = normalized * radius;
            }
        }
    }
}
