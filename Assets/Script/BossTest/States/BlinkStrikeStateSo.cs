using UnityEngine;

[CreateAssetMenu(menuName = "FSM/Boss/BlinkStrike")]
public class BlinkStrikeStateSo : BaseStateSo<BlinkStrikeState>
{
    public float hideTime = 0.35f;
    public float strikeDelay = 0.15f;
    public float recover = 0.35f;
    public float damage = 18f;
    public float hitRange = 3.8f;
    public float appearOffset = 2.2f;
}
