using UnityEngine;

namespace Highfly.World
{
    public sealed class HighflyWorldBillboard : MonoBehaviour
    {
        private void LateUpdate()
        {
            Camera camera = Camera.main;
            if (camera == null)
                return;

            Vector3 direction = transform.position - camera.transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.001f)
                return;

            transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        }
    }
}
