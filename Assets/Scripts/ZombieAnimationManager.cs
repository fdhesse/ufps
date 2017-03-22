using UnityEngine;

public class ZombieAnimationManager : MonoBehaviour
{
    public float AnimationSmoothValue = 1;
    public float AnimationSpeedMultiplier = 1;
    public float DeathAnimationTime = 1.18f;

    private Animator _animator;

    private Vector3 _lastPosition;

    private bool _isDead;

    void Start()
    {
        _animator = GetComponentInChildren<Animator>();
        _lastPosition = transform.position;
    }

    void Update()
    {
        if (_isDead) return;

        if (_lastPosition != transform.position)
            _animator.SetBool("isIdle", false);
        var delta = Vector3.Distance(_lastPosition, transform.position) * AnimationSpeedMultiplier;
        _animator.speed = Mathf.Lerp(_animator.speed, delta, AnimationSmoothValue);
        _lastPosition = transform.position;
    }

    void OnDestroy()
    {
        
    }

    public void TriggerDeathAnimationAndDestroy()
    {
        _isDead = true;
        _animator.SetBool("isDead", true);
        _animator.SetBool("isIdle", true);
        vp_Utility.Destroy(gameObject, DeathAnimationTime);
    }

// add by hxh temporarily, please fix it
    public void Die()
    {
        if( !_isDead )
        {
            TriggerDeathAnimationAndDestroy();
        }        
    }
}
