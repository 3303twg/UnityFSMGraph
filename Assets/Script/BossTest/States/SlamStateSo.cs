using UnityEngine;

[CreateAssetMenu(menuName = "FSM/Boss/Slam")]
public class SlamStateSo : BaseStateSo<SlamState>
{
    public float windup = 0.85f;
    public float recover = 0.6f;
    public float damage = 20f;
    public float radius = 4.5f;
}
