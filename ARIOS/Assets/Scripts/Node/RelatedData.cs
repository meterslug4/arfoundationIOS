using System.Collections.Generic;
using System.Collections;
using System.Security.Permissions;
using UnityEngine;
[System.Serializable]
public class RelatedData
{
    public Node adjacentNodes;//인접노드
    public float g; //인접 노드까지의 이동비용
    public LinkObject linkObject;//인접노드사이에 생성된 링크
}
[System.Serializable]
public class NodeLInkData
{
    public List<NodeInfo> saveNodeList = new List<NodeInfo>();
    public List<LinkInfo> saveLinkList = new List<LinkInfo>();
}
[System.Serializable]
public class NodeInfo
{
    //public Vector3 pos;
    public float x;
    public float y;
    public float z;
    public int nodeId;
    public string nodeName;
    public List<int> relateNodeId = new List<int>();
    public float F;
}
[System.Serializable]
public class LinkInfo
{
    //startNode
    public int startNodeId;
    //endNode
    public int endNodeId;
    //name
    public string name;
}

public static class Constant
{
    public static readonly string NodeRoot = "NodeRoot";
    public static readonly string LinkRoot = "LinkRoot";
}
[System.Serializable]
public class Item
{
    public Vector3 pos;
}
[System.Serializable]
public class ItemListWrapper
{
    public List<Item> items;
    public ItemListWrapper(List<Item> itemList)
    {
        items = itemList;
    }
}
public static class CONST
{
    public const string NOT = "Nothing";
    public const string YOON = "Yoon";
    public const string JEON = "Jeon";
    public const string CONFERENCE1 = "Conference_Big";
    public const string EXIT = "Exit";
    public const string CONFERENCE2 = "Conference_Small";
}