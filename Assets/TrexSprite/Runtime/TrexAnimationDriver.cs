using UnityEngine;

namespace TrexSprite
{
    [RequireComponent(typeof(Animator))]
    public class TrexAnimationDriver : MonoBehaviour
    {
        private Animator animator;

        private static readonly int Speed =
            Animator.StringToHash("Speed");

        private static readonly int Running =
            Animator.StringToHash("Running");

        private static readonly int JumpTrigger =
            Animator.StringToHash("Jump");

        private static readonly int AttackTrigger =
            Animator.StringToHash("Attack");

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        // Velocidad de movimiento: Idle / Walk
        public void SetMoveSpeed(float speed)
        {
            animator.SetFloat(Speed, Mathf.Abs(speed));
        }

        // Activar o desactivar carrera
        public void SetRunning(bool running)
        {
            animator.SetBool(Running, running);
        }

        // Ejecutar salto
        public void Jump()
        {
            animator.ResetTrigger(AttackTrigger);
            animator.SetTrigger(JumpTrigger);
        }

        // Ejecutar ataque
        public void Attack()
        {
            animator.SetTrigger(AttackTrigger);
        }
    }
}