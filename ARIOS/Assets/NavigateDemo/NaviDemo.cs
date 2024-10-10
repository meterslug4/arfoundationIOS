using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class NaviDemo : MonoBehaviour
{
    protected Camera mainCam;
    public GameObject agent; //3D �ʿ��� �������� ���� ������Ʈ
    protected float yAxis;
    protected GameObject nodeRoot;
    public Button pathFindBtn;

    protected bool reFind = false;
    protected float defaultValue = 999999.0f;
    public delegate void DelSetPath(List<Node> prentList, bool setNewPath);
    public DelSetPath delSetPath;
    public Node startNode; //�������̵� ���
    public Node endNode; //�������̵� ��� 
    public List<Node> nodeList = new List<Node>();
    public List<Node> parentList = new List<Node>();
    protected Dictionary<int, Node> dic_Open = new Dictionary<int, Node>();
    protected Dictionary<int, Node> dic_Close = new Dictionary<int, Node>();
    protected Dictionary<int, Node> dic_parent = new Dictionary<int, Node>();//���������� ������������ ��尡 ������� 
    public Dictionary<string, Node> dicDest = new Dictionary<string, Node>();
    private void Awake()
    {
        mainCam = Camera.main;
        nodeRoot = GameObject.Find("NodeRoot");
    }
    void Start()
    {
        pathFindBtn.onClick.AddListener(PathFind);
    }
    private void Update()
    {
        MoveAgent();
    }
    //ó�� ���۽ÿ� �ٶ󺸴� ����� ��ġ�� ���� 
    protected void SetStartPosition()
    {

    }
    protected void MoveAgent()
    {
        Vector3 camPos = mainCam.transform.position;
        Vector3 agentPos = new Vector3(camPos.x, 0, camPos.z);
        agent.transform.position = agentPos;
        //Quaternion camRot = mainCam.transform.rotation;
        //yAxis = camRot.eulerAngles.y;
        //Vector3 userRot = agent.transform.eulerAngles;
        //userRot.y = yAxis;
        //agent.transform.eulerAngles = userRot;
    }

    protected void PathFind()
    {
        reFind = true;
        if (reFind)
        {
            ClearAll();
        }
        if (startNode != null)
        {
            startNode.SetH(startNode.transform.position, endNode.transform.position);
            InsertQueue(startNode);
        }
        if (!dic_parent.ContainsKey(startNode.nodeId))
        {
            Debug.Log("���� ��� �߰�");
            dic_parent.Add(startNode.nodeId, startNode);
        }

        while (nodeList.Count > 0)
        {
            Node node = DeQueue();
            if (!dic_parent.ContainsKey(node.nodeId))
            {
                Debug.Log(node.nodeId);
                dic_parent.Add(node.nodeId, node);
            }
            for (int i = 0; i < node.relatedDataList.Count; i++)
            {
                if (!dic_Open.ContainsKey(node.relatedDataList[i].adjacentNodes.nodeId))
                {
                    dic_Open.Add(node.relatedDataList[i].adjacentNodes.nodeId,
                        node.relatedDataList[i].adjacentNodes);
                }
            }
            if (dic_Close.ContainsKey(node.nodeId))
            {
                continue;
            }
            dic_Close.Add(node.nodeId, node);
            //�������� ���������� ����
            if (node.pos == endNode.pos)
            {
                Debug.Log("������ ����");
                break;
            }
            for (int i = 0; i < node.relatedDataList.Count; i++)
            {
                int nextNodeId = node.relatedDataList[i].adjacentNodes.nodeId;
                float G = node.G + node.relatedDataList[i].g;
                Vector3 v = endNode.pos - node.relatedDataList[i].adjacentNodes.pos;
                float sqr = v.sqrMagnitude;
                float H = sqr;
                dic_Open[nextNodeId].H = H;//��������� �޸���ƽ �� ����
                if (dic_Open[nextNodeId].F < G + H)
                {
                    continue;
                }
                dic_Open[nextNodeId].F = G + H;
                dic_Open[nextNodeId].G = G;
                InsertQueue(dic_Open[nextNodeId]);
            }
        }
        //���۳���� ��������� �׷��ֱ�
        foreach (var v in dic_parent.Values)
        {
            Debug.Log(v);
            MeshRenderer mesh = v.gameObject.GetComponent<MeshRenderer>();
            if (v.nodeId == startNode.nodeId)
            {
                mesh.material.color = Color.red;
            }
            if (v.nodeId == endNode.nodeId)
            {
                mesh.material.color = Color.blue;
            }
            if (v.nodeId != startNode.nodeId && v.nodeId != endNode.nodeId)
            {
                mesh.material.color = Color.green;
            }
            parentList.Add(v);
        }
        delSetPath.Invoke(parentList, true);
    }

    protected void InsertQueue(Node node)
    {
        nodeList.Add(node);//�߰�
        int now = nodeList.Count - 1;
        while (now > 0)//now == 0�ϋ��� ����Ʈ�� ������ 1���� ������ �����ʿ䰡 ���� now�� 1�϶����� ���� ��
        {
            int next = (now - 1) / 2; //�ڽ��� �θ����� index���� ������ ���� index���� �ȴ�
            if (nodeList[now].F > nodeList[next].F)
            {
                break;
            }
            Node temp = nodeList[now];
            nodeList[now] = nodeList[next];
            nodeList[next] = temp;
            now = next;
        }
    }
    //F���� ���ؼ� �� ���� �������� �켱������ ����
    protected Node DeQueue()
    {
        Node returnValue = nodeList[0];//���� ������带 ��ȯ
        int lastIndex = nodeList.Count - 1;//�ֻ��� ��Ʈ�� �̵���ų ������ �ε���
        nodeList[0] = nodeList[lastIndex];//������ ���� root�� �̵�
        nodeList.RemoveAt(lastIndex);//������ ���� ����
        lastIndex -= 1;//������ ���� ��������� lastIndex���� �ϳ� ���δ�.
        int nowIndex = 0; //������ ��尡 0�� index�� �̵������Ƿ� now�� 0���� ����
        while (true)
        {
            //nowIndex�� ���� childnode�� ���� ���Ѵ�.
            int leftChildIndex = 2 * nowIndex + 1;//���� ���
            int rightChildIndex = 2 * nowIndex + 2;//������ ���

            //nextIndex�� �񱳴���̴�.
            int nextIndex = nowIndex; //nextIndex�� ��ȭ�ذ��鼭 ��� ���س�����

            if (leftChildIndex <= lastIndex && nodeList[nextIndex].F > nodeList[leftChildIndex].F)
            {
                nextIndex = leftChildIndex; //leftIndex�� ������ ������ nextIndex�� �ϴ� leftIndex�� ����
            }
            if (rightChildIndex <= lastIndex && nodeList[nextIndex].F > nodeList[rightChildIndex].F)
            {
                nextIndex = rightChildIndex;
            }
            if (nextIndex == nowIndex)
            {
                break;
            }
            Node temp = nodeList[nowIndex];
            nodeList[nowIndex] = nodeList[nextIndex];
            nodeList[nextIndex] = temp;
            nowIndex = nextIndex;
        }
        return returnValue;
    }
    protected void ClearAll()
    {
        nodeList.Clear();
        parentList.Clear();
        dic_Close.Clear();
        dic_parent.Clear();
        foreach (var v in dic_Open.Values)
        {
            //���¸���Ʈ�� �߰��Ǿ� �ִ� F���� �ʱⰪ���� ������
            v.ResetF(defaultValue);
        }
    }
}
