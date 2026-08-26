using UnityEngine;
using Highfly.Combat;

namespace Highfly.Mobile
{
    public sealed class HighflyThirdPersonCamera : MonoBehaviour
    {
        [SerializeField] private Transform followTarget;
        [SerializeField] private HighflyCameraLookArea lookArea;
        [SerializeField] private HighflyTargetingSystem targeting;

        [Header("Orbit")]
        [SerializeField] private float distance = 5.5f;
        [SerializeField] private float height = 1.55f;
        [SerializeField] private float yaw = 0f;
        [SerializeField] private float pitch = 14f;
        [SerializeField] private float minPitch = -15f;
        [SerializeField] private float maxPitch = 55f;
        [SerializeField] private float positionSharpness = 18f;

        [Header("Collision")]
        [SerializeField] private LayerMask collisionMask = ~0;
        [SerializeField] private float collisionRadius = 0.2f;
        [SerializeField] private float minDistance = 1.1f;

        [Header("Soft target assist")]
        [SerializeField] private bool targetAssist = true;
        [SerializeField, Range(0f, 1f)] private float assistStrength = 0.08f;

        private Vector3 _velocity;

        public void SetFollowTarget(Transform target)
        {
            followTarget = target;
        }

        private void LateUpdate()
        {
            if (followTarget == null)
                return;

            Vector2 look = lookArea != null ? lookArea.ConsumeDelta() : Vector2.zero;

#if UNITY_EDITOR || UNITY_STANDALONE
            if (Input.GetMouseButton(1))
                look += new Vector2(Input.GetAxis("Mouse X") * 3f, Input.GetAxis("Mouse Y") * 3f);
#endif

            yaw += look.x;
            pitch -= look.y;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            if (targetAssist && targeting != null && targeting.HasTarget && (lookArea == null || !lookArea.IsDragging))
            {
                Vector3 local = targeting.CurrentTarget.AimPoint.position - followTarget.position;
                if (local.sqrMagnitude > 0.01f)
                {
                    float desiredYaw = Quaternion.LookRotation(local.normalized, Vector3.up).eulerAngles.y;
                    yaw = Mathf.LerpAngle(yaw, desiredYaw, assistStrength);
                }
            }

            Vector3 pivot = followTarget.position + Vector3.up * height;
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 desiredDirection = rotation * Vector3.back;
            float resolvedDistance = ResolveCollisionDistance(pivot, desiredDirection);
            Vector3 desiredPosition = pivot + desiredDirection * resolvedDistance;

            float t = 1f - Mathf.Exp(-positionSharpness * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, t);
            transform.rotation = rotation;
        }

        private float ResolveCollisionDistance(Vector3 pivot, Vector3 direction)
        {
            RaycastHit[] hits = Physics.SphereCastAll(
                pivot,
                collisionRadius,
                direction,
                distance,
                collisionMask,
                QueryTriggerInteraction.Ignore);

            float nearest = distance;
            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit hit = hits[i];
                if (hit.collider == null)
                    continue;

                Transform hitTransform = hit.collider.transform;
                if (followTarget != null &&
                    (hitTransform == followTarget || hitTransform.IsChildOf(followTarget)))
                    continue;

                if (hit.distance < nearest)
                    nearest = hit.distance;
            }

            if (nearest < distance)
                return Mathf.Clamp(nearest - collisionRadius, minDistance, distance);

            return distance;
        }
    }
}
