using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
public class ImageTrackBase : MonoBehaviour
{
    public UIController uiController;
    protected ARTrackedImageManager imageManager;
    protected IReferenceImageLibrary imageLibrary;
    protected Camera arCam;
    protected Vector3 updatePos;
    protected Vector3 firstPos;
    protected bool findImg = false;
    protected GameObject firstObj;
    protected GameObject updateObj;
    protected string firstPosInfo = string.Empty;
    protected string updatePosInfo = string.Empty;
    void Start()
    {
        firstObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        firstObj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        firstObj.transform.position = new Vector3(100, 100, 100);
        updateObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        updateObj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        updateObj.transform.position = new Vector3(100, 100, 100);

        SetImageManager();
    }
    private void Update()
    {
        string msg = string.Format("added Pos {0} updated Pos {1} cameraPos {2}",
            firstPosInfo, updatePosInfo,arCam.transform.position);
        uiController.SetImgInfoMsg(msg);
    }
    public void SetImageManager()
    {
        if(arCam == null)
        {
            arCam = Camera.main;
        }
        imageManager = GetComponent<ARTrackedImageManager>();
        imageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }
    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach(var v in eventArgs.added)
        {
            firstPos = v.transform.position;
            firstPosInfo = firstPos.ToString();
            firstObj.transform.position = firstPos;
        }
        foreach(var v in eventArgs.updated)
        {
            updatePos = v.transform.position;
            updatePosInfo = updatePos.ToString();
            updateObj.transform.position = updatePos;
        }
    }
}
