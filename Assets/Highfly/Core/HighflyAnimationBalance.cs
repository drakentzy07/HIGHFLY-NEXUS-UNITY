using UnityEngine;

namespace Highfly.Core
{
    /// <summary>
    /// Keeps UAL humanoid legs and torso free during normal locomotion.
    /// Only the active combat state may override the upper body; this avoids
    /// the empty FreeUpper override freezing the avatar in a bind pose.
    /// Ground-aligns the visual feet once without moving the physics capsule.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HighflyAnimationBalance : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private CharacterController capsule;
        [SerializeField] private float upperFadeRate = 12f;
        private static readonly int FreeUpper = Animator.StringToHash("FreeUpper");
        private bool aligned;
        private int stableIdleFrames;
        private HighflyThirdPersonMotor motor;

        public void Configure(Animator targetAnimator, CharacterController controller)
        {
            animator = targetAnimator;
            capsule = controller;
            motor = controller != null ? controller.GetComponent<HighflyThirdPersonMotor>() : null;
        }

        private void Start()
        {
            if (animator == null) animator = GetComponentInChildren<Animator>();
            if (capsule == null) capsule = GetComponentInParent<CharacterController>();
            if (animator == null) return;
            if (motor == null && capsule != null)
                motor = capsule.GetComponent<HighflyThirdPersonMotor>();
            if (animator.layerCount > 1) animator.SetLayerWeight(1, 0f);
            // Wait for Mecanim to sample upright Idle_Loop and for FURY to
            // confirm physical ground contact before measuring the ankles.
        }

        private void OnEnable()
        {
            aligned = false;
            stableIdleFrames = 0;
            if (animator != null && animator.layerCount > 1)
                animator.SetLayerWeight(1, 0f);
        }

        private void AlignFootHeight()
        {
            if (aligned || animator == null || capsule == null ||
                !animator.isHuman || !animator.avatar.isValid) return;
            Transform left = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            Transform right = animator.GetBoneTransform(HumanBodyBones.RightFoot);
            if (left == null || right == null) return;
            float footY = Mathf.Min(left.position.y, right.position.y);
            float floor = capsule.bounds.min.y;
            float correction = Mathf.Clamp(floor + .09f - footY, -.22f, .22f);
            transform.position += Vector3.up * correction;
            aligned = true;
            Debug.Log("FURY FEET ALIGNED " + name + " offsetY=" + correction +
                " footY=" + footY + " capsuleBottom=" + floor);
        }

        private void LateUpdate()
        {
            if (animator == null || !animator.isActiveAndEnabled) return;
            if (!aligned && motor != null && motor.IsGrounded &&
                motor.CurrentPlanarSpeed < .10f && !motor.IsDoingSomersault)
            {
                stableIdleFrames++;
                if (stableIdleFrames >= 3) AlignFootHeight();
            }
            else if (!aligned) stableIdleFrames = 0;
            if (animator.layerCount < 2) return;
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(1);
            bool action = state.shortNameHash != FreeUpper;
            if (animator.IsInTransition(1))
            {
                AnimatorStateInfo next = animator.GetNextAnimatorStateInfo(1);
                action = next.shortNameHash != FreeUpper;
            }
            float value = Mathf.MoveTowards(animator.GetLayerWeight(1),
                action ? 1f : 0f, Time.deltaTime * upperFadeRate);
            animator.SetLayerWeight(1, value);
        }
    }
}
