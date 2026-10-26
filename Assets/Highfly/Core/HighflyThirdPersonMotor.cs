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
        [SerializeField] private float walkSpeed = 2.8f;
        [SerializeField] private float moveSpeed = 5.2f;
        [SerializeField] private bool runModeByDefault = true;
        private bool _runMode;
        [SerializeField] private float sprintSpeed = 7.2f;
        [SerializeField] private float rotationSharpness = 15f;
        [SerializeField] private float acceleration = 18f;
        [SerializeField] private float deceleration = 28f;
        [Header("Aerial control: keep run momentum during the jump")]
        [SerializeField] private float airAcceleration = 7.5f;
        [SerializeField] private float airDeceleration = 1.6f;
        [SerializeField] private float minDoubleJumpInterval = 0.11f;
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
        private Vector2 _lastDirectionalInput = Vector2.up;
        private Coroutine _flipRoutine;
        private Quaternion _originalVisualRotation;
        private Vector3 _originalVisualPosition;
        private float _takeoffTime = -100f;
        [SerializeField] private float flipPivotHeight = 0.92f;

        private CharacterController _controller;
        // Non-alloc ground probe supports standing jumps on uneven RG POLY mesh.
        // isGrounded alone can miss the first frame at rest on prefab surfaces.
        private readonly RaycastHit[] _groundHits = new RaycastHit[12];
        private bool _groundContact;
        private Vector3 _planarVelocity;
        private float _verticalVelocity;
        private bool _isDashing;
        private Vector3 _dashDirection;
        private float _dashElapsed;
        private float _dashDuration;
        private float _dashDistance;
        private Vector3 _facingForAction;
        private float _facingRequestUntil = -100f;
        private int _waterContacts;
        private float _swimSurfaceY;

        public Vector3 LastMoveDirection { get; private set; } = Vector3.forward;
        public bool IsMoving { get; private set; }
        public bool IsRunningMode => _runMode;

        /// <summary>Switch walk/run without changing the analog joystick or camera.</summary>
        public void ToggleMoveMode() { _runMode = !_runMode; }
        public bool IsDashing => _isDashing;
        public enum MotionPhase { Idle, Moving, Rising, Falling, Dashing, Swimming, Wading }
        public MotionPhase CurrentPhase { get; private set; }
        public bool IsGrounded => _groundContact && _jumpCount == 0 && _verticalVelocity <= 0f;
        public float CurrentPlanarSpeed => _planarVelocity.magnitude;
        public float CurrentVerticalSpeed => _verticalVelocity;
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
            _runMode = runModeByDefault;
            if (cameraTransform == null && Camera.main != null)
                cameraTransform = Camera.main.transform;
            if (flipVisual == null && animator != null && animator.transform != transform)
                flipVisual = animator.transform;
            if (flipVisual != null)
            {
                _originalVisualRotation = flipVisual.localRotation;
                _originalVisualPosition = flipVisual.localPosition;
            }
        }

        private void Update()
        {
            // One authoritative Update/CharacterController.Move per frame for
            // locomotion, gravity, jumps and dash. No early return during dash.
            _groundContact = _controller.isGrounded ||
                (_jumpCount == 0 && _verticalVelocity <= 0f && ProbeNearGround());
            if (_groundContact && _verticalVelocity <= 0f)
            {
                _lastGroundedTime = Time.time;
                _jumpCount = 0;
                if (_flipRoutine == null && flipVisual != null)
                {
                    flipVisual.localRotation = _originalVisualRotation;
                    // Foot calibration is owned by HighflyAnimationBalance, so
                    // do not force an obsolete position into the visual every frame.
                }
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
            // Remember the CAMERA-relative input before facing/turning changes
            // the actor's transform. Required for 4-direction FURY aerial flips.
            if (input.sqrMagnitude > inputDeadZone * inputDeadZone)
                _lastDirectionalInput = input.normalized;

            Vector3 desiredDirection = GetCameraRelativeDirection(input);
            IsMoving = desiredDirection.sqrMagnitude > 0.001f;

            if (IsMoving) LastMoveDirection = desiredDirection.normalized;
            // Combat asks FURY to steer facing: no direct second transform
            // rotation in a different Update order, preventing 1-frame snaps.
            Vector3 facing = Time.time <= _facingRequestUntil ?
                _facingForAction : IsMoving ? LastMoveDirection : Vector3.zero;
            if (facing.sqrMagnitude > .01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(facing, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    targetRotation,
                    1f - Mathf.Exp(-rotationSharpness * Time.deltaTime));
            }

            float movementSpeed =
                IsSwimming ? swimSpeed :
                IsWading ? wadeSpeed :
                _runMode
                    ? Mathf.Lerp(moveSpeed, sprintSpeed,
                        Mathf.InverseLerp(.60f, .95f, input.magnitude))
                    : walkSpeed;

            float targetSpeed = movementSpeed * input.magnitude;
            Vector3 desiredVelocity =
                IsMoving ? desiredDirection.normalized * targetSpeed : Vector3.zero;

            bool airborne = _jumpCount > 0 || (!_groundContact && !IsSwimming);
            float velocityRate;
            if (IsMoving)
                velocityRate = IsSwimming ? swimAcceleration :
                    airborne ? airAcceleration : acceleration;
            else
                velocityRate = IsSwimming ? swimDeceleration :
                    airborne ? airDeceleration : deceleration;
            // When the player lets go of the left stick to press jump/ATK,
            // airborne velocity decays slowly instead of dropping to zero
            // in ~0.13 s as in the previous FURY implementation.

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
            else if (_groundContact && _verticalVelocity < 0f)
            {
                _verticalVelocity = -2f;
            }
            else
            {
                _verticalVelocity += gravity * Time.deltaTime;
            }

            Vector3 planarStep = _planarVelocity * Time.deltaTime;
            if (_isDashing)
            {
                // Smooth-start/smooth-end displacement: no teleport-like
                // constant-speed coroutine, no suspension of vertical physics.
                float from = Mathf.Clamp01(_dashElapsed / _dashDuration);
                _dashElapsed = Mathf.Min(_dashDuration, _dashElapsed + Time.deltaTime);
                float to = Mathf.Clamp01(_dashElapsed / _dashDuration);
                float easeFrom = from * from * (3f - 2f * from);
                float easeTo = to * to * (3f - 2f * to);
                planarStep = _dashDirection * (_dashDistance * (easeTo - easeFrom));
                if (_dashElapsed >= _dashDuration)
                {
                    _isDashing = false;
                    // Resume at a controllable run speed, not the huge dash
                    // displacement-per-frame speed.
                    _planarVelocity = _dashDirection *
                        Mathf.Max(_planarVelocity.magnitude, moveSpeed * .65f);
                }
            }

            CollisionFlags collision = _controller.Move(
                planarStep + Vector3.up * (_verticalVelocity * Time.deltaTime));
            if ((collision & CollisionFlags.Above) != 0 && _verticalVelocity > 0f)
                _verticalVelocity = 0f;
            if ((collision & CollisionFlags.Below) != 0 && _verticalVelocity <= 0f)
            {
                if (_jumpCount > 0 && _flipRoutine != null)
                    StopAndRestoreFlip();
                _verticalVelocity = -2f;
                _jumpCount = 0;
                _lastGroundedTime = Time.time;
                _groundContact = true;
            }
            else
            {
                // A "nearby" floor is NOT a landing while we are airborne.
                // Only a below-collision ends an in-progress jump.
                _groundContact = _jumpCount == 0 && _verticalVelocity <= 0f &&
                    ProbeNearGround();
            }

            CurrentPhase = IsSwimming ? MotionPhase.Swimming :
                IsWading ? MotionPhase.Wading :
                _isDashing ? MotionPhase.Dashing :
                _jumpCount > 0 || !_groundContact ?
                    (_verticalVelocity > 0f ? MotionPhase.Rising : MotionPhase.Falling) :
                _planarVelocity.sqrMagnitude > .025f ?
                    MotionPhase.Moving : MotionPhase.Idle;

            if (animator != null)
            {
                // Animate REAL planar velocity, not instantaneous joystick deflection:
                // legs accelerate smoothly rather than snapping to a full-speed cycle.
                float locomotion = IsSwimming ? 0f :
                    Mathf.Clamp01(_planarVelocity.magnitude / Mathf.Max(.01f, sprintSpeed));
                animator.SetFloat("MoveSpeed", locomotion, .10f, Time.deltaTime);
                animator.SetBool("IsMoving",
                    _planarVelocity.sqrMagnitude > .0324f && !IsSwimming);
                animator.SetBool("Grounded", _groundContact &&
                    _verticalVelocity <= 0f && _jumpCount == 0);
                SetAnimatorBoolIfPresent("IsSwimming", IsSwimming);
                SetAnimatorBoolIfPresent("IsWading", IsWading);
            }
        }

        /// <summary>Bindable by the HUD jump button; 1st jump and directional 2nd somersault.</summary>
        public void Jump()
        {
            _queuedJumpUntil = Time.time +
                Mathf.Max(jumpBufferSeconds, _isDashing ? _dashDuration + .08f : jumpBufferSeconds);
            ConsumeJumpRequest();
        }

        private void ConsumeJumpRequest()
        {
            if (_queuedJumpUntil < Time.time || _isDashing || ExternalMovementLock ||
                IsSwimming || IsWading) return;
            bool groundGrace = _groundContact || _controller.isGrounded ||
                               ProbeNearGround() ||
                               (Time.time - _lastGroundedTime) <= coyoteSeconds;
            if (_jumpCount == 0 && groundGrace)
            {
                _jumpCount = 1;
                _takeoffTime = Time.time;
                _groundContact = false;
                _verticalVelocity = firstJumpVelocity;
                _queuedJumpUntil = -100f;
                if (animator != null) animator.SetBool("Grounded", false);
                TriggerAnimationIfPresent("Jump");
                Debug.Log("FURY ACTION Jump1: planarSpeed=" +
                    _planarVelocity.magnitude.ToString("F2") +
                    " vertical=" + _verticalVelocity.ToString("F2"));
            }
            else if (_jumpCount == 1 && !_groundContact &&
                Time.time - _takeoffTime >= minDoubleJumpInterval)
            {
                _jumpCount = 2;
                _verticalVelocity = secondJumpVelocity;
                _queuedJumpUntil = -100f;
                if (animator != null) animator.SetBool("Grounded", false);
                TriggerAnimationIfPresent("DoubleJump");
                StartFlip();
                Debug.Log("FURY ACTION Jump2: planarSpeed=" +
                    _planarVelocity.magnitude.ToString("F2") +
                    " vertical=" + _verticalVelocity.ToString("F2"));
            }
        }

        private bool ProbeNearGround()
        {
            if (_controller == null || !_controller.enabled) return false;
            Bounds b = _controller.bounds;
            // Probe from just above capsule midpoint, ignoring our own rig.
            float radius = Mathf.Max(.08f, _controller.radius);
            Vector3 origin = new Vector3(b.center.x, b.min.y + radius + .22f, b.center.z);
            int count = Physics.RaycastNonAlloc(origin, Vector3.down,
                _groundHits, radius + .56f, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                RaycastHit hit = _groundHits[i];
                if (hit.collider == null || hit.collider.transform.IsChildOf(transform) ||
                    hit.normal.y < .57f) continue;
                float gap = b.min.y - hit.point.y;
                if (gap >= -.05f && gap <= .22f)
                    return true;
            }
            return false;
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

        private void StopAndRestoreFlip()
        {
            if (_flipRoutine != null) StopCoroutine(_flipRoutine);
            _flipRoutine = null;
            if (flipVisual != null)
            {
                flipVisual.localRotation = _originalVisualRotation;
                flipVisual.localPosition = _originalVisualPosition;
            }
        }

        private void StartFlip()
        {
            if (flipVisual == null) return;
            StopAndRestoreFlip();
            // Use CURRENT input: never replay a stale direction from a run
            // completed several seconds ago. A neutral double jump flips forward.
            Vector2 input = movementJoystick != null && movementJoystick.IsHeld ?
                movementJoystick.Value : Vector2.up;
            if (input.sqrMagnitude < .08f) input = Vector2.up;
            Vector3 axis = Mathf.Abs(input.y) >= Mathf.Abs(input.x) ?
                (input.y >= 0f ? Vector3.right : Vector3.left) :
                (input.x >= 0f ? Vector3.back : Vector3.forward);
            _originalVisualRotation = flipVisual.localRotation;
            _originalVisualPosition = flipVisual.localPosition;
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
                Quaternion spin = Quaternion.AngleAxis(-360f * eased, axis);
                flipVisual.localRotation = _originalVisualRotation * spin;
                // Turn around the torso rather than around the ankles/feet.
                // The physics capsule itself stays vertical and collision-safe.
                Vector3 pivot = Vector3.up * flipPivotHeight;
                flipVisual.localPosition = _originalVisualPosition +
                    _originalVisualRotation * (pivot - spin * pivot);
                yield return null;
            }
            if (flipVisual != null)
            {
                flipVisual.localRotation = _originalVisualRotation;
                flipVisual.localPosition = _originalVisualPosition;
            }
            _flipRoutine = null;
        }

        private void OnDisable()
        {
            ExternalMovementLock = false;
            _queuedJumpUntil = -100f;
            if (_flipRoutine != null) StopCoroutine(_flipRoutine);
            _flipRoutine = null;
            _isDashing = false;
            if (flipVisual != null)
            {
                flipVisual.localRotation = _originalVisualRotation;
                flipVisual.localPosition = _originalVisualPosition;
            }
        }

        public void BindVisual(Animator newAnimator)
        {
            if (newAnimator == null || newAnimator.transform == transform) return;
            if (_flipRoutine != null) StopCoroutine(_flipRoutine);
            _flipRoutine = null;
            if (flipVisual != null)
            {
                flipVisual.localRotation = _originalVisualRotation;
                flipVisual.localPosition = _originalVisualPosition;
            }
            animator = newAnimator;
            flipVisual = newAnimator.transform;
            _originalVisualRotation = flipVisual.localRotation;
            _originalVisualPosition = flipVisual.localPosition;
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

        public void FaceForAction(Vector3 direction, float seconds = .17f)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude <= .001f) return;
            _facingForAction = direction.normalized;
            _facingRequestUntil = Time.time + Mathf.Max(.03f, seconds);
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
            // Pressing DASH with a neutral joystick is a valid action.
            Dash(IsMoving ? LastMoveDirection : transform.forward,
                defaultDashDistance, defaultDashDuration);
        }

        public void Dash(Vector3 direction)
        {
            Dash(direction, defaultDashDistance, defaultDashDuration);
        }

        public void Dash(Vector3 direction, float distance, float duration)
        {
            Dash(direction, distance, duration, true);
        }

        public void Dash(Vector3 direction, float distance, float duration,
            bool playDashAnimation, bool interruptExisting = false)
        {
            // Skill mobility can override a basic dash for combo chaining.
            if ((_isDashing && !interruptExisting) || ExternalMovementLock ||
                IsSwimming || IsWading || !isActiveAndEnabled) return;

            direction.y = 0f;
            if (direction.sqrMagnitude < .001f) direction = transform.forward;
            _dashDirection = direction.normalized;
            _dashDistance = Mathf.Max(.10f, distance);
            _dashDuration = Mathf.Max(.05f, duration);
            _dashElapsed = 0f;
            _isDashing = true;
            Debug.Log("FURY ACTION Dash: distance=" +
                _dashDistance.ToString("F2") + " duration=" +
                _dashDuration.ToString("F2") +
                " air=" + (_jumpCount > 0));
            // Dash is an ACTION, not a mode that freezes gravity or input.
            if (playDashAnimation) TriggerAnimationIfPresent("Dash");
        }
    }
}
