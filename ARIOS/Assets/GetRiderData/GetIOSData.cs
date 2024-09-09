using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.IO;
using UnityEngine.UI;
using UnityEngine.XR.ARSubsystems;

public class GetIOSData : MonoBehaviour
{
    public ARPointCloudManager pcdManager; //ARFoundation Point cloud Manager
    protected int saveFileCnt = 0;
    protected List<Vector3> pcdList = new List<Vector3>();//point cloud position
    protected List<Color> colorList = new List<Color>();
    protected int idx = 0;
    protected int listCnt = 200000;
    protected string saveFilePath;
    protected string saveColorPath;

    public ARCameraManager arCameraManager;
    public Camera arCam;
    protected List<Vector3> cameraPath = new List<Vector3>(); //camera position list
    protected float interval = 1.0f;
    protected float saveTime = 0.0f;
    protected Texture2D cameraTexture;


    public Button saveBtn;//��������� pcdList(20������ �����ʾƵ� ��������) cameraPosList���� ����
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
        // ���� �������� �ؽ�ó ������ �����ɴϴ�.
        if (arCameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
        {
            // ī�޶� �̹����� �ػ󵵸� �����ɴϴ�.
            var cameraResolution = new Vector2(image.width, image.height);

            // ���ο� �ؽ�ó�� �ػ󵵿� �°� ����
            if (cameraTexture == null || cameraTexture.width != image.width || cameraTexture.height != image.height)
            {
                cameraTexture = new Texture2D(image.width, image.height, TextureFormat.RGBA32, false);
            }

            // ī�޶� �̹��� �����͸� �ؽ�ó�� ����
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

            // �̹��� ��� �� �ݵ�� Dispose ȣ��
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
            foreach(ARPointCloud pointClod in args.added)
            {
                if(pointClod.positions.HasValue)
                {
                    pcdList.AddRange(pointClod.positions.Value);
                    Vector3 pos = pcdList[idx];
                    Color color = GetColorAtWorldPosition(pos);
                    colorList.Add(color);
                }
            }

            foreach(ARPointCloud pointCloud in args.updated)
            {
                if (pointCloud.positions.HasValue)
                {
                    pcdList.AddRange(pointCloud.positions.Value);
                    Vector3 pos = pcdList[idx];
                    Color color = GetColorAtWorldPosition(pos);
                    colorList.Add(color);
                }
            }
            UpdatePCDFile();
        }
    }
    public Color GetColorAtWorldPosition(Vector3 worldPosition)
    {
        Vector2 uv = GetUVFromWorldPosition(worldPosition);
        int x = Mathf.FloorToInt(uv.x * cameraTexture.width);
        int y = Mathf.FloorToInt(uv.y * cameraTexture.height);

        return cameraTexture.GetPixel(x, y);
    }
    // 3D ����Ʈ Ŭ���� ��ġ�� 2D ī�޶� ��ǥ�� ��ȯ
    protected Vector2 GetUVFromWorldPosition(Vector3 worldPosition)
    {
        Vector3 screenPos = arCam.WorldToScreenPoint(worldPosition);
        return new Vector2(screenPos.x / Screen.width, screenPos.y / Screen.height);
    }
    protected void UpdatePCDFile()
    {
        if(pcdList.Count == listCnt)
        {
            SavePCDToBIN(); //���̳ʸ��� ������
            saveFileCnt += 1;//����ī��� 1����
            pcdList.Clear();//����Ʈ �ʱ�ȭ
            idx = 0;
        }
    }
    protected void SavePCDToBIN()
    {
        string fileName = string.Format("PCD{0}.bin", saveFileCnt);
        saveFilePath = Path.Combine(Application.persistentDataPath, fileName);
        using (BinaryWriter writer = new BinaryWriter(File.Open(saveFilePath, FileMode.Create)))
        {
            // ����Ʈ�� ������ ���� ����
            writer.Write(pcdList.Count);
            // �� ����Ʈ ��ǥ�� ����
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
            // Color ����Ʈ�� ������ ����
            writer.Write(colorList.Count);

            // �� Color�� r, g, b, a ���� ����
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
                // ����Ʈ�� ������ ���� ����
                writer.Write(pcdList.Count);
                // �� ����Ʈ ��ǥ�� ����
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
                // Color ����Ʈ�� ������ ����
                writer.Write(colorList.Count);

                // �� Color�� r, g, b, a ���� ����
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
                // ����Ʈ�� ������ ���� ����
                writer.Write(cameraPath.Count);

                // �� ����Ʈ ��ǥ�� ����
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

