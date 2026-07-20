using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFSMAgent
{
    public Transform transform { get; }
    //public Blackboard blackboard; FSMRuntime참조라 필요한가?
    public BaseStat baseStat { get; set; }
    public IFSMNavigator Navigator { get; }
    public FSMGraphRuntime GraphRuntime { get; set; }
}
