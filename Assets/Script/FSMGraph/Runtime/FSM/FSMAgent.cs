using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSMAgent : MonoBehaviour, IFSMAgent
{
    public FSMGraphSo graphSo;

    public Transform Transform => this.transform;

    FSMGraphRuntime graphRuntime;
    public FSMGraphRuntime GraphRuntime => graphRuntime;

    public Blackboard Blackboard => GraphRuntime.Blackboard;

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
        graphRuntime = new FSMGraphRuntime(graphSo, this, stateMachine);
        GraphRuntime.Init();
    }

    void Update()
    {
        GraphRuntime?.Tick();
    }

    private void OnDestroy()
    {
        GraphRuntime.Dispose();
    }
}
