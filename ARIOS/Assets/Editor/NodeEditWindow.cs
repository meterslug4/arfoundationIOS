using Codice.CM.Common;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System;
using Newtonsoft.Json;
public class NodeEditWindow : EditorWindow
{
    protected Node prefab;
    protected string NodePrefabPath = "Assets/Prefabs/NodeEdit/Node.prefab";

    protected Node startNode;
    protected int listSize = 0;
    protected List<Node> linkNodeList_Eidt = new List<Node>();
    protected string jsonFilePath = "No File Selected";
    protected List<Item> itemList = new List<Item>();//json���� ���� �о�� pos����

    [MenuItem("Custom/NodeEdit")]
    public static void Init()
    {
        NodeEditWindow window = (NodeEditWindow)EditorWindow.GetWindow(typeof(NodeEditWindow));
    }
    void OnGUI()
    {
        #region ������
        GUILayout.Label("Node ����", EditorStyles.boldLabel);
        NodePrefabPath = EditorGUILayout.TextField("Prefab Path", NodePrefabPath);
        if (GUILayout.Button("Node ����"))
        {
            CreateNode();
        }

        GUILayout.Label("����������������������������������������������������������");
        if (GUILayout.Button("Json���� ����"))
        {
            SelectNodeJsonFile();
        }
        #endregion
        
        GUILayout.Label("Selected File: ");
        GUILayout.TextField(jsonFilePath);

        if(GUILayout.Button("Json���Ϸ� Node ����"))
        {
            CreateNodesFromJson();
        }
        GUILayout.Label("����������������������������������������������������������");
        if (GUILayout.Button("Link ����"))
        {
            CreateLink();
        }

        if (GUILayout.Button("Save"))
        {
            SaveNodeLinkData();
        }
        if (GUILayout.Button("Load"))
        {
            LoadNodeLinkData();
        }
        if (GUILayout.Button("Delete"))
        {
            Delete();
        }
    }
    void SelectNodeJsonFile()
    {
        jsonFilePath = EditorUtility.OpenFilePanel("Select JSON File", "", "json");
    }
    void CreateNodesFromJson()
    {
        try
        {
            //if(!string.IsNullOrEmpty(jsonFilePath))
            //{
            //    string json = Utils.LoadJsonFile(jsonFilePath);
            //    var obj = JsonConvert.DeserializeObject<ItemListWrapper>(json);
            //    itemList = obj.items;
            //    if(itemList.Count >0)
            //    {
            //        for(int i=0; i<itemList.Count; i++)
            //        {
            //            CreateNode(itemList[i].pos);
            //        }
            //    }
            //}
            if(!string.IsNullOrEmpty(jsonFilePath))
            {
                string json = Utils.LoadJsonFile(jsonFilePath);
                var obj = JsonConvert.DeserializeObject<Vector3List>(json);
                List<Vector3Serialize> list = obj.vectors;
                if(list.Count >0)
                {
                    for(int i=0; i<list.Count; i++)
                    {
                        CreateNode(list[i].GetVector3());
                    }
                }
            }
        }
        catch(Exception e)
        {
            Debug.Log(e);
        }
    }
    /// <summary>
    /// ������ ������ ����� Node ������ �ҷ��ͼ� ����
    /// </summary>
    void CreateNode()
    {
        GameObject nodeRoot = GameObject.Find(Constant.NodeRoot);
        if (nodeRoot == null)
        {
            nodeRoot = new GameObject(Constant.NodeRoot);
        }
        nodeRoot.transform.position = Vector3.zero;
        prefab = AssetDatabase.LoadAssetAtPath<Node>(NodePrefabPath);
        if (prefab != null)
        {
            Node n = Instantiate(prefab);
            n.transform.position = Vector3.zero;
            n.pos = n.transform.position;
            n.nodeId = n.GetInstanceID();
            n.F = 99999.0f;
            n.gameObject.name = string.Format("Node {0}", n.nodeId);
            n.SetText(n.gameObject.name);
            n.transform.parent = nodeRoot.transform;
        }
    }
    void CreateNode(Vector3 pos)
    {
        GameObject nodeRoot = GameObject.Find(Constant.NodeRoot);
        if (nodeRoot == null)
        {
            nodeRoot = new GameObject(Constant.NodeRoot);
        }
        nodeRoot.transform.position = Vector3.zero;
        prefab = AssetDatabase.LoadAssetAtPath<Node>(NodePrefabPath);
        if (prefab != null)
        {
            Node n = Instantiate(prefab);
            n.transform.position = pos;
            n.pos = n.transform.position;
            n.nodeId = n.GetInstanceID();
            n.F = 99999.0f;
            n.gameObject.name = string.Format("Node {0}", n.nodeId);
            n.SetText(n.gameObject.name);
            n.transform.parent = nodeRoot.transform;
        }
    }
    /// <summary>
    /// ���η����� ���� ������ ����
    /// </summary>
    void CreateLink()
    {
        GameObject linkRoot = GameObject.Find(Constant.LinkRoot);
        if (linkRoot == null)
        {
            linkRoot = new GameObject(Constant.LinkRoot);
        }
        linkRoot.transform.position = Vector3.zero;
        NodeMultiSelect();
        bool isCreatedLink = false; // �̹� ������ ��ũ�ΰ�? ��ũ �������� ��������
        for (int i = 0; i < linkNodeList_Eidt.Count; i++)
        {
            isCreatedLink = false;
            //�ڱ� �ڽŰ��� ������ �����Ѵ�
            if (linkNodeList_Eidt[i].nodeId == startNode.nodeId)
            {
                Debug.Log("�ڽŰ��� ��ũ�� �����Ҽ� ���� ���� ���� ����");
                continue;
            }
            //�̹� ��ũ�� ����Ǿ� �ִ� ������ �����̶�� �����Ѵ�(�ߺ��� ������ ����)
            //startNode�� ���� ����ɾ��ִ� ��ũ���������´�.
            var list = startNode.relatedDataList;
            //�̹� ������ ����Ʈ�� ������
            if (list.Count > 0)
            {
                for (int k = 0; k < list.Count; k++)
                {
                    var nodes = list[k].linkObject.GetStartEndNode();
                    //startNode�� ����� ��ũ���߿��� ���� �����Ϸ��� linkNodeList_Edit�� ��尡
                    //�̹� startNode�� ��ũ�� startNode�� endNode�� ���ԵǾ� �ִٸ� �̹� ������ �ȳ��
                    if (nodes.Item1.nodeId == linkNodeList_Eidt[i].nodeId ||
                        nodes.Item2.nodeId == linkNodeList_Eidt[i].nodeId)
                    {
                        Debug.Log(string.Format("{0}��{1}�� �̹� ��ũ���谡 �����Ǿ� �ִ�", startNode,
                            linkNodeList_Eidt[i]));
                        isCreatedLink = true;
                        break;
                    }
                }
            }
            if (isCreatedLink == true)
            {
                continue;
            }
            if (linkNodeList_Eidt[i] != null)
            {
                RelatedData rData1 = new RelatedData();
                rData1.adjacentNodes = linkNodeList_Eidt[i];
                //startNode���� ���� ������ �̵����
                rData1.g = startNode.SetLinkNodeCost(linkNodeList_Eidt[i].transform.position);
                rData1.linkObject = null;

                RelatedData rData2 = new RelatedData();
                rData2.adjacentNodes = startNode;
                //���� ��忡�� startNode���� �̵����
                rData2.g = linkNodeList_Eidt[i].SetLinkNodeCost(startNode.transform.position);
                rData2.linkObject = null;
                //��ũ ����
                LinkObject link = new GameObject().AddComponent<LinkObject>();
                link.SetLink(startNode, linkNodeList_Eidt[i]);
                link.transform.parent = linkRoot.transform;

                rData1.linkObject = link;
                rData2.linkObject = link;

                startNode.relatedDataList.Add(rData1);
                linkNodeList_Eidt[i].relatedDataList.Add(rData2);
            }
        }
        startNode = null;
        linkNodeList_Eidt.Clear();
    }
    //��Ƽ ����Ʈ�� ���� ��ũ�� ����
    //���� ������ ����� �������� �׵ڷ� ��Ƽ ����Ʈ �� ������ �����Ѵ�
    //�����Ϳ��� ����ϴ� linkNodeList_Edit�� ������ ������Ʈ���� ������� �־��ش�
    //ó�� ������ ��尡 ���۳�尡 �ȴ�
    void NodeMultiSelect()
    {
        var objs = Selection.gameObjects;
        //�ּ� 2���� ���õǾ���
        if (objs.Length >= 2)
        {
            startNode = null;
            linkNodeList_Eidt.Clear();
            for (int i = 0; i < objs.Length; i++)
            {
                if (i == 0)
                {
                    startNode = objs[i].GetComponent<Node>();
                }
                else
                {
                    linkNodeList_Eidt.Add(objs[i].GetComponent<Node>());
                }
            }
        }
    }

    void Delete()
    {
        //���õ� ������Ʈ�� Node�϶� link�ϋ� ����
        //���õ� ��带 �����ϱ��� �ش� ���� ���� ���� �����Ǿ� �ִ� ������ ��ȸ�ϸ鼭
        //linkNodeList���� ��������������.
        //�̵� ��뿡 ���Ѱ͵� ���� �����Ѵ�
        //������ linkObject�� ���� �����Ѵ�
        Node deleteTarget = Selection.activeGameObject.GetComponent<Node>();//���� ����̵� ���
        //��������� �������� ������� ���� ���õ� ������ �����
        for (int i = 0; i < deleteTarget.relatedDataList.Count; i++)
        {
            //�������� ����� ���� ������ �湮�Ͽ� ����������� ������ ���´�.
            Node adNode = deleteTarget.relatedDataList[i].adjacentNodes;
            int deleteIndx = 0;
            for (int k = 0; k < adNode.relatedDataList.Count; k++)
            {
                //�����ؾ��� ��� �Ͱ��õ� ���� �߰�
                if (adNode.relatedDataList[k].adjacentNodes.nodeId == deleteTarget.nodeId)
                {
                    //������ ���
                    //adNode.relatedDataList.RemoveAt(k);
                    deleteIndx = k;
                }
            }
            adNode.relatedDataList.RemoveAt(deleteIndx);
            //��带 �����ϱ����� ���� ������ ��ũ '������Ʈ����' ����)
            DestroyImmediate(deleteTarget.relatedDataList[i].linkObject.gameObject);
        }
        //�ص� ��� ����, ���� ��ũ ���λ���
        DestroyImmediate(deleteTarget.gameObject);
    }

    /// <summary>
    /// ���� ���� �����Ǿ��ִ� node link data ����
    /// </summary>
    protected void SaveNodeLinkData()
    {
        NodeLInkData saveData = new NodeLInkData();
        var nodes = GameObject.FindObjectsOfType<Node>();
        for (int i = 0; i < nodes.Length; i++)
        {
            //saveData.saveNodeList.Add(nodes[i]);
            //i��° ������ ������ �����Ѵ�
            NodeInfo nodeInfo = new NodeInfo();
            nodeInfo.x = nodes[i].pos.x;
            nodeInfo.y = nodes[i].pos.y;
            nodeInfo.z = nodes[i].pos.z;
            nodeInfo.nodeId = nodes[i].nodeId;
            nodeInfo.F = nodes[i].F;
            if (!string.IsNullOrEmpty(nodes[i].nodeName))
            {
                nodeInfo.nodeName = nodes[i].nodeName;
            }
            else
            {
                nodeInfo.nodeName = nodes[i].gameObject.name;
            }
            //������ ������ �Ƶ� ����
            for (int k = 0; k < nodes[i].relatedDataList.Count; k++)
            {
                nodeInfo.relateNodeId.Add(nodes[i].relatedDataList[k].adjacentNodes.nodeId);
            }
            saveData.saveNodeList.Add(nodeInfo);
        }
        var links = GameObject.FindObjectsOfType<LinkObject>();
        for (int i = 0; i < links.Length; i++)
        {
            LinkInfo linkInfo = new LinkInfo();
            linkInfo.startNodeId = links[i].startNode.nodeId;
            linkInfo.endNodeId = links[i].endNode.nodeId;
            linkInfo.name = links[i].name;
            saveData.saveLinkList.Add(linkInfo);
        }
        //-----------saveData �ۼ� �Ϸ�----------------
        string json = JsonUtility.ToJson(saveData, true);
        Debug.Log(json);
        string path = Application.dataPath + "/NodeLinkJson" + "/JsonData.json";
        Debug.Log(path);
        File.WriteAllText(path, json);
    }
    /// <summary>
    /// ����� node link data �ҷ�����
    /// </summary>
    protected void LoadNodeLinkData()
    {
        string path = Application.dataPath + "/NodeLinkJson" + "/JsonData.json";
        GameObject nodeRoot = GameObject.Find(Constant.NodeRoot);
        if (nodeRoot == null)
        {
            nodeRoot = new GameObject(Constant.NodeRoot);
        }
        GameObject linkRoot = GameObject.Find(Constant.LinkRoot);
        if (linkRoot == null)
        {
            linkRoot = new GameObject(Constant.LinkRoot);
        }
        nodeRoot.transform.position = Vector3.zero;
        linkRoot.transform.position = Vector3.zero;
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            NodeLInkData loadData = JsonUtility.FromJson<NodeLInkData>(json);
            //�����͸� �о�ͼ� ���� ������Ʈ �� �����������

            //node �ٽ� ����

            //nodeID, Node,Node�� ������ Node���� ID
            Dictionary<int, Node> createdNode = new Dictionary<int, Node>();

            for (int i = 0; i < loadData.saveNodeList.Count; i++)
            {
                Node n = AssetDatabase.LoadAssetAtPath<Node>(NodePrefabPath);
                Node node = Instantiate(n);
                //List<int> relateNodes = new List<int>();
                //GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                //Node node = obj.AddComponent<Node>();
                Vector3 nodePos = new Vector3(loadData.saveNodeList[i].x, loadData.saveNodeList[i].y, loadData.saveNodeList[i].z);
                node.transform.position = nodePos;
                node.pos = nodePos;
                node.nodeId = loadData.saveNodeList[i].nodeId;
                node.name = loadData.saveNodeList[i].nodeName;
                node.gameObject.name = node.name;
                node.SetText(node.name);
                node.F = loadData.saveNodeList[i].F;
                //relateNodes = loadData.saveNodeList[i].relateNodeId;
                createdNode.Add(node.nodeId, node);
                node.transform.parent = nodeRoot.transform;
            }
            for (int i = 0; i < loadData.saveLinkList.Count; i++)
            {
                GameObject obj = new GameObject(loadData.saveLinkList[i].name);
                LinkObject link = obj.AddComponent<LinkObject>();
                int startId = loadData.saveLinkList[i].startNodeId;
                int endId = loadData.saveLinkList[i].endNodeId;
                link.SetLink(createdNode[startId], createdNode[endId]);
                link.name = loadData.saveLinkList[i].name;

                Node startNode = createdNode[startId];
                Node endNode = createdNode[endId];

                //startNode�� �߰����� RelatedData
                RelatedData rData1 = new RelatedData();
                rData1.adjacentNodes = endNode;
                rData1.g = startNode.SetLinkNodeCost(endNode.transform.position);
                rData1.linkObject = link;
                startNode.relatedDataList.Add(rData1);

                //endNode�� �߰����� RelatedData
                RelatedData rData2 = new RelatedData();
                rData2.adjacentNodes = startNode;
                rData2.g = startNode.SetLinkNodeCost(startNode.transform.position);
                rData2.linkObject = link;
                endNode.relatedDataList.Add(rData2);

                link.transform.parent = linkRoot.transform;
            }
        }
    }
}


/// <summary>
/// ������ �󿡼� �׻� ����Ǿ��ϴ� update���� ó��
/// </summary>
[InitializeOnLoad]
public class AlwaysRunningEditor
{
    static AlwaysRunningEditor()
    {
        EditorApplication.update += Update;
    }
    protected static void Update()
    {
        if (Application.isPlaying)
        {
            //Debug.Log("��Ÿ�� ���� ���� X");
            return;
        }
        if (Selection.activeGameObject != null)
        {
            Node node = Selection.activeGameObject.GetComponent<Node>();
            //�̵��� ��尡 ��ũ�� ����Ǿ� �ִٸ�?
            //����� ���� �̵��ϴµ� ��� ��뵵 ���ϰԵȴ� node�� linkNodeCost ��ųʸ��� ����������Ѵ�
            if (node != null)
            {
                node.pos = node.transform.position; //�̵��ϸ鼭 �������� ����Ǹ� �׻� �����ǰ��� ����
                //���� ��ġ�� �̵���ų�� �ڽŰ� ���� �ִ� ��� ��ũ���������´�.
                for (int i = 0; i < node.relatedDataList.Count; i++)
                {
                    //��ũ���� ���� ��尡 start ���� endd���� Ȯ���Ͽ� ���η����� ���� ����
                    node.relatedDataList[i].linkObject.ChangeLinkPosition(node);
                    float tempG = node.SetLinkNodeCost(node.relatedDataList[i].adjacentNodes.transform.position);
                    node.relatedDataList[i].g = tempG;
                    //���� ������ relatedDataList������ �̵��� node�� ã�Ƽ� �ش��ϴ� G ���� �ٲ��ش�
                    Node adNode = node.relatedDataList[i].adjacentNodes;
                    for (int k = 0; k < adNode.relatedDataList.Count; k++)
                    {
                        if (adNode.relatedDataList[k].adjacentNodes.nodeId == node.nodeId)
                        {
                            adNode.relatedDataList[k].g = tempG;
                        }
                    }
                }
            }
        }
    }
}