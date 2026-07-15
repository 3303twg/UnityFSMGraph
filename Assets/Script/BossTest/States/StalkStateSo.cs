using UnityEngine;

[CreateAssetMenu(menuName = "FSM/Boss/Stalk")]
public class StalkStateSo : BaseStateSo<StalkState>
{
    public float duration = 2.2f;
    public float orbitSpeed = 3.2f;
    public float approachSpeed = 1.8f;
    public float preferDistance = 6.5f;
    public float forceDecideDistance = 3.2f;
}
