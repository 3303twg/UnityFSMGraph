using System;
using UnityEngine;

[Serializable]
public class CompareState : BaseState
{
    public CompareOperatorType compareOperator;
    public string leftKey;
    public float rightKey;
    public string rightString;
    public bool rightBool;

    public CompareState(IFSMAgent agent, CompareStateSo data)
        : base(agent)
    {
        compareOperator = data.compareOperator;
        leftKey = data.leftKey;
        rightKey = data.rightKey;
        rightString = data.rightString;
        rightBool = data.rightBool;
    }

    public bool Evaluate()
    {
        return agent.Blackboard.Compare(
            leftKey, compareOperator, rightKey, rightString, rightBool);
    }

    public override void Enter()
    {
        if (Evaluate())
            agent.Navigator.GoToTrueNode();
        else
            agent.Navigator.GoToFalseNode();
    }
}
