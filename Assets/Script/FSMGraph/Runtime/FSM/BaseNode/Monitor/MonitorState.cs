using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonitorState : BaseState, IFSMMonitor
{
    //얘는 상태일 필요가 없는데?(상태로 해도 상관없긴 하지)
    //어디서 틱체크하지?
    public CompareOperatorType compareOperator;
    public string leftKey;
    public float rightKey;
    [NonSerialized] object left;
    [NonSerialized] object right;
    public string nodeId { get; set; }
    public bool isUsed = false;

    bool hit;
    public MonitorState(IFSMAgent agent, StateMachine stateMachine, MonitorStateSo data):base(agent, stateMachine)
    {
        compareOperator = data.compareOperator;
        leftKey = data.leftKey;
        rightKey = data.rightKey;
    }


    public void Init()
    {
        //필요한가?
    }
    public override void Update()
    {
        hit = Compare(agent.GraphRuntime.blackboard.Get(leftKey), (object)rightKey);
        if (isUsed)
        {
            if (!hit)
            {
                isUsed = false;
            }
            return;
        }

        if (hit)
        {
            isUsed = true;
            agent.Navigator.GoToPortFrom(nodeId, PortType.Output);

        }

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
