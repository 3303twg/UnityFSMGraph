using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;

public class CompareState : BaseState
{
    public CompareOperatorType compareOperator;
    public BlackboardKey leftKey;
    public float rightKey;
    public object left;
    public object right;

    public CompareState(EnemyController enemyController, StateMachine stateMachine, CompareStateSo data)
        :base(enemyController, stateMachine)
    {
        compareOperator = data.compareOperator;
        leftKey = data.leftKey;
        rightKey = data.rightKey;
    }

    

    //흠..... 뭘 어캐받지 무슨타입이던간 받아야하는데 제네릭으로 받는다 치고
    public override void Enter()
    {
        Debug.Log("도착");
        left = enemyController.GetBlackboardValue(leftKey);
        //right = enemyController.GetBlackboardValue((BlackboardKey)rightKey);
        right = (float)rightKey;
        Debug.Log(left);
        Debug.Log(right);
        if(Compare(left, right))
        {
            Debug.Log("참트루");
            enemyController.Navigator.GoToTrueNode();
        }
        else
        {
            Debug.Log("뻘스");
            enemyController.Navigator.GoToFalseNode();
        }
    }

    public bool Compare(object left, object right)
    {
        switch (compareOperator)
        {
            case CompareOperatorType.Equal:
                return Equals(left, right);
                break;
            case CompareOperatorType.NotEqual:
                return !Equals(left, right);
            case CompareOperatorType.Greater:
                return Comparer<object>.Default.Compare(left, right) > 0;
            case CompareOperatorType.Less:
                return Comparer<object>.Default.Compare(left, right) < 0;
        }

        return false;
    }
}
