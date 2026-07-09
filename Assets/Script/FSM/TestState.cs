using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestState : BaseState
{
    string textTest = "0";
    public TestState(EnemyController enemyController, StateMachine stateMachine, TestStateSo data)
        :base(enemyController, stateMachine)
    {
        textTest = data.testText;
    }

    public override void Enter()
    {
        Debug.Log(textTest);
    }

    public override void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            //stateMachine.ChangeState(enemyController.stateDic[TestStateSo]);

            enemyController.Navigator.GoToNextNode();
        }
    }
    public override void Exit()
    {

    }
}
