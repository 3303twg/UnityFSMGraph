using UnityEngine;

[CreateAssetMenu(menuName = "FSM/Boss/Enrage")]
public class EnrageStateSo : BaseStateSo<EnrageState>
{
    public float duration = 1.2f;
    public float speedMul = 1.45f;
    public float damageMul = 1.35f;
}
