using UnityEngine;
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
            if (motor != null && Input.GetKeyDown(KeyCode.Space)) motor.Jump();
            if (combat == null) return;
            if (Input.GetKeyDown(KeyCode.J)) combat.BasicAttack();
            if (Input.GetKeyDown(KeyCode.Alpha1)) combat.Prisma();
            if (Input.GetKeyDown(KeyCode.Alpha2)) combat.Ruptura();
            if (Input.GetKeyDown(KeyCode.Alpha3)) combat.Tempestad();
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                HighflyCombatController existing = combat.GetComponent<HighflyCombatController>();
                if (existing != null) existing.DashOrDodge();
            }
        }
#endif
    }
}
