using UnityEngine;

[CreateAssetMenu(menuName = "FSM/Boss/Backstep")]
public class BackstepStateSo : BaseStateSo<BackstepState>
{
    public float duration = 0.35f;
    public float speed = 10f;
}
