using UnityEngine;

[CreateAssetMenu(menuName = "FSM/Boss/MeteorRain")]
public class MeteorRainStateSo : BaseStateSo<MeteorRainState>
{
    public int count = 8;
    public float windup = 0.5f;
    public float interval = 0.14f;
    public float recover = 0.45f;
    public float damage = 7f;
    public float fallSpeed = 12f;
    public float spawnHeight = 8f;
}
