using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class AIMoveTowards : HumanizedSpeedAction
{
    public SharedTransform Target;

    public float MaxRotationDelta = 5;
    public float ThresholdCompleteDistance = 0.1f;
    public float AnimationSpeedFactor = 1;

    public bool EnableGrab;
    public float GrabDamageWaitTime = 2;
    public int GradDamagePerHit = 10;

    private Animator _animator;

    private vp_DamageHandler _zombieDamageHandler;

    private GameObject _player;
    private FreezePlayer _freezePlayer;
    private vp_FPPlayerDamageHandler _playerDamageHandler;

    public override void OnAwake()
    {
        base.OnAwake();

        _animator = gameObject.GetComponentInChildren<Animator>();
        _player = GameObject.FindGameObjectWithTag("Player");
        _playerDamageHandler = _player.GetComponent<vp_FPPlayerDamageHandler>();
        _freezePlayer = _player.GetComponentInChildren<FreezePlayer>();
        _zombieDamageHandler = GetComponent<vp_DamageHandler>();
    }

    public override void OnStart()
    {
        _animator.speed = _speed * AnimationSpeedFactor;
    }

    public override TaskStatus OnUpdate()
    {
        if (_animator.GetBool("isDead") || Target.Value == null)
            return TaskStatus.Failure;

        var dst = Vector3.Distance(transform.position, Target.Value.position);
        if (dst < ThresholdCompleteDistance)
        {
            if (EnableGrab)
            {
                if (!_animator.GetBool("isGrabbing"))
                    EnableGrabAnim();
            }
            else
            {
                _animator.SetBool("isIdle", true);
                return TaskStatus.Success;
            }
        }
        else
        {
            transform.position += transform.forward * _speed * Time.deltaTime;
            transform.rotation = Quaternion.RotateTowards(transform.rotation,
                Quaternion.LookRotation(Target.Value.position - transform.position, transform.up), MaxRotationDelta);
        }

        _animator.SetBool("isIdle", false);

        return TaskStatus.Running;
    }

    void EnableGrabAnim()
    {
        transform.LookAt(_player.transform);

        _animator.SetBool("isIdle", false);
        _animator.speed = 1;
        _animator.SetBool("isGrabbing", true);

        StartCoroutine(UnfrostWhenDead());
        StartCoroutine(ApplyPlayerDamage());
    }

    IEnumerator ApplyPlayerDamage()
    {
        while (_zombieDamageHandler.CurrentHealth > 0)
        {
            yield return new WaitForSeconds(GrabDamageWaitTime);
            _playerDamageHandler.Damage(GradDamagePerHit);
        }
    }

    IEnumerator UnfrostWhenDead()
    {
        while (_zombieDamageHandler.CurrentHealth > 0)
        {
            _freezePlayer.Frost = true;
            yield return new WaitForSeconds(.25f);
        }

        _freezePlayer.Frost = false;
        _animator.SetBool("isGrabbing", false);
    }
}
