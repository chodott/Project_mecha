using System;
using System.Collections;
using UnityEngine;

public enum FireState
{
    Idle,
    Charging,
    PostFire
}

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private BaseBullet _defaultBullet;
    [SerializeField] private BaseBullet _fullChargeBullet;
    [SerializeField] private float _fullChargeTime;
    [SerializeField] private Transform _muzzleTransform;
    [SerializeField] private float _fireCoolDown = 0.2f;


    private Coroutine _firePoseRoutine;
    private float _curChargeTime;
    private bool _isCharging = false;

    public event Action<FireState> OnFireStateChanged;

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
        OnFireStateChanged(FireState.Charging);
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
        OnFireStateChanged(FireState.PostFire);
        yield return new WaitForSeconds(_fireCoolDown);
        OnFireStateChanged(FireState.Idle);
    }
}
