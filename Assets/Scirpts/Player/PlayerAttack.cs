using System;
using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private GameObject _defaultBullet;
    [SerializeField] private GameObject _fullChargeBullet;
    [SerializeField] private Animator _animator;
    [SerializeField] private float _fullChargeTime;
    [SerializeField] private Transform _muzzleTransform;
    [SerializeField] private float _fireCoolDown = 0.03f;
    private float _curChargeTime;

    private bool _isShooting = false;

    protected void Update()
    {
        if (_isShooting)
        {
            _curChargeTime += Time.deltaTime;
        }
    }
    public void StartCharging()
    {
        _isShooting = true;
        _animator.SetLayerWeight(1, 1f);
    }

    public void Fire(float direction)
    {
        GameObject launchedMissile;
        if (_curChargeTime >= _fullChargeTime)
        {
            launchedMissile = Instantiate(_fullChargeBullet, _muzzleTransform.position, _muzzleTransform.rotation);
        }
        else
        {
            launchedMissile = Instantiate(_defaultBullet, _muzzleTransform.position, _muzzleTransform.rotation);
        }

        Vector3 directionVector = transform.right * direction;
        float xOffset = MathF.Abs(_muzzleTransform.localPosition.x) * direction;
        Vector3 launchPosition = transform.position + new Vector3(xOffset, _muzzleTransform.localPosition.y, 0);
        launchedMissile.GetComponent<BaseBullet>().Launch(launchPosition, directionVector);
        _curChargeTime = 0;

        StartCoroutine(FirePoseRoutine());
    }

    private IEnumerator FirePoseRoutine()
    {
        yield return new WaitForSeconds(_fireCoolDown);

        _animator.SetLayerWeight(1, 0f);
        _isShooting = false;
    }
}
