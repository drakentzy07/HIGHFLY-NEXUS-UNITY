using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using Highfly.Core;
using Highfly.Combat;

namespace Highfly.Mobile
{
    /// <summary>Testing parity for PC and WebGL. Touch HUD works independently.</summary>
    public sealed class HighflyFuryKeyboardInput : MonoBehaviour
    {
        [SerializeField] private HighflyThirdPersonMotor motor;
        [SerializeField] private HighflyGuinevereCombat combat;

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            // New Input System only: accessing UnityEngine.Input.GetKeyDown
            // generated an exception every WebGL frame, even without a key.
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (motor != null && keyboard.spaceKey.wasPressedThisFrame) motor.Jump();
            if (motor != null && (keyboard.leftShiftKey.wasPressedThisFrame ||
                keyboard.rightShiftKey.wasPressedThisFrame)) motor.Dash();
            if (combat == null) return;
            if (keyboard.jKey.wasPressedThisFrame) combat.BasicAttack();
            if (keyboard.digit1Key.wasPressedThisFrame) combat.Prisma();
            if (keyboard.digit2Key.wasPressedThisFrame) combat.Ruptura();
            if (keyboard.digit3Key.wasPressedThisFrame) combat.Tempestad();
#elif ENABLE_LEGACY_INPUT_MANAGER
            if (motor != null && Input.GetKeyDown(KeyCode.Space)) motor.Jump();
            if (motor != null && Input.GetKeyDown(KeyCode.LeftShift)) motor.Dash();
            if (combat == null) return;
            if (Input.GetKeyDown(KeyCode.J)) combat.BasicAttack();
            if (Input.GetKeyDown(KeyCode.Alpha1)) combat.Prisma();
            if (Input.GetKeyDown(KeyCode.Alpha2)) combat.Ruptura();
            if (Input.GetKeyDown(KeyCode.Alpha3)) combat.Tempestad();
#endif
        }
#endif
    }
}
