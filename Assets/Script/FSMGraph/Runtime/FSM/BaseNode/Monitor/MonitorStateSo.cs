using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Monitor", menuName = "FSM/Monitor")]
public class MonitorStateSo : BaseStateSo<MonitorState>
{
    public CompareOperatorType compareOperator;
    public string leftKey;
    public float rightKey;
    public string rightString;
    public bool rightBool;
    public int priority;
    public bool canReActive = true;
}
