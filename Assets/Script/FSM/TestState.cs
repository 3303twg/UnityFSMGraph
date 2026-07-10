using System;
using UnityEngine;

[Serializable]
public class TestState : BaseState
{
    public string textTest = "0";

    public TestState(EnemyController enemyController, StateMachine stateMachine, TestStateSo data)
        : base(enemyController, stateMachine)
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
            enemyController.Navigator.GoToNextNode();
    }
}
