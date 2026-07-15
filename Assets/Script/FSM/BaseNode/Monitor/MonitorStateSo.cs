using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Monitor", menuName = "FSM/Monitor")]
public class MonitorStateSo : BaseStateSo<MonitorState>
{
    public CompareOperatorType compareOperator;
    public BlackboardKey leftKey;
    public float rightKey;
}
