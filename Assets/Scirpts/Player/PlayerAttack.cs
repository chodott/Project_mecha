using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public enum FireState
{
    Idle,
    Charging,
    PostFire
}

public class PlayerAttack : NetworkBehaviour
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

    protected void Update()
    {
        if (_isCharging)
        {
            _curChargeTime += Time.deltaTime;
        }
    }

    private IEnumerator FirePoseRoutine()
    {
        OnFireStateChanged(FireState.PostFire);
        yield return new WaitForSeconds(_fireCoolDown);
        OnFireStateChanged(FireState.Idle);
    }

    private void PlayLocalFire(float direction)
    {
        //BaseBullet bulletPrefab = _curChargeTime >= _fullChargeTime ? _fullChargeBullet : _defaultBullet;
        //Vector3 directionVector = transform.right * direction;
        //float xOffset = MathF.Abs(_muzzleTransform.localPosition.x) * direction;
        //Vector3 launchPosition = transform.position + new Vector3(xOffset, _muzzleTransform.localPosition.y, 0);

        //BaseBullet newBullet = ObjectPoolManager.Instance.Get<BaseBullet>(bulletPrefab);
        //newBullet.Launch(launchPosition, directionVector);

        _curChargeTime = 0;
        _isCharging = false;

        if (_firePoseRoutine != null)
        {
            StopCoroutine(_firePoseRoutine);
        }
        _firePoseRoutine = StartCoroutine(FirePoseRoutine());
    }

    public void StartCharging()
    {
        _isCharging = true;
        OnFireStateChanged(FireState.Charging);
    }

    public void Fire(float direction)
    {
        if (IsOwner == false)
        {
            return;
        }

        FireServerRpc(direction, _curChargeTime);
        PlayLocalFire(direction);
    }

    public void BreakCharging()
    {
        _isCharging = false;
        _curChargeTime = 0;

        if (_firePoseRoutine != null)
        {
            StopCoroutine(_firePoseRoutine);
        }
    }

    //Network
    [ServerRpc]
    private void FireServerRpc(float direction, float chargeTime, ServerRpcParams rpcParams = default)
    {
        Vector3 directionVector = transform.right * direction;
        float xOffset = MathF.Abs(_muzzleTransform.localPosition.x) * direction;
        Vector3 launchPosition = transform.position + new Vector3(xOffset, _muzzleTransform.localPosition.y, 0);
        Quaternion rotation = direction > 0 ? Quaternion.identity : Quaternion.Euler(0, 180, 0);

        BaseBullet bulletPrefab = chargeTime >= _fullChargeTime ? _fullChargeBullet : _defaultBullet;
        GameObject go = Instantiate(bulletPrefab.gameObject, launchPosition, rotation);

        NetworkObject no = go.GetComponent<NetworkObject>();
        no.SpawnWithOwnership(rpcParams.Receive.SenderClientId);
        go.GetComponent<BaseBullet>().Launch(launchPosition, directionVector);
    }
}
