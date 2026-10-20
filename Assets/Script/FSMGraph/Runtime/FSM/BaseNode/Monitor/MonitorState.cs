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

    bool hit;
    public MonitorState(IFSMAgent agent, StateMachine stateMachine, MonitorStateSo data):base(agent, stateMachine)
    {
        compareOperator = data.compareOperator;
        leftKey = data.leftKey;
        rightKey = data.rightKey;
        rightString = data.rightString;
        rightBool = data.rightBool;
    }


    public void Init()
    {
        //필요한가?
    }
    public override void Update()
    {
        hit = agent.GraphRuntime.blackboard.Compare(
            leftKey, compareOperator, rightKey, rightString, rightBool);
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

}
