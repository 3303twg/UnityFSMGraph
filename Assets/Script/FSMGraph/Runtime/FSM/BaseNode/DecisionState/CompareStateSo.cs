using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="CompareOperator", menuName ="FSM/Compare/CompareOperator")]
public class CompareStateSo : BaseStateSo<CompareState>
{
    public CompareOperatorType compareOperator;
    public string leftKey;
    public float rightKey;
    public string rightString;
    public bool rightBool;
    
}
