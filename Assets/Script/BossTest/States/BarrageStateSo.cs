using UnityEngine;

public enum BarragePattern
{
    Random = 0,
    Fan = 1,
    Ring = 2,
    Spiral = 3,
    Cross = 4,
    Bloom = 5,
    Storm = 6
}

[CreateAssetMenu(menuName = "FSM/Boss/Barrage")]
public class BarrageStateSo : BaseStateSo<BarrageState>
{
    public BarragePattern pattern = BarragePattern.Random;
    public int shotCount = 5;
    public float windup = 0.35f;
    public float interval = 0.12f;
    public float recover = 0.4f;
    public float damage = 5f;
    public float projectileSpeed = 14f;
    public float spreadAngle = 28f;
    public int ringCount = 10;
    public int waveCount = 3;
    [Tooltip("발사 중 횡이동 속도 (0이면 제자리)")]
    public float strafeSpeed = 0f;
}
