using UnityEngine;

public static class PlayerAnim
{
    public static readonly int Idle = Animator.StringToHash("Idle");
    public static readonly int Run = Animator.StringToHash("Run");
    public static readonly int Jump = Animator.StringToHash("Jump");
    public static readonly int Landing = Animator.StringToHash("Landing");
    public static readonly int Stun = Animator.StringToHash("Stun");
}