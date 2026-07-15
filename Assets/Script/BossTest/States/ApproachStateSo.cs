using UnityEngine;

[CreateAssetMenu(fileName = "ApproachState", menuName = "FSM/Boss/Approach")]
public class ApproachStateSo : BaseStateSo<ApproachState>
{
    [Tooltip("이 거리 안으로 들어오면 Compare로 넘어감")]
    public float engageDistance = 12f;
    public float moveSpeed = 4f;
}
