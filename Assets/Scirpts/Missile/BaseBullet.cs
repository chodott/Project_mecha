using Unity.Netcode;
using UnityEngine;

public class BaseBullet : NetworkPoolable
{
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;
    private Rigidbody2D _rigidBody;
    [SerializeField] private float _speed;
    [SerializeField] private float _power;
    public float Power {get { return _power; } }

    private Vector2 _moveDirection;

    protected void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _rigidBody = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();

    }
    protected void Start()
    {
        _rigidBody.gravityScale = 0;
    }
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

        if(collision.TryGetComponent<BaseBullet>(out var otherBullet))
        {
            if(_power <= otherBullet.Power)
            {
                Destroy(gameObject);
            }
            return;
        }

        if (collision.TryGetComponent<NetworkObject>(out var target))
        {
            if (target.OwnerClientId == OwnerClientId)
            {
                return;
            }
        }

        IDamageable damageable = collision.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(Power);
        }
        Destroy(gameObject);

    }

    public void Launch(Vector2 position, Vector2 direction)
    {
        transform.position = position;
        _moveDirection = direction;
    }

    public override void OnSpawn()
    {
        _rigidBody.angularVelocity = 0;
        _rigidBody.linearVelocity = Vector2.zero;
    }

    public override void OnDespawn()
    {
    }
}
