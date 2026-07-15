using UnityEngine;

[CreateAssetMenu(menuName = "FSM/Boss/SpinRush")]
public class SpinRushStateSo : BaseStateSo<SpinRushState>
{
    public float duration = 1.4f;
    public float moveSpeed = 5.5f;
    public float spinSpeed = 720f;
    public float tickInterval = 0.2f;
    public float damage = 4f;
    public float hitRadius = 2.8f;
}
