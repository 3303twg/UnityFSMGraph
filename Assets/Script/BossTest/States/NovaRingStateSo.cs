using UnityEngine;

[CreateAssetMenu(menuName = "FSM/Boss/NovaRing")]
public class NovaRingStateSo : BaseStateSo<NovaRingState>
{
    public float windup = 0.55f;
    public float expandTime = 0.85f;
    public float maxRadius = 9f;
    public float damage = 16f;
    public float bandWidth = 0.85f;
}
