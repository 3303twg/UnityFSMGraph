using UnityEngine;

public interface IFSMNavigator
{
    string CurrentNodeId { get; }
    void GoToNode(string nodeId);
    void GoToNextNode();
    void GoToTrueNode();
    void GoToFalseNode();
    void GoToPortFrom(string nodeId, PortType portType);
}
