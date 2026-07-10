using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFSMNavigator
{
    string CurrentNodeId { get; }
    //void GotoNode(string nodeId);
    void GoToNextNode();
    void GoToTrueNode();
    void GoToFalseNode();
}
