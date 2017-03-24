using UnityEngine;

public class ZombieAnimationManager : MonoBehaviour
{
    public float DeathAnimationTime = 1.18f;

    private Animator _animator;

    void Start()
    {
        _animator = GetComponentInChildren<Animator>();
    }

    public void TriggerDeathAnimationAndDestroy()
    {
        _animator.SetBool("isDead", true);
        _animator.SetBool("isIdle", true);

        vp_Utility.Destroy(gameObject, DeathAnimationTime);
    }
}
