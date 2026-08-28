using UnityEngine;

namespace Highfly.World
{
    /// <summary>
    /// Lightweight ambient city life. Keeps citizens walking around an authored home point.
    /// </summary>
    public sealed class HighflyNpcWander : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private float radius = 5f;
        [SerializeField] private float speed = 0.85f;
        [SerializeField] private float pauseMin = 1.5f;
        [SerializeField] private float pauseMax = 4.5f;

        private Vector3 _home;
        private Vector3 _target;
        private float _waitUntil;
        private bool _hasTarget;

        private void Awake()
        {
            _home = transform.position;
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
        }

        private void Update()
        {
            if (Time.time < _waitUntil)
            {
                SetMoving(false);
                return;
            }

            if (!_hasTarget)
            {
                Vector2 circle = Random.insideUnitCircle * Mathf.Max(0.5f, radius);
                _target = _home + new Vector3(circle.x, 0f, circle.y);
                _hasTarget = true;
            }

            Vector3 delta = _target - transform.position;
            delta.y = 0f;

            if (delta.sqrMagnitude < 0.18f)
            {
                _hasTarget = false;
                _waitUntil = Time.time + Random.Range(pauseMin, pauseMax);
                SetMoving(false);
                return;
            }

            Vector3 direction = delta.normalized;
            transform.position += direction * speed * Time.deltaTime;
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(direction, Vector3.up),
                1f - Mathf.Exp(-8f * Time.deltaTime));

            SetMoving(true);
        }

        private void SetMoving(bool moving)
        {
            if (animator == null)
                return;

            if (HasParameter("MoveSpeed"))
                animator.SetFloat("MoveSpeed", moving ? 0.35f : 0f, 0.12f, Time.deltaTime);
            if (HasParameter("IsMoving"))
                animator.SetBool("IsMoving", moving);
        }

        private bool HasParameter(string name)
        {
            AnimatorControllerParameter[] parameters = animator.parameters;
            for (int i = 0; i < parameters.Length; i++)
                if (parameters[i].name == name)
                    return true;
            return false;
        }
    }
}
