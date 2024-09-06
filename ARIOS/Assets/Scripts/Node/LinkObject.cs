using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LinkObject : MonoBehaviour
{
    [SerializeField] protected LineRenderer line;
    public Node startNode; //���η������� �׸��� ��ũ�� �������� �Ǵ³��
    public Node endNode; //���η������� �׸��� ��ũ�� ������ �Ǵ� ���
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
    /// �Ķ���ͷ� ���� ��尡 start���� end ���� �����Ͽ�
    /// start�� 0���� end�� 1�����Ͽ� Setpositions�� �ٽý����Ѵ�.
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