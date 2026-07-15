using UnityEngine;

[CreateAssetMenu(menuName = "FSM/Boss/Vacuum")]
public class VacuumStateSo : BaseStateSo<VacuumState>
{
    public float duration = 1.1f;
    public float pullStrength = 9f;
    public float pulseInterval = 0.14f;
}
