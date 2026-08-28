using System.Collections;
using UnityEngine;
using Highfly.Mobile;

namespace Highfly.Core
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class HighflyThirdPersonMotor : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private HighflyVirtualJoystick movementJoystick;
        [SerializeField] private Animator animator;

        [Header("Locomotion")]
        [SerializeField] private float walkSpeed = 2.35f;
        [SerializeField] private float runSpeed = 4.65f;
        [SerializeField] private float runInputThreshold = 0.66f;
        [SerializeField] private float rotationSharpness = 13f;
        [SerializeField] private float acceleration = 12f;
        [SerializeField] private float deceleration = 18f;
        [SerializeField] private float gravity = -24f;
        [SerializeField] private float groundedStickForce = -2.5f;

        [Header("Dash")]
        [SerializeField] private float defaultDashDistance = 4.2f;
        [SerializeField] private float defaultDashDuration = 0.18f;

        private CharacterController _controller;
        private Vector3 _planarVelocity;
        private float _verticalVelocity;
        private bool _isDashing;

        public Vector3 LastMoveDirection { get; private set; } = Vector3.forward;
        public bool IsMoving { get; private set; }
        public bool IsDashing => _isDashing;
        public float CurrentSpeed => new Vector3(_planarVelocity.x, 0f, _planarVelocity.z).magnitude;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            if (cameraTransform == null && Camera.main != null)
                cameraTransform = Camera.main.transform;
        }

        private void Update()
        {
            if (_isDashing)
            {
                UpdateAnimatorFromVelocity();
                return;
            }

            Vector2 input = movementJoystick != null ? movementJoystick.Value : Vector2.zero;

#if UNITY_EDITOR || UNITY_STANDALONE
            if (input.sqrMagnitude < 0.001f)
                input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#endif

            input = Vector2.ClampMagnitude(input, 1f);
            float magnitude = input.magnitude;
            Vector3 desiredDirection = GetCameraRelativeDirection(input);
            IsMoving = magnitude > 0.055f && desiredDirection.sqrMagnitude > 0.001f;

            if (IsMoving)
            {
                LastMoveDirection = desiredDirection.normalized;
                Quaternion targetRotation = Quaternion.LookRotation(LastMoveDirection, Vector3.up);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    1f - Mathf.Exp(-rotationSharpness * Time.deltaTime));
            }

            float targetSpeed = 0f;
            if (IsMoving)
            {
                if (magnitude < runInputThreshold)
                {
                    float walkBlend = Mathf.InverseLerp(0.055f, runInputThreshold, magnitude);
                    targetSpeed = Mathf.Lerp(0.65f, walkSpeed, walkBlend);
                }
                else
                {
                    float runBlend = Mathf.InverseLerp(runInputThreshold, 1f, magnitude);
                    targetSpeed = Mathf.Lerp(walkSpeed, runSpeed, runBlend);
                }
            }

            Vector3 desiredVelocity = IsMoving ? LastMoveDirection * targetSpeed : Vector3.zero;
            float rate = desiredVelocity.sqrMagnitude > _planarVelocity.sqrMagnitude ? acceleration : deceleration;
            _planarVelocity = Vector3.MoveTowards(
                _planarVelocity,
                desiredVelocity,
                Mathf.Max(0.1f, rate) * Time.deltaTime);

            if (_controller.isGrounded)
            {
                if (_verticalVelocity < 0f)
                    _verticalVelocity = groundedStickForce;
            }
            else
            {
                _verticalVelocity += gravity * Time.deltaTime;
            }

            Vector3 velocity = _planarVelocity + Vector3.up * _verticalVelocity;
            _controller.Move(velocity * Time.deltaTime);
            UpdateAnimatorFromVelocity();
        }

        private void UpdateAnimatorFromVelocity()
        {
            if (animator == null)
                return;

            float actualSpeed = _controller != null
                ? new Vector3(_controller.velocity.x, 0f, _controller.velocity.z).magnitude
                : CurrentSpeed;

            float locomotion = actualSpeed <= 0.06f
                ? 0f
                : (actualSpeed <= walkSpeed
                    ? Mathf.Lerp(0.12f, 0.52f, Mathf.InverseLerp(0.1f, walkSpeed, actualSpeed))
                    : Mathf.Lerp(0.55f, 1f, Mathf.InverseLerp(walkSpeed, runSpeed, actualSpeed)));

            animator.SetFloat("MoveSpeed", locomotion, 0.12f, Time.deltaTime);
            animator.SetBool("IsMoving", actualSpeed > 0.08f);
        }

        private Vector3 GetCameraRelativeDirection(Vector2 input)
        {
            if (input.sqrMagnitude < 0.001f)
                return Vector3.zero;

            if (cameraTransform == null)
                return new Vector3(input.x, 0f, input.y);

            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;
            forward.y = 0f;
            right.y = 0f;

            if (forward.sqrMagnitude < 0.001f)
                forward = Vector3.forward;
            if (right.sqrMagnitude < 0.001f)
                right = Vector3.right;

            forward.Normalize();
            right.Normalize();
            Vector3 direction = forward * input.y + right * input.x;
            return direction.sqrMagnitude > 1f ? direction.normalized : direction;
        }

        public void ResetMotion()
        {
            _planarVelocity = Vector3.zero;
            _verticalVelocity = groundedStickForce;
            _isDashing = false;
            IsMoving = false;

            if (animator != null)
            {
                animator.SetFloat("MoveSpeed", 0f);
                animator.SetBool("IsMoving", false);
            }
        }

        public void Dash()
        {
            Dash(LastMoveDirection, defaultDashDistance, defaultDashDuration);
        }

        public void Dash(Vector3 direction)
        {
            Dash(direction, defaultDashDistance, defaultDashDuration);
        }

        public void Dash(Vector3 direction, float distance, float duration)
        {
            if (_isDashing || !isActiveAndEnabled)
                return;

            if (direction.sqrMagnitude < 0.001f)
                direction = transform.forward;

            StartCoroutine(DashRoutine(
                direction.normalized,
                Mathf.Max(0.1f, distance),
                Mathf.Max(0.06f, duration)));
        }

        private IEnumerator DashRoutine(Vector3 direction, float distance, float duration)
        {
            _isDashing = true;
            IsMoving = true;
            LastMoveDirection = direction;

            if (animator != null)
                animator.SetTrigger("Dash");

            float elapsed = 0f;
            while (elapsed < duration)
            {
                float dt = Time.deltaTime;
                elapsed += dt;
                float t = Mathf.Clamp01(elapsed / duration);
                float ease = Mathf.Lerp(1.2f, 0.55f, t);
                _controller.Move(direction * (distance / duration) * ease * dt);
                yield return null;
            }

            _planarVelocity = direction * Mathf.Min(walkSpeed, runSpeed * 0.55f);
            _isDashing = false;
        }
    }
}
