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
    public GameObject nodePivot;
    void Start()
    {
        SetImageManager();
    }
    private void Update()
    {
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
            Vector3 tempPos = new Vector3(firstPos.x, 0, firstPos.z);
            nodePivot.transform.position = tempPos;
        }
    }
}
