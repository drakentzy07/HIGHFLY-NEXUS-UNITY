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
        [SerializeField] private float deceleration = 55f;
        [SerializeField] private float inputDeadZone = 0.06f;
        [SerializeField] private float gravity = -24f;

        [Header("Swimming")]
        [SerializeField] private float swimSpeed = 3.6f;
        [SerializeField] private float swimAcceleration = 12f;
        [SerializeField] private float swimDeceleration = 28f;
        [SerializeField] private float swimSurfaceOffset = 1.05f;
        [SerializeField] private float swimBuoyancy = 6.5f;
        [SerializeField] private float maxSwimVerticalSpeed = 2.4f;
        [SerializeField] private float wadeStartDepth = 0.28f;
        [SerializeField] private float swimStartDepth = 0.92f;
        [SerializeField] private float wadeSpeed = 3.15f;

        [Header("Dash")]
        [SerializeField] private float defaultDashDistance = 4.2f;
        [SerializeField] private float defaultDashDuration = 0.16f;

        [Header("FURY 360 2.0 • jumping")]
        [SerializeField] private float firstJumpVelocity = 10.2f;
        [SerializeField] private float secondJumpVelocity = 10.8f;
        [SerializeField] private float coyoteSeconds = 0.12f;
        [SerializeField] private float jumpBufferSeconds = 0.12f;
        [SerializeField] private float flipDuration = 0.54f;
        [SerializeField] private Transform flipVisual;
        private float _lastGroundedTime = -100f;
        private float _queuedJumpUntil = -100f;
        private int _jumpCount;
        private Coroutine _flipRoutine;
        private Quaternion _originalVisualRotation;

        private CharacterController _controller;
        private Vector3 _planarVelocity;
        private float _verticalVelocity;
        private bool _isDashing;
        private int _waterContacts;
        private float _swimSurfaceY;

        public Vector3 LastMoveDirection { get; private set; } = Vector3.forward;
        public bool IsMoving { get; private set; }
        public bool IsDashing => _isDashing;
        public bool ExternalMovementLock { get; set; }
        public int JumpCount => _jumpCount;
        public bool IsAirborne => _controller != null && !_controller.isGrounded;
        public bool IsDoingSomersault => _flipRoutine != null;
        public bool IsInAerialComboWindow => IsAirborne && _jumpCount >= 2;
        public bool IsInWater => _waterContacts > 0;
        public float WaterDepth
        {
            get
            {
                if (_waterContacts <= 0 || _controller == null)
                    return 0f;

                return Mathf.Max(
                    0f,
                    _swimSurfaceY - _controller.bounds.min.y);
            }
        }

        public bool IsWading =>
            _waterContacts > 0 &&
            WaterDepth >= wadeStartDepth &&
            WaterDepth < swimStartDepth;

        public bool IsSwimming =>
            _waterContacts > 0 &&
            WaterDepth >= swimStartDepth;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            if (cameraTransform == null && Camera.main != null)
                cameraTransform = Camera.main.transform;
            if (flipVisual == null && animator != null && animator.transform != transform)
                flipVisual = animator.transform;
            if (flipVisual != null)
                _originalVisualRotation = flipVisual.localRotation;
        }

        private void Update()
        {
            if (_isDashing)
                return;
            if (_controller.isGrounded)
            {
                _lastGroundedTime = Time.time;
                if (_verticalVelocity <= 0f)
                    _jumpCount = 0;
                if (_flipRoutine == null && flipVisual != null)
                    flipVisual.localRotation = _originalVisualRotation;
            }
            if (_queuedJumpUntil >= Time.time)
                ConsumeJumpRequest();

            Vector2 input = ExternalMovementLock ? Vector2.zero : (movementJoystick != null ? movementJoystick.Value : Vector2.zero);

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
            if (!ExternalMovementLock && input.sqrMagnitude < 0.001f)
                input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#endif

            input = Vector2.ClampMagnitude(input, 1f);
            if (input.magnitude < inputDeadZone)
                input = Vector2.zero;

            Vector3 desiredDirection = GetCameraRelativeDirection(input);
            IsMoving = desiredDirection.sqrMagnitude > 0.001f;

            if (IsMoving)
            {
                LastMoveDirection = desiredDirection.normalized;
                Quaternion targetRotation = Quaternion.LookRotation(LastMoveDirection, Vector3.up);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    1f - Mathf.Exp(-rotationSharpness * Time.deltaTime));
            }

            float movementSpeed =
                IsSwimming ? swimSpeed :
                IsWading ? wadeSpeed :
                moveSpeed;

            float targetSpeed = movementSpeed * input.magnitude;
            Vector3 desiredVelocity =
                IsMoving ? desiredDirection.normalized * targetSpeed : Vector3.zero;

            float velocityRate;
            if (IsMoving)
                velocityRate = IsSwimming ? swimAcceleration : acceleration;
            else
                velocityRate = IsSwimming ? swimDeceleration : deceleration;

            if (ExternalMovementLock) _planarVelocity = Vector3.zero;
            _planarVelocity = Vector3.MoveTowards(
                _planarVelocity,
                desiredVelocity,
                velocityRate * Time.deltaTime);

            if (!IsMoving && _planarVelocity.sqrMagnitude < 0.0025f)
                _planarVelocity = Vector3.zero;

            if (IsSwimming)
            {
                float targetY = _swimSurfaceY - swimSurfaceOffset;
                float heightError = targetY - transform.position.y;
                _verticalVelocity = Mathf.Clamp(
                    heightError * swimBuoyancy,
                    -maxSwimVerticalSpeed,
                    maxSwimVerticalSpeed);
            }
            else if (_controller.isGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = -2f;
            }
            else
            {
                _verticalVelocity += gravity * Time.deltaTime;
            }

            Vector3 velocity = _planarVelocity + Vector3.up * _verticalVelocity;
            _controller.Move(velocity * Time.deltaTime);

            if (animator != null)
            {
                float locomotion = IsSwimming ? 0f : input.magnitude;
                animator.SetFloat("MoveSpeed", locomotion, 0.06f, Time.deltaTime);
                animator.SetBool("IsMoving", IsMoving && !IsSwimming);
                SetAnimatorBoolIfPresent("IsSwimming", IsSwimming);
                SetAnimatorBoolIfPresent("IsWading", IsWading);
            }
        }

        /// <summary>Bindable by the HUD jump button; 1st jump and directional 2nd somersault.</summary>
        public void Jump()
        {
            _queuedJumpUntil = Time.time + jumpBufferSeconds;
            ConsumeJumpRequest();
        }

        private void ConsumeJumpRequest()
        {
            if (_queuedJumpUntil < Time.time || _isDashing || ExternalMovementLock ||
                IsSwimming || IsWading) return;
            bool groundGrace = _controller.isGrounded ||
                               (Time.time - _lastGroundedTime) <= coyoteSeconds;
            if (_jumpCount == 0 && groundGrace)
            {
                _jumpCount = 1;
                _verticalVelocity = firstJumpVelocity;
                _queuedJumpUntil = -100f;
                TriggerAnimationIfPresent("Jump");
            }
            else if (_jumpCount == 1 && !_controller.isGrounded)
            {
                _jumpCount = 2;
                _verticalVelocity = secondJumpVelocity;
                _queuedJumpUntil = -100f;
                TriggerAnimationIfPresent("DoubleJump");
                StartFlip();
            }
        }

        private void TriggerAnimationIfPresent(string name)
        {
            if (animator == null) return;
            foreach (AnimatorControllerParameter parameter in animator.parameters)
                if (parameter.name == name &&
                    parameter.type == AnimatorControllerParameterType.Trigger)
                {
                    animator.SetTrigger(name);
                    return;
                }
        }

        private void StartFlip()
        {
            if (flipVisual == null) return;
            if (_flipRoutine != null) StopCoroutine(_flipRoutine);
            flipVisual.localRotation = _originalVisualRotation;
            Vector3 worldDirection = LastMoveDirection;
            worldDirection.y = 0f;
            if (worldDirection.sqrMagnitude < .001f)
                worldDirection = transform.forward;
            Vector3 relative = transform.InverseTransformDirection(worldDirection.normalized);
            Vector3 axis = new Vector3(relative.z, 0f, -relative.x).normalized;
            if (axis.sqrMagnitude < .01f) axis = Vector3.right;
            _flipRoutine = StartCoroutine(FlipVisual(axis));
        }

        private IEnumerator FlipVisual(Vector3 axis)
        {
            float elapsed = 0f;
            while (elapsed < flipDuration && flipVisual != null)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / Mathf.Max(.01f, flipDuration));
                // Smooth acceleration prevents a visual snap on button press.
                float eased = t * t * (3f - 2f * t);
                flipVisual.localRotation = _originalVisualRotation *
                    Quaternion.AngleAxis(-360f * eased, axis);
                yield return null;
            }
            if (flipVisual != null) flipVisual.localRotation = _originalVisualRotation;
            _flipRoutine = null;
        }

        private void OnDisable()
        {
            ExternalMovementLock = false;
            _queuedJumpUntil = -100f;
            if (_flipRoutine != null) StopCoroutine(_flipRoutine);
            _flipRoutine = null;
            if (flipVisual != null) flipVisual.localRotation = _originalVisualRotation;
        }

        public void BindVisual(Animator newAnimator)
        {
            if (newAnimator == null || newAnimator.transform == transform) return;
            if (_flipRoutine != null) StopCoroutine(_flipRoutine);
            _flipRoutine = null;
            if (flipVisual != null) flipVisual.localRotation = _originalVisualRotation;
            animator = newAnimator;
            flipVisual = newAnimator.transform;
            _originalVisualRotation = flipVisual.localRotation;
        }

        public void EnterWater(float surfaceY)
        {
            _waterContacts++;
            _swimSurfaceY = Mathf.Max(_swimSurfaceY, surfaceY);
            if (IsSwimming)
                _verticalVelocity = 0f;

            _planarVelocity *= IsSwimming ? 0.55f : 0.82f;
            SetAnimatorBoolIfPresent("IsSwimming", IsSwimming);
            SetAnimatorBoolIfPresent("IsWading", IsWading);
        }

        public void ExitWater(float surfaceY)
        {
            _waterContacts = Mathf.Max(0, _waterContacts - 1);
            if (_waterContacts == 0)
            {
                _swimSurfaceY = 0f;
                _verticalVelocity = -1f;
                SetAnimatorBoolIfPresent("IsSwimming", false);
                SetAnimatorBoolIfPresent("IsWading", false);
            }
        }

        private void SetAnimatorBoolIfPresent(string parameterName, bool value)
        {
            if (animator == null)
                return;

            AnimatorControllerParameter[] parameters = animator.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                AnimatorControllerParameter parameter = parameters[i];
                if (parameter.type == AnimatorControllerParameterType.Bool &&
                    string.Equals(parameter.name, parameterName, System.StringComparison.Ordinal))
                {
                    animator.SetBool(parameterName, value);
                    return;
                }
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
            if (_isDashing || IsSwimming || IsWading || !isActiveAndEnabled)
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
