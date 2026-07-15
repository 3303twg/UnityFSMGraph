using UnityEngine;

[CreateAssetMenu(menuName = "FSM/Boss/HeavySlash")]
public class HeavySlashStateSo : BaseStateSo<HeavySlashState>
{
    public float windup = 0.7f;
    public float recover = 0.55f;
    public float damage = 22f;
    public float hitRange = 4.2f;
}
