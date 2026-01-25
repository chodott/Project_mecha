using System;
using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private BaseBullet _defaultBullet;
    [SerializeField] private BaseBullet _fullChargeBullet;
    [SerializeField] private Animator _animator;
    [SerializeField] private float _fullChargeTime;
    [SerializeField] private Transform _muzzleTransform;
    [SerializeField] private float _fireCoolDown = 0.2f;

    private Coroutine _firePoseRoutine;
    private float _curChargeTime;
    private bool _isCharging = false;

    protected void Start()
    {
        ObjectPoolManager.Instance.PreloadDefault(_defaultBullet, 10);
        ObjectPoolManager.Instance.PreloadDefault(_fullChargeBullet, 5);
    }

    protected void Update()
    {
        if (_isCharging)
        {
            _curChargeTime += Time.deltaTime;
        }
    }
    public void StartCharging()
    {
        _isCharging = true;
        _animator.SetLayerWeight(1, 1f);
    }

    public void Fire(float direction)
    {
        BaseBullet bulletPrefab =  _curChargeTime >= _fullChargeTime ? _fullChargeBullet : _defaultBullet;

        Vector3 directionVector = transform.right * direction;
        float xOffset = MathF.Abs(_muzzleTransform.localPosition.x) * direction;
        Vector3 launchPosition = transform.position + new Vector3(xOffset, _muzzleTransform.localPosition.y, 0);

        BaseBullet newBullet =  ObjectPoolManager.Instance.Get<BaseBullet>(bulletPrefab);
        newBullet.Launch(launchPosition, directionVector);
        _curChargeTime = 0;
        _isCharging = false;

        if(_firePoseRoutine != null)
        {
            StopCoroutine(_firePoseRoutine);
        }
        _firePoseRoutine = StartCoroutine(FirePoseRoutine());
    }

    private IEnumerator FirePoseRoutine()
    {
        _animator.SetTrigger("Fire");
        yield return new WaitForSeconds(_fireCoolDown);
        _animator.SetLayerWeight(1, 0f);
    }
}
