using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemyController : MonoBehaviour
{
    [SerializeField]
    public BaseStat enemyStat;

    public FSMAgent agent;

    private void Awake()
    {
        agent = GetComponent<FSMAgent>();
        enemyStat.hp = enemyStat.maxHp;
        
    }
    private void Start()
    {
        agent.Blackboard.Add(Blackboard.CreateVariable(typeof(float), "float"));
        agent.Blackboard.Add(Blackboard.CreateVariable(typeof(float), "distance"));
        agent.Blackboard.Add(Blackboard.CreateVariable(typeof(int), "int"));
        //agent.Blackboard.Add<int>(new BlackboardVariable<int>() { key = "distance" });
        //agent.Blackboard.Add<string>(new BlackboardVariable<string>() { key = "string" });
        //agent.Blackboard.Add<float>(new BlackboardVariable<float>() { key = "float" });
    }

    public float distance;
    public void Update()
    {
        distance = Vector2.Distance(FastPlayer.instance.transform.position, transform.position);
        agent.Blackboard.Set("distance", distance);
    }

    [ContextMenu("TakeDamage")]
    public void TakeDamage()
    {
        enemyStat.hp -= 1f;
        agent.Blackboard.Set("Hp", enemyStat.hp);
    }
}
