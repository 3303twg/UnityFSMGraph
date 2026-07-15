using UnityEngine;

[CreateAssetMenu(menuName = "FSM/Boss/ComboMelee")]
public class ComboMeleeStateSo : BaseStateSo<ComboMeleeState>
{
    public int hitCount = 3;
    public float windup = 0.25f;
    public float betweenHits = 0.28f;
    public float recover = 0.45f;
    public float damage = 7f;
    public float hitRange = 3.6f;
}
