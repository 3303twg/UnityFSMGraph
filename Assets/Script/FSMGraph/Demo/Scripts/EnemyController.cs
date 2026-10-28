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
        agent.GraphRuntime.blackboard.Add(Blackboard.CreateVariable(typeof(float), "float"));
        agent.GraphRuntime.blackboard.Add(Blackboard.CreateVariable(typeof(int), "int"));
        //agent.GraphRuntime.blackboard.Add<int>(new BlackboardVariable<int>() { key = "distance" });
        //agent.GraphRuntime.blackboard.Add<string>(new BlackboardVariable<string>() { key = "string" });
        //agent.GraphRuntime.blackboard.Add<float>(new BlackboardVariable<float>() { key = "float" });
    }


    [ContextMenu("TakeDamage")]
    public void TakeDamage()
    {
        enemyStat.hp -= 1f;
        agent.GraphRuntime.blackboard.Set("Hp", enemyStat.hp);
    }
}
