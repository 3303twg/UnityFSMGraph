using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFSMMonitor
{
    public string nodeId { get; set; }
    public void Init();
}
