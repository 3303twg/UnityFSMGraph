using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CompareState : BaseState
{
    public CompareOperatorType compareOperator;
    public string leftKey;
    public float rightKey;

    [NonSerialized] object left;
    [NonSerialized] object right;

    public CompareState(EnemyController enemyController, StateMachine stateMachine, CompareStateSo data)
        : base(enemyController, stateMachine)
    {
        compareOperator = data.compareOperator;
        leftKey = data.leftKey;
        rightKey = data.rightKey;
    }

    public override void Enter()
    {
        left = enemyController.GraphRuntime.blackboard.Get(leftKey);
        right = rightKey;

        if (Compare(left, right))
            enemyController.Navigator.GoToTrueNode();
        else
            enemyController.Navigator.GoToFalseNode();
    }

    public bool Compare(object left, object right)
    {
        switch (compareOperator)
        {
            case CompareOperatorType.Equal:
                return Equals(left, right);
            case CompareOperatorType.NotEqual:
                return !Equals(left, right);
            case CompareOperatorType.Greater:
                return Comparer<object>.Default.Compare(left, right) > 0;
            case CompareOperatorType.Less:
                return Comparer<object>.Default.Compare(left, right) < 0;
            case CompareOperatorType.GreaterOrEqual:
                return Comparer<object>.Default.Compare(left, right) >= 0;
            case CompareOperatorType.LessOrEqual:
                return Comparer<object>.Default.Compare(left, right) <= 0;
        }

        return false;
    }
}
