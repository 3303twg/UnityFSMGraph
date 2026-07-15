using UnityEngine;

[CreateAssetMenu(menuName = "FSM/Boss/HomingVolley")]
public class HomingVolleyStateSo : BaseStateSo<HomingVolleyState>
{
    public int count = 4;
    public float windup = 0.45f;
    public float interval = 0.12f;
    public float recover = 0.35f;
    public float damage = 8f;
    public float speed = 5.5f;
    public float turnRate = 160f;
}
