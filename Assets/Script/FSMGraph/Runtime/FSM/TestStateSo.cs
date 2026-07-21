using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TestStateSo", menuName = "FSM/State/TestStateSo") ]
public class TestStateSo : BaseStateSo<TestState>
{
    [Header("테스트텍스트")]
    public string testText = "ㅋㅋ";
    
}
