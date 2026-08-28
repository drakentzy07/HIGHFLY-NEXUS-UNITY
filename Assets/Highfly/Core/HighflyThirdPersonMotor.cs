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

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5.2f;
        [SerializeField] private float sprintSpeed = 7.2f;
        [SerializeField] private float rotationSharpness = 15f;
        [SerializeField] private float acceleration = 18f;
        [SerializeField] private float gravity = -24f;

        [Header("Dash")]
        [SerializeField] private float defaultDashDistance = 4.2f;
        [SerializeField] private float defaultDashDuration = 0.16f;

        private CharacterController _controller;
        private Vector3 _planarVelocity;
        private float _verticalVelocity;
        private bool _isDashing;

        public Vector3 LastMoveDirection { get; private set; } = Vector3.forward;
        public bool IsMoving { get; private set; }
        public bool IsDashing => _isDashing;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            if (cameraTransform == null && Camera.main != null)
                cameraTransform = Camera.main.transform;
        }

        private void Update()
        {
            if (_isDashing)
                return;

            Vector2 input = movementJoystick != null ? movementJoystick.Value : Vector2.zero;

#if UNITY_EDITOR || UNITY_STANDALONE
            if (input.sqrMagnitude < 0.001f)
                input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#endif

            input = Vector2.ClampMagnitude(input, 1f);
            Vector3 desiredDirection = GetCameraRelativeDirection(input);
            IsMoving = desiredDirection.sqrMagnitude > 0.001f;

            if (IsMoving)
            {
                LastMoveDirection = desiredDirection.normalized;
                Quaternion targetRotation = Quaternion.LookRotation(LastMoveDirection, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 1f - Mathf.Exp(-rotationSharpness * Time.deltaTime));
            }

            float targetSpeed = moveSpeed * input.magnitude;
            Vector3 desiredVelocity = desiredDirection.normalized * targetSpeed;
            _planarVelocity = Vector3.MoveTowards(_planarVelocity, desiredVelocity, acceleration * Time.deltaTime);

            if (_controller.isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = -2f;
            else
                _verticalVelocity += gravity * Time.deltaTime;

            Vector3 velocity = _planarVelocity + Vector3.up * _verticalVelocity;
            _controller.Move(velocity * Time.deltaTime);

            if (animator != null)
            {
                animator.SetFloat("MoveSpeed", input.magnitude, 0.1f, Time.deltaTime);
                animator.SetBool("IsMoving", IsMoving);
            }
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
            forward.Normalize();
            right.Normalize();
            return forward * input.y + right * input.x;
        }

        public void ResetMotion()
        {
            _planarVelocity = Vector3.zero;
            _verticalVelocity = -2f;
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

            StartCoroutine(DashRoutine(direction.normalized, Mathf.Max(0.1f, distance), Mathf.Max(0.05f, duration)));
        }

        private IEnumerator DashRoutine(Vector3 direction, float distance, float duration)
        {
            _isDashing = true;
            if (animator != null)
                animator.SetTrigger("Dash");

            float elapsed = 0f;
            while (elapsed < duration)
            {
                float dt = Time.deltaTime;
                elapsed += dt;
                _controller.Move(direction * (distance / duration) * dt);
                yield return null;
            }

            _isDashing = false;
        }
    }
}
