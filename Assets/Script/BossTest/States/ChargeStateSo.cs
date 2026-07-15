using UnityEngine;

public enum ChargeStyle
{
    Random = 0,
    Straight = 1,
    Zigzag = 2,
    Curve = 3,
    Double = 4,
    Feint = 5
}

[CreateAssetMenu(fileName = "ChargeAttack", menuName = "FSM/Boss/ChargeAttack")]
public class ChargeStateSo : BaseStateSo<ChargeState>
{
    public ChargeStyle style = ChargeStyle.Random;
    public float chargeSpeed = 14f;
    public float duration = 0.7f;
    public float damage = 15f;
    public float hitRadius = 2f;
    public float zigzagAmp = 4.5f;
    public float zigzagFreq = 14f;
    public float curveTurnRate = 220f;
}
