using System;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.IO;

public class GetCameraFrame : MonoBehaviour
{
    public Camera arCamera;
    public RawImage camFrame;//실시간 카메라 프레임
    public RawImage captureImg;//캡쳐된 프레임 포인트 클라우드 color를 위한 캡쳐이미지
    public ARCameraManager cameraManager;
    public ARPointCloudManager pcdManager;
    public TMP_Text imgSizeTxt;
    public TMP_Text errorTxt;
    public TMP_Text saveTxt;
    protected Texture2D cameraTexture;
    protected int screenW;
    protected int screenH;
    string info = string.Empty;

    //로그 체크용으로만 사용하는변수 
    bool first = true;
    bool firstPcd = true;

    bool isProcess = false;
    protected List<Vector3> pcdList = new List<Vector3>();
    protected List<Vector3> tempPcdList = new List<Vector3>();
    protected List<Vector3> cameraPathList = new List<Vector3>();
    protected List<Color> colorList = new List<Color>();
    protected int saveCnt = 0;
    protected int maxCnt = 100000;
    //아이패드 screen 해상도 2388, 1688
    void Start()
    {
        saveCnt = 0;
        if (arCamera == null)
        {
            arCamera = Camera.main;
        }
        if (cameraManager == null)
        {
            cameraManager = GetComponent<ARCameraManager>();
        }
        CheckScreenSize();
        cameraManager.frameReceived += OnReceivedCameraFrame;
        pcdManager.pointCloudsChanged += OnPointCloudChanged;
    }
    private void Update()
    {
        CurrentSaveState();
    }
    unsafe void OnReceivedCameraFrame(ARCameraFrameEventArgs args)
    {
        if (isProcess == false)
        {
            isProcess = true;
            try
            {
                if (cameraManager != null)
                {
                    if (!cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
                    {
                        return;
                    }
                    if (first)
                    {
                        first = false;
                        info += string.Format("imgW:{0} imgH:{1}\n", image.width, image.height);
                    }
                    imgSizeTxt.text = info;

                    var conversionParams = new XRCpuImage.ConversionParams
                    {
                        inputRect = new RectInt(0, 0, image.width, image.height),
                        outputDimensions = new Vector2Int(image.width, image.height),
                        outputFormat = TextureFormat.RGB24,
                        transformation = XRCpuImage.Transformation.MirrorX
                    };


                    int size = image.GetConvertedDataSize(conversionParams);
                    var buffer = new NativeArray<byte>(size, Allocator.Temp);
                    image.Convert(conversionParams, new IntPtr(buffer.GetUnsafePtr()), buffer.Length);

                    captureImg.rectTransform.sizeDelta =
                        new Vector2(conversionParams.outputDimensions.x, conversionParams.outputDimensions.y);
                    captureImg.rectTransform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                    captureImg.texture = cameraTexture;
                    image.Dispose();

                    if (cameraTexture == null)
                    {
                        cameraTexture = new Texture2D
                                (
                                conversionParams.outputDimensions.x,
                                conversionParams.outputDimensions.y,
                                conversionParams.outputFormat,
                                false
                                );
                    }
                    cameraTexture.LoadRawTextureData(buffer);
                    cameraTexture.Apply();
                    buffer.Dispose();
                }
            }
            catch (System.Exception e)
            {
                errorTxt.text = e.Message;
            }
        }
    }
    protected void OnPointCloudChanged(ARPointCloudChangedEventArgs args)
    {
        if(firstPcd)
        {
            firstPcd = false;
            info += string.Format("point cloud get");
        }
        if(isProcess)
        {
            foreach(ARPointCloud pointCloud in args.added)
            {
                if(pointCloud.positions.HasValue)
                {
                    //pcdList.AddRange(pointCloud.positions.Value);
                    tempPcdList.Clear();
                    tempPcdList.AddRange(pointCloud.positions.Value); //현재 읽어온것을 임시로 받아옴
                    cameraPathList.Add(arCamera.transform.position);
                    
                   for(int i=0; i<tempPcdList.Count;i++)
                    {
                        Vector3 screenPos = arCamera.WorldToScreenPoint(tempPcdList[i]);
                        int x = (int)screenPos.x;
                        int y = (int)screenPos.y;
                        if(cameraTexture == null)
                        {
                            //Color pcdColor = Color.cyan;
                            //colorList.Add(pcdColor);
                        }
                        else
                        {
                            var pos = ChangeScreenPosionToTexturePos(x, y, cameraTexture);
                            if (pos.Item1 >= 0 && pos.Item1 < cameraTexture.width
                                && pos.Item2 >= 0 && pos.Item2 < cameraTexture.height)
                            {
                                Color pcdColor = cameraTexture.GetPixel(pos.Item1, pos.Item2);
                                colorList.Add(pcdColor);
                                pcdList.Add(tempPcdList[i]);
                            }
                            //else
                            //{
                            //    Color pcdColor = Color.magenta;
                            //    colorList.Add(pcdColor);
                            //}
                        }
                    }
                }
            }
            foreach(ARPointCloud pointCloud in args.updated)
            {
                if(pointCloud.positions.HasValue)
                {
                    //pcdList.AddRange(pointCloud.positions.Value);
                    tempPcdList.Clear();
                    tempPcdList.AddRange(pointCloud.positions.Value);
                    cameraPathList.Add(arCamera.transform.position);
                    for(int i=0; i<tempPcdList.Count;i++)
                    {
                        Vector3 screenPos = arCamera.WorldToScreenPoint(tempPcdList[i]);
                        int x = (int)screenPos.x;
                        int y = (int)screenPos.y;
                        if(cameraTexture == null)
                        {
                            //Color pcdColor = Color.cyan;
                            //colorList.Add(pcdColor);
                        }
                        else
                        {
                            var pos = ChangeScreenPosionToTexturePos(x, y, cameraTexture);
                            if (pos.Item1 >= 0 && pos.Item1 < cameraTexture.width
                                && pos.Item2 >= 0 && pos.Item2 < cameraTexture.height)
                            {
                                Color pcdColor = cameraTexture.GetPixel(pos.Item1, pos.Item2);
                                pcdList.Add(tempPcdList[i]);
                                colorList.Add(pcdColor);
                            }
                            //else
                            //{
                            //    Color pcdColor = Color.magenta;
                            //    colorList.Add(pcdColor);
                            //}
                        }
                    }
                }
            }
            SavePCD();
        }
        isProcess = false;
    }
    protected (int,int) ChangeScreenPosionToTexturePos(int screenX, int screenY, Texture2D texture)
    {
        //screen pos -> noamalize
        float tempX = screenX / screenW;
        float tempY = screenY / screenH;
        int textureW = texture.width;
        int textureH = texture.height;
        int posX = (int)(textureW * tempX);
        int posY = (int)(textureH * tempY);
        return (posX, posY);
    }
    protected void SavePCD()
    {
        if(pcdList.Count >= maxCnt)
        {
            //save
            string pcdFileName = string.Format("PCDFile{0}.bin", saveCnt);
            string camFileName = string.Format("CamPathFile{0}.bin", saveCnt);
            string colorFileName = string.Format("colorFile{0}.bin", saveCnt);

            string pcdPath = Path.Combine(Application.persistentDataPath, pcdFileName);
            string camPath = Path.Combine(Application.persistentDataPath, camFileName);
            string colorPath = Path.Combine(Application.persistentDataPath, colorFileName);

            using (BinaryWriter writer = new BinaryWriter(File.Open(pcdPath, FileMode.Create)))
            {
                writer.Write(pcdList.Count);
                foreach (Vector3 point in pcdList)
                {
                    writer.Write(point.x);
                    writer.Write(point.y);
                    writer.Write(point.z);
                }
            }
            using(BinaryWriter writer = new BinaryWriter(File.Open(camPath,FileMode.Create)))
            {
                writer.Write(cameraPathList.Count);
                foreach (Vector3 point in cameraPathList)
                {
                    writer.Write(point.x);
                    writer.Write(point.y);
                    writer.Write(point.z);
                }
            }
            using (BinaryWriter writer = new BinaryWriter(File.Open(colorPath,FileMode.Create)))
            {
                writer.Write(colorList.Count);
                foreach(Color color in colorList)
                {
                    writer.Write(color.r);
                    writer.Write(color.g);
                    writer.Write(color.b);
                }
            }
            saveCnt += 1;
            pcdList.Clear();
            cameraPathList.Clear();
            colorList.Clear();
        }
    }
    protected void CurrentSaveState()
    {
        string s = string.Format("Current PCD Cnt : {0} saveFile Cnt : {1} color Cnt :{2} cameraPath Cnt{3}"
            , pcdList.Count, saveCnt,colorList.Count,cameraPathList.Count);
        saveTxt.text = s;
    }
    protected void CheckScreenSize()
    {
        screenW = Screen.width;
        screenH = Screen.height;
        info = string.Format("SW:{0} SH:{1}\n", screenW, screenH);
    }
}
