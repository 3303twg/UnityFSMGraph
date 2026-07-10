using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NodeType
{
    Entry,
    Transition,//하씨 이름 머하지..
    Action,
    Monitor,
    Reference
}

public enum CompareOperatorType
{
    Equal,
    NotEqual,
    Greater,
    Less,
    GreaterOrEqual,
    LessOrEqual
}

public enum BlackboardKey
{
    CurHp,
    DetectionDistance,
}
