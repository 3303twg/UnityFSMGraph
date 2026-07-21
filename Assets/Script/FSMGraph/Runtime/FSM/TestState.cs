using System;
using UnityEngine;

[Serializable]
public class TestState : BaseState
{
    public string textTest = "0";

    public TestState(IFSMAgent agent, StateMachine stateMachine, TestStateSo data)
        : base(agent, stateMachine)
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
            agent.Navigator.GoToNextNode();
    }
}
