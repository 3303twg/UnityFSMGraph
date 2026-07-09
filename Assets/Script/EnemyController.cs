using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public FSMGraphSo graphSo; //네이밍 꼬라지
    StateMachine stateMachine;

    private void Awake()
    {
        stateMachine = new StateMachine();

        //stateFactory.CreateState()
    }

}
