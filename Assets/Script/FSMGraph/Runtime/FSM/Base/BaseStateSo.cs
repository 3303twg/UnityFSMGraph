using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseStateSo<T> : BaseStateSoAsset where T : BaseState
{
    public override BaseState CreateState(IFSMAgent agent)
        => (T)Activator.CreateInstance(typeof(T), agent, this);
}
