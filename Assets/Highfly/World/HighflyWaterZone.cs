using UnityEngine;
using Highfly.Core;

namespace Highfly.World
{
    [DisallowMultipleComponent]
    public sealed class HighflyWaterZone : MonoBehaviour
    {
        [SerializeField] private float surfaceY;

        public void Configure(float waterSurfaceY)
        {
            surfaceY = waterSurfaceY;

            Collider trigger = GetComponent<Collider>();
            if (trigger != null)
                trigger.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            HighflyThirdPersonMotor motor =
                other.GetComponentInParent<HighflyThirdPersonMotor>();

            if (motor != null)
                motor.EnterWater(surfaceY);
        }

        private void OnTriggerExit(Collider other)
        {
            HighflyThirdPersonMotor motor =
                other.GetComponentInParent<HighflyThirdPersonMotor>();

            if (motor != null)
                motor.ExitWater(surfaceY);
        }
    }
}
