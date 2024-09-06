using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Demo : MonoBehaviour
{
    protected Camera mainCam; //xrCamera
    public GameObject user;//�̴ϸʿ� ǥ�õ� user objects
    protected float yAxis;
    public UIManager uiManager;

    public delegate void DelSetPath(List<Node> prentList, bool setNewPath);
    public DelSetPath delSetPath;
    public List<Node>nodeList = new List<Node>();
    public List<Node> parentList = new List<Node>();
    public Node startNode; //�������̵� ���
    public Node endNode; //�������̵� ��� 
    public bool reFind = false;
    protected float defaultValue = 999999.0f;
    protected Dictionary<int, Node> dic_Open = new Dictionary<int, Node>();
    protected Dictionary<int, Node> dic_Close = new Dictionary<int, Node>();
    protected Dictionary<int, Node> dic_parent = new Dictionary<int, Node>();//���������� ������������ ��尡 ������� 


    public Dictionary<string,Node> dicDest= new Dictionary<string, Node>();
    protected List<string> dropdownList = new List<string>();

    private void Awake()
    {
        mainCam = Camera.main;
        var root = GameObject.Find("NodeRoot");
        var nodes = root.GetComponentsInChildren<Node>();
        dropdownList.Add(CONST.NOT);
        for (int i=0; i<nodes.Length; i++)
        {
            if(!string.IsNullOrEmpty(nodes[i].nodeName))
            {
                dicDest.Add(nodes[i].nodeName, nodes[i]);
                dropdownList.Add(nodes[i].nodeName);
            }
        }
    }

    void Start()
    {
        uiManager.SetDropdownMenu(dropdownList);
        uiManager.findPathBtn.onClick.AddListener(PathFind);
    }
    void Update()
    {
        MoveUserObj();
    }
    protected void MoveUserObj()
    {
        Vector3 camPos = mainCam.transform.position;
        Vector3 userPos = new Vector3(camPos.x, 0, camPos.z);
        user.transform.position = userPos;
        //y������ �¿� ȸ���� ǥ��
        Quaternion camRot = mainCam.transform.rotation;
        yAxis = camRot.eulerAngles.y;
        Vector3 userRot = user.transform.eulerAngles;
        userRot.y = yAxis;
        user.transform.eulerAngles = userRot;
    }

    protected void PathFind()
    {
        reFind = true;
        if (reFind)
        {
            ClearAll();
        }
        //�߰� ����
        //startNode�� null�϶� �ݰ�ȿ��� ���� ����� ��带 ã���Ŀ� ���� ����� ��带 ��������
        //��ã�⸦ �����ϰ� ��尡 ���� ���� ��ġ���� startNode�� ������ direction�� �����Ѵ�
        //��尡 ���� ���� ��Ȳ���� ��� ���̵� ���ư����� ó�����ֱ�����
        if (startNode != null)
        {
            //startNode�� H��
            //startNode���� ������ ������ �Ÿ�
            startNode.SetH(startNode.transform.position, endNode.transform.position);

            //�켱���� ť�� ���ؼ� ������ ���� �̴´�.
            //�켱���� ť�� ��������� �־ Ʈ���� �����Ѵ�
            Debug.Log("Insert PQ");
            InsertQueue(startNode);
        }

        //���� �����Ѱ͸� Ž���ؼ� �����ϴ� ��ųʸ� ������� �����ϸ� ã�����̵ȴ�
        //ó������ startNode���� �����ϴ� startNode�� ������ ��������  �־��ְ� ����
        if (!dic_parent.ContainsKey(startNode.nodeId))
        {
            Debug.Log("���� ��� �߰�");
            dic_parent.Add(startNode.nodeId, startNode);
        }

        while (nodeList.Count > 0)
        {
            //���� ���� �ĺ��� ã�´�
            Node node = DeQueue();
            if (!dic_parent.ContainsKey(node.nodeId))
            {
                Debug.Log(node.nodeId);
                dic_parent.Add(node.nodeId, node);
            }
            //������ ����� ���� ������ dic_Open�� �ʱ�ȭ �ؼ� �־���´�
            //���� ��忡�� ������ �湮�� �ĺ����� ����
            for (int i = 0; i < node.relatedDataList.Count; i++)
            {
                if (!dic_Open.ContainsKey(node.relatedDataList[i].adjacentNodes.nodeId))
                {
                    dic_Open.Add(node.relatedDataList[i].adjacentNodes.nodeId,
                        node.relatedDataList[i].adjacentNodes);
                }
            }
            Debug.Log(string.Format("{0}�� ���� ����� ���� ���̴� �����´�---", node));
            //������ ��ǥ�� ���� ��η� ã�Ƽ� ������ ��츦 ã�� �̹� �湮�Ȱ��(close �� ���) ��ŵ
            //� ������� �˾Ƽ� index�� �����Ҽ� �־����
            if (dic_Close.ContainsKey(node.nodeId))
            {
                //�̹� close dictionary�� �߰��� ���̶��? ��ŵ
                //Debug.Log(string.Format("{0}�� �̹� �湮�Ѱ��̴� �Ѿ��",node));
                continue;
            }
            //�湮������ ����? ->close dic�� �߰��ؼ� �湮�Ѱ� üũ �������� �湮���ϵ���
            dic_Close.Add(node.nodeId, node);
            //�������� ���������� ����
            if (node.pos == endNode.pos)
            {
                Debug.Log("������ ����");
                break;
            }
            //�̵��Ҽ� �ִ� ������� Ȯ���ؼ� ����(open)
            //���� �̿����� ���� �̵������Ѱ�(������ ������ ����) ,�湮�ߴ��� ���ߴ����� ��
            //node�� ��� ���

            //���������� ����� �̹� ���Ǿ� ����

            //������ �̵������� ���鿡 ���Ͽ� ���� ó��
            for (int i = 0; i < node.relatedDataList.Count; i++)
            {
                //string s1= string.Format("{0}�� ���� ���� �̵��Ҽ��ִ� �ĺ����̴�", node.linkNodeList[i]);
                //Debug.Log(s1);
                //�̵� ������ ���� ������ ���� ��� ���
                //���ο� �濡 ���� �� ���
                //������ ����� id �������� �� ����� id���ȴ�
                int nextNodeId = node.relatedDataList[i].adjacentNodes.nodeId;
                //���� ��忡�� ���� ���� �̵��ϴµ� ��� ���(�Ÿ�)
                float G = node.G + node.relatedDataList[i].g;
                //����� �޸���ƽ ��
                //���� ���� ��忡�� ������������ �޸���ƽ ��
                Vector3 v = endNode.pos - node.relatedDataList[i].adjacentNodes.pos;
                float sqr = v.sqrMagnitude;
                float H = sqr;
                dic_Open[nextNodeId].H = H;//��������� �޸���ƽ �� ����

                //������ �̵������� ��������� F(G+H)���� �ٸ���ο��� �߰������� ��ŵ
                //dic_Open�� �⺻���� max value �̱⋚���� ���� ã������ ���ٸ� ������ ũ�� 
                //�۴ٸ� �̹� �湮�Ͽ� ���� ���� ���ִ� ����̰� �������� ����� �������̱⿡
                //�� ���� ���� �����°;ƴϸ� ��ŵ�Ѵ�(������ �켱����)
                if (dic_Open[nextNodeId].F < G + H)
                {
                    //string s2 = string.Format("{0}������ �̵� ����� �������� ���� �ʴ� �ǹ̾��� �Ѿ��", node.linkNodeList[i]);
                    //Debug.Log(s2);
                    continue;
                }
                //���� ��쿡 �Ȱɸ��� ���� ���� ����̹Ƿ� open�� ���� �����Ѵ�
                dic_Open[nextNodeId].F = G + H;
                dic_Open[nextNodeId].G = G;
                //string s3 = string.Format("{0}�� F���� ���� �����Ѱ� {1}", dic_Open[nextNodeId],
                //    dic_Open[nextNodeId].F);
                //Debug.Log(s3);
                //�켱���� ť���� �־���
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
        //Ž���� ��ε��� parentList�� ���ο� ��ΰ� Ž���Ǿ� ���� �־�����  = ture
        delSetPath.Invoke(parentList, true);
    }

    protected void InsertQueue(Node node)
    {
        nodeList.Add(node);//�߰�
        //�������� �߰��Ȱ����� ���� ������.
        //count  1,2,3...
        //now    0,1,2...
        int now = nodeList.Count - 1;
        while (now > 0)//now == 0�ϋ��� ����Ʈ�� ������ 1���� ������ �����ʿ䰡 ���� now�� 1�϶����� ���� ��
        {
            //now�� �θ� ���� �񱳸��ؾ���
            //now ����� �θ� ��带 �˾ƾ���
            //���� �� ����� now�� �θ� ��尡��
            int next = (now - 1) / 2; //�ڽ��� �θ����� index���� ������ ���� index���� �ȴ�
            //now index�� ���� next index�� ������ ������
            if (nodeList[now].F > nodeList[next].F)
            {
                //��ȯ���ʿ� ���� ���� ����
                break;
            }
            //�� ��ȯ
            Node temp = nodeList[now];
            nodeList[now] = nodeList[next];
            nodeList[next] = temp;
            //�� ��ȯ�� now index�� ���� ����� next index������ �����ϰ�
            //����ؼ� ������ ���������
            //���� ��ȯ�� ����
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

        //���� ������ Ȥ�� ����ū���� root �ڸ��� ������ ���ؿ� �°� �ٽ� ����
        //root (nodeList[0])�� ������ǵ���
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
            //���ʰ��� �񱳸��ϴ��� �����ʰ����� �񱳸��Ѵ� 
            //nextIndex�� �̹� left�� ����Ȼ��¶��ϴ��� right�� �ѹ��� ���Ѵ�.
            if (rightChildIndex <= lastIndex && nodeList[nextIndex].F > nodeList[rightChildIndex].F)
            {
                nextIndex = rightChildIndex;
            }
            //leftChildIndex rightChildIndex�� �񱳸��غ����� ���� if������ �ɸ��� �ʰ�
            //nextIndex�� ���� nowIndex���� ������ ���°��� �ΰ��� child��� ��� �� ���� ���̴�
            //���� ��ȯ�� �ʿ䰡���� => while�� ��������
            if (nextIndex == nowIndex)
            {
                //���̻� nextIndex�� ������ �Ͼ���ʰ� child��尡 ���ų� �ڽź��� ��������
                //������쿡�� ����� ���ġ�� �������̹Ƿ� while�� ����
                break;
            }
            //while�� �������°� �ƴ϶�� left�� righrt�߿� �����̻��ܼ� nextIndex�� ������ ����
            //���� ��ȯ����
            Node temp = nodeList[nowIndex];
            nodeList[nowIndex] = nodeList[nextIndex];
            nodeList[nextIndex] = temp;
            //�� ��ȯ�Ŀ� nowIndex�� ��ȯ�� nextIndex�� �����ؾ�
            //�ٽ� ���� child node�� ���ϸ鼭 �˻縦 �����ϴ� nowIndex���� nextIndex������ �����Ѵ�.
            nowIndex = nextIndex;
        }
        //��ó�� �����صξ��� root�� ���� ���������� ��ȯ
        return returnValue;
    }
    protected void ClearAll()
    {
        nodeList.Clear();
        parentList.Clear();
        dic_Close.Clear();
        dic_parent.Clear();
        //�̹� ������ �ѹ� ��� Ž���������� �� ������ F���� ����Ǿ� �ִ»���
        //���¸���Ʈ�� ���� �����ÿ� �����ϹǷ� ������¾ʰ� �ʿ��� ���� �ʱ�ȭ�Ѵ�
        //dic_Open�� F���� ���� �ٽ� ó�� ���·� �ʱ�ȭ
        foreach (var v in dic_Open.Values)
        {
            //���¸���Ʈ�� �߰��Ǿ� �ִ� F���� �ʱⰪ���� ������
            v.ResetF(defaultValue);
        }
    }
}
