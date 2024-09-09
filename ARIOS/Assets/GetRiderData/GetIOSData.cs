using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.IO;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class GetIOSData : MonoBehaviour
{
    public ARPointCloudManager pcdManager; //ARFoundation Point cloud Manager
    protected int saveFileCnt = 0;
    protected List<Vector3> pcdList = new List<Vector3>();//point cloud position
    protected List<IOSData> dataList = new List<IOSData>(); //point cloud position, Color
    protected int idx = 0;
    protected int listCnt = 200000;
    protected string saveFilePath;

    public ARCameraManager arCameraManager;
    public Camera arCam;
    protected List<Vector3> cameraPath = new List<Vector3>(); //camera position list
    protected float interval = 1.0f;
    protected float saveTime = 0.0f;
    protected Texture2D cameraTexture;


    public Button saveBtn;//현재까지의 pcdList(20만개가 되지않아도 저장을함) cameraPosList까지 저장
    protected bool isSave = true;
    private void Awake()
    {
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
        AddCameraPositions();
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
        // 현재 프레임의 텍스처 정보를 가져옵니다.
        if (arCameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
        {
            // 카메라 이미지의 해상도를 가져옵니다.
            var cameraResolution = new Vector2(image.width, image.height);

            // 새로운 텍스처를 해상도에 맞게 생성
            if (cameraTexture == null || cameraTexture.width != image.width || cameraTexture.height != image.height)
            {
                cameraTexture = new Texture2D(image.width, image.height, TextureFormat.RGBA32, false);
            }

            // 카메라 이미지 데이터를 텍스처에 복사
            var conversionParams = new XRCpuImage.ConversionParams
            {
                inputRect = new RectInt(0, 0, image.width, image.height),
                outputDimensions = new Vector2Int(image.width, image.height),
                outputFormat = TextureFormat.RGBA32,
                transformation = XRCpuImage.Transformation.None
            };

            var rawTextureData = cameraTexture.GetRawTextureData<byte>();
            image.Convert(conversionParams, rawTextureData);
            cameraTexture.Apply();

            // 이미지 사용 후 반드시 Dispose 호출
            image.Dispose();
        }
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
        if(isSave)
        {
            foreach(ARPointCloud pointCloud in args.updated)
            {
                UpdatePCDFile();
                if (pointCloud.positions.HasValue)
                {
                    pcdList.AddRange(pointCloud.positions.Value);
                    Vector3 pos = pcdList[idx];
                    Color color = GetColorAtWorldPosition(pos);
                    idx += 1;
                    IOSData data = new IOSData(pos,color);
                    dataList.Add(data);
                }
            }
        }
    }
    public Color GetColorAtWorldPosition(Vector3 worldPosition)
    {
        Vector2 uv = GetUVFromWorldPosition(worldPosition);
        int x = Mathf.FloorToInt(uv.x * cameraTexture.width);
        int y = Mathf.FloorToInt(uv.y * cameraTexture.height);

        return cameraTexture.GetPixel(x, y);
    }
    // 3D 포인트 클라우드 위치를 2D 카메라 좌표로 변환
    protected Vector2 GetUVFromWorldPosition(Vector3 worldPosition)
    {
        Vector3 screenPos = arCam.WorldToScreenPoint(worldPosition);
        return new Vector2(screenPos.x / Screen.width, screenPos.y / Screen.height);
    }
    protected void UpdatePCDFile()
    {
        if(pcdList.Count == listCnt)
        {
            SavePCDToBIN(); //바이너리로 저장후
            saveFileCnt += 1;//파일카운드 1증가
            pcdList.Clear();//리스트 초기화
            dataList.Clear();
            idx = 0;
        }
    }
    protected void SavePCDToBIN()
    {
        string fileName = string.Format("PCD{0}.bin", saveFileCnt);
        saveFilePath = Path.Combine(Application.persistentDataPath, fileName);
        using (BinaryWriter writer = new BinaryWriter(File.Open(saveFilePath, FileMode.Create)))
        {
            // 포인트의 개수를 먼저 저장
            //writer.Write(pcdList.Count);
            writer.Write(dataList.Count);
            // 각 포인트 좌표를 저장
            //foreach (Vector3 point in pcdList)
            //{
            //    writer.Write(point.x);
            //    writer.Write(point.y);
            //    writer.Write(point.z);
            //}
            foreach(IOSData data in dataList)
            {
                writer.Write(data.pos.x);
                writer.Write(data.pos.y);
                writer.Write(data.pos.z);

                writer.Write(data.color.r);
                writer.Write(data.color.g);
                writer.Write(data.color.b);
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
                //writer.Write(pcdList.Count);
                writer.Write(dataList.Count);
                // 각 포인트 좌표를 저장
                //foreach (Vector3 point in pcdList)
                //{
                //    writer.Write(point.x);
                //    writer.Write(point.y);
                //    writer.Write(point.z);
                //}

                foreach (IOSData data in dataList)
                {
                    writer.Write(data.pos.x);
                    writer.Write(data.pos.y);
                    writer.Write(data.pos.z);

                    writer.Write(data.color.r);
                    writer.Write(data.color.g);
                    writer.Write(data.color.b);
                    writer.Write(data.color.a);
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
public struct IOSData
{
    public Vector3 pos;
    public Color color;
    public IOSData(Vector3 p, Color c)
    {
        pos = p;
        color = c;
    }
}

