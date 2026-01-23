using UnityEngine;

abstract public class StateEvent
{

}

sealed class ParriedEvent : StateEvent
{
    public Vector3 ContactPoint { get; }
    public float Damage { get; }
    public float MoveLaneSpeed { get; }
    public ParriedEvent(Vector3 contactPoint, float damage, float moveLaneSpeed)
    {
        ContactPoint = contactPoint;
        Damage = damage;
        MoveLaneSpeed = moveLaneSpeed;
    }
}

sealed class InputParryEvent : StateEvent
{

}

sealed class InputMoveEvent : StateEvent
{
    public int IsRight { get; }
    public InputMoveEvent(int isRight) => IsRight = isRight;
}

sealed class OnDamagedEvent : StateEvent
{
}


sealed class ProjectileHitEvent : StateEvent
{
    public Collision Collision { get; }
    public ProjectileHitEvent(Collision collision) => Collision = collision;
}

sealed class CarCollisionEvent : StateEvent
{
    public Collision Collision { get; }
    public CarCollisionEvent(Collision collision) => Collision = collision;
}

sealed class OnTriggerEnterEvent : StateEvent
{
    public Collider Collider { get; }
    public OnTriggerEnterEvent(Collider collider) => Collider = collider;
}