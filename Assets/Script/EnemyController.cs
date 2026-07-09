using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public FSMGraphSo graphSo; //네이밍 꼬라지

    [SerializeField]
    private StateMachine stateMachine;
    FSMGraphRuntime graphRuntime;
    public IFSMNavigator Navigator => graphRuntime;

    private void Awake()
    {
        stateMachine = new StateMachine();
        graphRuntime = new FSMGraphRuntime(graphSo, this, stateMachine);
        graphRuntime.Init();



    }
    private void Start()
    {
    }

    private void Update()
    {
        stateMachine?.Update();   
    }
}
