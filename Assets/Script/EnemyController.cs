using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    StateMachine stateMachine;

    private void Awake()
    {
        stateMachine = new StateMachine();

        //stateFactory.CreateState()
    }

}
