using UnityEngine;

[CreateAssetMenu(fileName = "MeleeAttack", menuName = "FSM/Boss/MeleeAttack")]
public class MeleeAttackStateSo : BaseStateSo<MeleeAttackState>
{
    public float windup = 0.35f;
    public float recover = 0.55f;
    public float damage = 10f;
    public float hitRange = 3.5f;
}
