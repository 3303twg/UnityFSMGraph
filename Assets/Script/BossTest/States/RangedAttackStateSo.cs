using UnityEngine;

[CreateAssetMenu(fileName = "RangedAttack", menuName = "FSM/Boss/RangedAttack")]
public class RangedAttackStateSo : BaseStateSo<RangedAttackState>
{
    public float windup = 0.4f;
    public float recover = 0.5f;
    public float damage = 8f;
    public float projectileSpeed = 16f;
    public GameObject projectilePrefab;
}
