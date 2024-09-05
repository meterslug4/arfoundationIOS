using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
public class CameraPathData : MonoBehaviour
{
    protected Camera mainCamera;
    protected XROrigin xrOrigin;
    public Button addCameraPath;
    public Button saveBtn;
    public GameObject cube;
    protected List<Vector3> cameraPathList = new List<Vector3>();
    protected float cameraHeight;
    private void Awake()
    {
        cameraHeight = 0.0f;
        mainCamera = Camera.main;
        addCameraPath.onClick.AddListener(OnClickedAddCameraPath);
        saveBtn.onClick.AddListener(OnClickedSaveCameraPathListToJSON);
    }
    protected void OnClickedAddCameraPath()
    {
        Vector3 camPos = mainCamera.transform.position;
        Vector3 tempPos = new Vector3(camPos.x, cameraHeight, camPos.z);
        CreatePathObj(tempPos);
        cameraPathList.Add(tempPos);
    }
    protected GameObject CreatePathObj(Vector3 pos)
    {
        GameObject obj = Instantiate(cube, pos, Quaternion.identity);
        obj.transform.position = pos;
        return obj;
    }
    protected void OnClickedSaveCameraPathListToJSON()
    {
        Vector3List vector3List = new Vector3List();
        for(int i=0; i<cameraPathList.Count; i++)
        {
            vector3List.vectors.Add(new Vector3Serialize(cameraPathList[i]));
        }
        string json = JsonUtility.ToJson(vector3List, true);
        string path = Application.persistentDataPath + "/cameraPathList";
        File.WriteAllText(path, json);
    }
}
[Serializable]
public class Vector3Serialize
{
    public float x;
    public float y;
    public float z;
    public Vector3Serialize(Vector3 vector)
    {
        x = vector.x;
        y = vector.y;
        z = vector.z;
    }
    public Vector3 GetVector3()
    {
        return new Vector3(x, y, z);
    }
}
[SerializeField]
public class Vector3List
{
    public List<Vector3Serialize> vectors = new List<Vector3Serialize>();
}

