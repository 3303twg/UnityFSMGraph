using UnityEngine;

public abstract class BaseStateSoAsset : ScriptableObject
{
    public abstract BaseState CreateState(EnemyController controller, StateMachine stateMachine);
}

