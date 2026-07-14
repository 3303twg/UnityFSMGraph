using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="CompareOperator", menuName ="FSM/Compare/CompareOperator")]
public class CompareStateSo : BaseStateSo<CompareState>
{
    public CompareOperatorType compareOperator;
    public BlackboardKey leftKey;
    //public BlackboardKey rightKey;
    public float rightKey;
    
}
