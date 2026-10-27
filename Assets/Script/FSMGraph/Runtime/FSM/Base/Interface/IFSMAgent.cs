using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFSMAgent
{
    public Transform Transform { get; }
    public IFSMNavigator Navigator { get; }
    public FSMGraphRuntime GraphRuntime { get; set; }
}
