using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSMAgent : MonoBehaviour, IFSMAgent
{
    public FSMGraphSo graphSo;

    public Transform transform => this.transform;
    public BaseStat baseStat { get; set; }
    
    public FSMGraphRuntime GraphRuntime { get; set; }

    [SerializeReference]
    BaseState runtimeDebugState;
    StateMachine stateMachine;

    public IFSMNavigator Navigator => GraphRuntime;
    public BaseState RuntimeDebugState
    {
        get => runtimeDebugState;
        set => runtimeDebugState = value;
    }

    private void Awake()
    {
        stateMachine = new StateMachine();
        GraphRuntime = new FSMGraphRuntime(graphSo, this, stateMachine);
        GraphRuntime.Init();
    }

    void Update()
    {
        stateMachine?.Update();
        foreach (var monitor in GraphRuntime.monitorList)
        {
            monitor?.Update();
        }
    }
}
