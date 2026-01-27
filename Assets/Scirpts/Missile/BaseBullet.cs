using Unity.Netcode;
using UnityEngine;

public class BaseBullet : NetworkBehaviour, IPoolable
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] protected Animator _animator;
    [SerializeField] protected float _speed;
    [SerializeField] protected float _damage;

    private Vector2 _moveDirection;

    public int PoolKey { get; set; }

    protected virtual void Move()
    {
        transform.Translate(_moveDirection * _speed * Time.deltaTime, Space.World);
    }

    protected virtual void Update()
    {
        Move();
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsServer == false)
        {
            return;
        }

        if (collision.TryGetComponent<NetworkObject>(out var target))
        {
            if(target.OwnerClientId == OwnerClientId)
            {
                return;
            }
        }

        IDamageable damageable = collision.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(_damage);
        }
        Destroy(gameObject);

    }

    public void Launch(Vector2 position, Vector2 direction)
    {
        transform.position = position;
        _moveDirection = direction;
    }

    public void OnSpawn()
    {

    }

    public void OnDespawn()
    {
    }
}
