using UnityEngine;

public class BaseBullet : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] protected Animator _animator;
    [SerializeField] protected float _speed;
    [SerializeField] protected float _damage;

    private Vector2 _moveDirection;

    protected virtual void Move()
    {
        transform.Translate(_moveDirection * _speed * Time.deltaTime);
    }

    protected virtual void Update()
    {
        Move();
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        Destroy(gameObject);
    }

    public void Launch(Vector2 position, Vector2 direction)
    {
        transform.position = position;
        _moveDirection = direction;
        _spriteRenderer.flipX = _moveDirection.x < 0;
    }
}
