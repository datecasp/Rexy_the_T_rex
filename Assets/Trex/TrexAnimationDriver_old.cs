using UnityEngine;

[RequireComponent(typeof(Animator))]
public class TrexAnimationDriver_old : MonoBehaviour
{
    Animator anim;
    void Awake() { anim = GetComponent<Animator>(); }
    public void Idle() => anim.Play("Idle");
    public void Walk() => anim.Play("Walk");
    public void Run() => anim.Play("Run");
    public void Jump() => anim.Play("Jump", 0, 0f);
    public void Roar() => anim.Play("Roar", 0, 0f);
    public void Die() => anim.Play("Die", 0, 0f);
}
