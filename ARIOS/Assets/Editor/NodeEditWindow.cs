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
    protected List<Item> itemList = new List<Item>();//json으로 부터 읽어온 pos정보

    [MenuItem("Custom/NodeEdit")]
    public static void Init()
    {
        NodeEditWindow window = (NodeEditWindow)EditorWindow.GetWindow(typeof(NodeEditWindow));
    }
    void OnGUI()
    {
        #region 노드생성
        GUILayout.Label("Node 생성", EditorStyles.boldLabel);
        NodePrefabPath = EditorGUILayout.TextField("Prefab Path", NodePrefabPath);
        if (GUILayout.Button("Node 생성"))
        {
            CreateNode();
        }

        GUILayout.Label("■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■");
        if (GUILayout.Button("Json파일 선택"))
        {
            SelectNodeJsonFile();
        }
        #endregion
        
        GUILayout.Label("Selected File: ");
        GUILayout.TextField(jsonFilePath);

        if(GUILayout.Button("Json파일로 Node 생성"))
        {
            CreateNodesFromJson();
        }
        GUILayout.Label("■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■");
        if (GUILayout.Button("Link 생성"))
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
    /// 지정된 프리팹 경로의 Node 프리팹 불러와서 생성
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
    /// 라인렌더러 생성 포지션 설정
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
        bool isCreatedLink = false; // 이미 생성된 링크인가? 링크 생성할지 말지결정
        for (int i = 0; i < linkNodeList_Eidt.Count; i++)
        {
            isCreatedLink = false;
            //자기 자신과의 연결은 제외한다
            if (linkNodeList_Eidt[i].nodeId == startNode.nodeId)
            {
                Debug.Log("자신과는 링크를 연결할수 없음 다음 루프 진행");
                continue;
            }
            //이미 링크가 연결되어 있는 노드와의 연결이라면 제외한다(중복된 연결은 제외)
            //startNode가 현재 연결될어있는 링크들을가져온다.
            var list = startNode.relatedDataList;
            //이미 생성된 리스트가 있을때
            if (list.Count > 0)
            {
                for (int k = 0; k < list.Count; k++)
                {
                    var nodes = list[k].linkObject.GetStartEndNode();
                    //startNode와 연결된 링크들중에서 현재 연결하려는 linkNodeList_Edit의 노드가
                    //이미 startNode의 링크의 startNode나 endNode로 포함되어 있다면 이미 연결이 된노드
                    if (nodes.Item1.nodeId == linkNodeList_Eidt[i].nodeId ||
                        nodes.Item2.nodeId == linkNodeList_Eidt[i].nodeId)
                    {
                        Debug.Log(string.Format("{0}과{1}는 이미 링크관계가 생성되어 있다", startNode,
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
                //startNode에서 인접 노드로의 이동비용
                rData1.g = startNode.SetLinkNodeCost(linkNodeList_Eidt[i].transform.position);
                rData1.linkObject = null;

                RelatedData rData2 = new RelatedData();
                rData2.adjacentNodes = startNode;
                //인접 노드에서 startNode로의 이동비용
                rData2.g = linkNodeList_Eidt[i].SetLinkNodeCost(startNode.transform.position);
                rData2.linkObject = null;
                //링크 생성
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
    //멀티 셀렉트를 통한 링크의 연결
    //먼저 선택한 대상을 기준으로 그뒤로 멀티 셀렉트 된 노드들을 연결한다
    //에디터에서 사용하는 linkNodeList_Edit에 선택한 오브젝트들을 순서대로 넣어준다
    //처음 선택한 노드가 시작노드가 된다
    void NodeMultiSelect()
    {
        var objs = Selection.gameObjects;
        //최소 2개는 선택되야함
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
        //선택된 오브젝트가 Node일때 link일떄 구분
        //선택된 노드를 삭제하기전 해당 노드와 인접 노드로 설정되어 있는 노드들을 순회하면서
        //linkNodeList에서 노드정보를지운다.
        //이동 비용에 관한것도 전부 삭제한다
        //생성된 linkObject를 전부 삭제한다
        Node deleteTarget = Selection.activeGameObject.GetComponent<Node>();//삭제 대상이될 노드
        //인접노드의 정보에서 삭제대상 노드와 관련된 정보는 지운다
        for (int i = 0; i < deleteTarget.relatedDataList.Count; i++)
        {
            //삭제노드와 연결된 인접 노드들을 방문하여 삭제도드와의 연결을 끊는다.
            Node adNode = deleteTarget.relatedDataList[i].adjacentNodes;
            int deleteIndx = 0;
            for (int k = 0; k < adNode.relatedDataList.Count; k++)
            {
                //삭제해야할 노드 와관련된 정보 발견
                if (adNode.relatedDataList[k].adjacentNodes.nodeId == deleteTarget.nodeId)
                {
                    //삭제될 노드
                    //adNode.relatedDataList.RemoveAt(k);
                    deleteIndx = k;
                }
            }
            adNode.relatedDataList.RemoveAt(deleteIndx);
            //노드를 삭제하기전에 노드와 연관된 링크 '오브젝트부터' 삭제)
            DestroyImmediate(deleteTarget.relatedDataList[i].linkObject.gameObject);
        }
        //해동 노드 삭제, 관련 링크 전부삭제
        DestroyImmediate(deleteTarget.gameObject);
    }

    /// <summary>
    /// 씬에 현재 생성되어있는 node link data 저장
    /// </summary>
    protected void SaveNodeLinkData()
    {
        NodeLInkData saveData = new NodeLInkData();
        var nodes = GameObject.FindObjectsOfType<Node>();
        for (int i = 0; i < nodes.Length; i++)
        {
            //saveData.saveNodeList.Add(nodes[i]);
            //i번째 노드들의 정보를 저장한다
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
            //인접한 노드들의 아디만 저장
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
        //-----------saveData 작성 완료----------------
        string json = JsonUtility.ToJson(saveData, true);
        Debug.Log(json);
        string path = Application.dataPath + "/NodeLinkJson" + "/JsonData.json";
        Debug.Log(path);
        File.WriteAllText(path, json);
    }
    /// <summary>
    /// 저장된 node link data 불러오기
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
            //데이터만 읽어와서 직접 오브젝트 다 생성해줘야함

            //node 다시 생성

            //nodeID, Node,Node완 인접한 Node들의 ID
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

                //startNode에 추가해줄 RelatedData
                RelatedData rData1 = new RelatedData();
                rData1.adjacentNodes = endNode;
                rData1.g = startNode.SetLinkNodeCost(endNode.transform.position);
                rData1.linkObject = link;
                startNode.relatedDataList.Add(rData1);

                //endNode에 추가해줄 RelatedData
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
/// 에디터 상에서 항상 실행되야하는 update문들 처리
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
            //Debug.Log("런타임 중임 실행 X");
            return;
        }
        if (Selection.activeGameObject != null)
        {
            Node node = Selection.activeGameObject.GetComponent<Node>();
            //이동한 노드가 링크가 연결되어 있다면?
            //연결된 노드로 이동하는데 드는 비용도 변하게된다 node의 linkNodeCost 딕셔너리도 변경해줘야한다
            if (node != null)
            {
                node.pos = node.transform.position; //이동하면서 포지션이 변경되면 항상 포지션값도 변경
                //노의 위치를 이동시킬시 자신과 관련 있는 모든 링크들을가져온다.
                for (int i = 0; i < node.relatedDataList.Count; i++)
                {
                    //링크에서 현재 노드가 start 인지 endd인지 확인하여 라인렌더러 길이 변경
                    node.relatedDataList[i].linkObject.ChangeLinkPosition(node);
                    float tempG = node.SetLinkNodeCost(node.relatedDataList[i].adjacentNodes.transform.position);
                    node.relatedDataList[i].g = tempG;
                    //인접 노드들의 relatedDataList에서도 이동한 node를 찾아서 해당하는 G 값을 바꿔준다
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