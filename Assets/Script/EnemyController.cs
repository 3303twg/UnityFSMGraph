using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemyController : MonoBehaviour
{
    [SerializeField]
    public BaseStat enemyStat;

    public FSMAgent fSMAgent;

    public void Init()
    {
        enemyStat.hp = enemyStat.maxHp;
        fSMAgent.GraphRuntime.blackboard.Set("Hp", enemyStat.hp);
        fSMAgent.baseStat = enemyStat;
    }


    private void Start()
    {

        Init();
    }

    [ContextMenu("TakeDamage")]
    public void TakeDamage()
    {
        enemyStat.hp -= 1f;
        fSMAgent.GraphRuntime.blackboard.Set("Hp", enemyStat.hp);
    }
}
