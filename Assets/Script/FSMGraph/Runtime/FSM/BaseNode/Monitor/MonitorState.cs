using UnityEngine;

public class MonitorState : BaseState, IFSMMonitor
{
    //얘는 상태일 필요가 없는데?(상태로 해도 상관없긴 하지)
    //어디서 틱체크하지?
    public CompareOperatorType compareOperator;
    public string leftKey;
    public float rightKey;
    public string rightString;
    public bool rightBool;
    public string nodeId { get; set; }
    public bool isUsed = false;
    public int priority;
    bool hit;
    bool canReActive;
    public MonitorState(IFSMAgent agent, MonitorStateSo data):base(agent)
    {
        compareOperator = data.compareOperator;
        leftKey = data.leftKey;
        rightKey = data.rightKey;
        rightString = data.rightString;
        rightBool = data.rightBool;
        priority = data.priority;
        canReActive = data.canReActive;

    }


    public bool Poll()
    {
        hit = agent.GraphRuntime.blackboard.Compare(
            leftKey, compareOperator, rightKey, rightString, rightBool);
        if (isUsed)
        {
            if (!hit && canReActive)
                isUsed = false;
            return false;
        }

        return hit;
    }

    public void Consume()
    {
        isUsed = true;
    }


}
