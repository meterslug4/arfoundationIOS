using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.IO;
using UnityEngine.UI;
using UnityEngine.XR.ARSubsystems;
using Unity.Collections;


public class GetIOSData : MonoBehaviour
{
    public ARPointCloudManager pcdManager; //ARFoundation Point cloud Manager
    protected int saveFileCnt = 0;
    protected List<Vector3> pcdList = new List<Vector3>();//point cloud position
    protected List<Color> colorList = new List<Color>();
    //protected int idx = 0;
    protected int listCnt = 200000;
    protected string saveFilePath;
    protected string saveColorPath;

    public ARCameraManager arCameraManager;
    public Camera arCam;
    protected List<Vector3> cameraPath = new List<Vector3>(); //camera position list
    protected float interval = 1.0f;
    protected float saveTime = 0.0f;
    protected Texture2D cameraTexture;

    protected int skip;


    public Button saveBtn;//현재까지의 pcdList(20만개가 되지않아도 저장을함) cameraPosList까지 저장
    protected bool isSave = true;
    private void Awake()
    {
        skip = 20;
        arCam = Camera.main;
        pcdManager = GameObject.FindObjectOfType<ARPointCloudManager>();
        if (pcdManager != null)
        {
            pcdManager.pointCloudsChanged += PointCloudsChanged;
        }
    }
    void Start()
    {
        cameraPath.Add(arCam.transform.position);
        saveBtn.onClick.AddListener(Save);
    }
    private void Update()
    {
        //AddCameraPositions();
    }
    private void OnEnable()
    {
        arCameraManager.frameReceived += OnCameraFrameReceived;
    }
    private void OnDisable()
    {
        arCameraManager.frameReceived -= OnCameraFrameReceived;
    }
    void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
    {
        if(Time.frameCount % skip !=0)
        {
            return;
        }
        if (!arCameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
        {
            return;
        }

        // 카메라 이미지를 처리하는 코드
        // XRCpuImage를 Texture2D로 변환하여 사용 가능
        var conversionParams = new XRCpuImage.ConversionParams
        {
            inputRect = new RectInt(0, 0, image.width, image.height),
            outputDimensions = new Vector2Int(image.width, image.height),
            outputFormat = TextureFormat.RGBA32,
            transformation = XRCpuImage.Transformation.None
        };

        cameraTexture = new Texture2D(image.width, image.height, TextureFormat.RGBA32, false);
        var rawTextureData = cameraTexture.GetRawTextureData<byte>();
        image.Convert(conversionParams, new NativeArray<byte>(rawTextureData, Allocator.Temp));
        cameraTexture.Apply();

        image.Dispose();
    }
    protected void AddCameraPositions()
    {
        if(isSave)
        {
            saveTime += Time.deltaTime;
            if(saveTime >= interval)
            {
                cameraPath.Add(arCam.transform.position);
                saveTime = 0.0f;
            }
        }
    }
    protected void PointCloudsChanged(ARPointCloudChangedEventArgs args)
    {
        if(Time.frameCount % skip != 0)
        {
            return;
        }
        if(isSave)
        {
            foreach(ARPointCloud pointCloud in args.added)
            {
                if(pointCloud.positions.HasValue)
                {
                    pcdList.AddRange(pointCloud.positions.Value);
                    cameraPath.Add(arCam.transform.position);
                    for (int i=0; i<pcdList.Count; i++)
                    {
                        Vector3 screenPoint = arCam.WorldToScreenPoint(pcdList[i]);
                        int pixelX = (int)screenPoint.x;
                        int pixelY = (int)screenPoint.y;
                        try
                        {
                            if (pixelX >= 0 && pixelX < cameraTexture.width && pixelY >= 0 && pixelY < cameraTexture.height)
                            {
                                Color pointColor = cameraTexture.GetPixel(pixelX, pixelY);  // 해당 2D 좌표의 색상 정보 추출
                                colorList.Add(pointColor);
                            }
                        }
                        catch(System.Exception e)
                        {
                            Color pointColor = Color.black;
                            colorList.Add(pointColor);
                        }
                    }
                }
            }

            foreach(ARPointCloud pointCloud in args.updated)
            {
                if (pointCloud.positions.HasValue)
                {
                    pcdList.AddRange(pointCloud.positions.Value);
                    cameraPath.Add(arCam.transform.position);
                    for (int i = 0; i < pcdList.Count; i++)
                    {
                        Vector3 screenPoint = arCam.WorldToScreenPoint(pcdList[i]);
                        int pixelX = (int)screenPoint.x;
                        int pixelY = (int)screenPoint.y;

                        try
                        {
                            if (pixelX >= 0 && pixelX < cameraTexture.width && pixelY >= 0 && pixelY < cameraTexture.height)
                            {
                                Color pointColor = cameraTexture.GetPixel(pixelX, pixelY);  // 해당 2D 좌표의 색상 정보 추출
                                colorList.Add(pointColor);
                            }
                        }
                        catch (System.Exception e)
                        {
                            Color pointColor = Color.black;
                            colorList.Add(pointColor);
                        }
                    }
                }
            }

            UpdatePCDFile();
        }
    }
    protected void UpdatePCDFile()
    {
        if(pcdList.Count == listCnt)
        {
            SavePCDToBIN(); //바이너리로 저장후
            saveFileCnt += 1;//파일카운드 1증가
            pcdList.Clear();//리스트 초기화
            colorList.Clear();
            cameraPath.Clear();
        }
    }
    protected void SavePCDToBIN()
    {
        string fileName = string.Format("PCD{0}.bin", saveFileCnt);
        saveFilePath = Path.Combine(Application.persistentDataPath, fileName);
        using (BinaryWriter writer = new BinaryWriter(File.Open(saveFilePath, FileMode.Create)))
        {
            // 포인트의 개수를 먼저 저장
            writer.Write(pcdList.Count);
            // 각 포인트 좌표를 저장
            foreach (Vector3 point in pcdList)
            {
                writer.Write(point.x);
                writer.Write(point.y);
                writer.Write(point.z);
            }
        }
        string colorFileName = string.Format("PCDColor{0}.bin", saveFileCnt);
        saveColorPath = Path.Combine(Application.persistentDataPath, colorFileName);
        using (BinaryWriter writer = new BinaryWriter(File.Open(saveColorPath, FileMode.Create)))
        {
            // Color 리스트의 개수를 저장
            writer.Write(colorList.Count);

            // 각 Color의 r, g, b, a 값을 저장
            foreach (Color color in colorList)
            {
                writer.Write(color.r);
                writer.Write(color.g);
                writer.Write(color.b);
                writer.Write(color.a);
            }
        }
    }
    protected void Save()
    {
        if(isSave)
        {
            isSave = false;
            string pcdFileName = "LastPcd.bin";
            string pcdSavePath = Path.Combine(Application.persistentDataPath, pcdFileName);
            using (BinaryWriter writer = new BinaryWriter(File.Open(pcdSavePath, FileMode.Create)))
            {
                // 포인트의 개수를 먼저 저장
                writer.Write(pcdList.Count);
                // 각 포인트 좌표를 저장
                foreach (Vector3 point in pcdList)
                {
                    writer.Write(point.x);
                    writer.Write(point.y);
                    writer.Write(point.z);
                }
            }
            string colorFileName = "LastColor.bin";
            string saveColorPath = Path.Combine(Application.persistentDataPath, colorFileName);
            using (BinaryWriter writer = new BinaryWriter(File.Open(saveColorPath, FileMode.Create)))
            {
                // Color 리스트의 개수를 저장
                writer.Write(colorList.Count);

                // 각 Color의 r, g, b, a 값을 저장
                foreach (Color color in colorList)
                {
                    writer.Write(color.r);
                    writer.Write(color.g);
                    writer.Write(color.b);
                    writer.Write(color.a);
                }
            }

            string camFileName = "camPath.bin";
            string camSavePath = Path.Combine(Application.persistentDataPath, camFileName);
            using (BinaryWriter writer = new BinaryWriter(File.Open(camSavePath, FileMode.Create)))
            {
                // 포인트의 개수를 먼저 저장
                writer.Write(cameraPath.Count);

                // 각 포인트 좌표를 저장
                foreach (Vector3 point in cameraPath)
                {
                    writer.Write(point.x);
                    writer.Write(point.y);
                    writer.Write(point.z);
                }
            }
        }
    }
}

