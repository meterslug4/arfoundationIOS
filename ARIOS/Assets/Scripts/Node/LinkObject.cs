using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LinkObject : MonoBehaviour
{
    [SerializeField] protected LineRenderer line;
    public Node startNode; //라인렌더러를 그릴시 링크의 시작점이 되는노드
    public Node endNode; //라인렌더러를 그릴시 링크의 끝점이 되는 노드
    public string name;
    [SerializeField] protected int linkId;
    protected Vector3[] positions = new Vector3[2];
    public void SetLink(Node n1, Node n2)
    {
        if (line == null)
        {
            line = gameObject.AddComponent<LineRenderer>();
        }
        startNode = n1;
        endNode = n2;
        name = string.Format("Link {0}--{1}", startNode, endNode);
        gameObject.name = name;
        line.startWidth = 0.1f;
        line.endWidth = 0.1f;

        line.startColor = Color.red;
        line.endColor = Color.red;

        positions[0] = startNode.transform.position;
        positions[1] = endNode.transform.position;

        line.positionCount = positions.Length;
        line.SetPositions(positions);

        line.material = new Material(Shader.Find("Sprites/Default"));
    }
    /// <summary>
    /// 파라미터로 받은 노드가 start인지 end 인지 구분하여
    /// start면 0변경 end면 1변경하여 Setpositions를 다시실행한다.
    /// </summary>
    /// <param name="node"></param>
    public void ChangeLinkPosition(Node node)
    {
        if (node.nodeId == startNode.nodeId)
        {
            positions[0] = node.transform.position;
        }
        if (node.nodeId == endNode.nodeId)
        {
            positions[1] = node.transform.position;
        }
        line.SetPositions(positions);
    }
    public (Node, Node) GetStartEndNode()
    {
        return (startNode, endNode);
    }
}