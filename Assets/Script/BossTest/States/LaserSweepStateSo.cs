using UnityEngine;

[CreateAssetMenu(menuName = "FSM/Boss/LaserSweep")]
public class LaserSweepStateSo : BaseStateSo<LaserSweepState>
{
    [Tooltip("흰 예고선이 천천히 지나가는 시간")]
    public float previewDuration = 0.55f;
    [Tooltip("예고 후 본타 전 대기")]
    public float holdDuration = 0.32f;
    [Tooltip("실제 레이저가 빠르게 긁는 시간")]
    public float strikeDuration = 0.22f;
    public float recover = 0.28f;
    public float damage = 22f;
    public float beamLength = 13f;
    public float beamWidth = 0.85f;
    public float sweepAngle = 120f;
    [Tooltip("본타를 몇 번 긁을지 (1=1회, 2=왕복)")]
    public int strikeCount = 1;
}
