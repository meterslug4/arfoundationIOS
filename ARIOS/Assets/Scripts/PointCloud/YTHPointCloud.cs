using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;
using System.IO;
public class YTHPointCloud : MonoBehaviour
{
    public ARPointCloudManager pointCloudManager;
    public Button pcdSaveBtn;
    protected List<Vector3> pointCloudPosList = new List<Vector3>();
    protected string saveFilePath;
    void Start()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, "PCD.bin");
        pointCloudManager.pointCloudsChanged += OnPointCloudChanged;
        pcdSaveBtn.onClick.AddListener(SavePCDToBIN);
    }
    protected void OnPointCloudChanged(ARPointCloudChangedEventArgs eventArgs)
    {
        foreach(ARPointCloud pointCloud in eventArgs.added)
        {
            if(pointCloud.positions.HasValue)
            {
                pointCloudPosList.AddRange(pointCloud.positions.Value);
            }
        }
        foreach (ARPointCloud pointCloud in eventArgs.updated)
        {
            if (pointCloud.positions.HasValue)
            {
                pointCloudPosList.AddRange(pointCloud.positions.Value);
            }
        }
    }
    protected void SavePCDToBIN()
    {
        using (BinaryWriter writer = new BinaryWriter(File.Open(saveFilePath, FileMode.Create)))
        {
            // 포인트의 개수를 먼저 저장
            writer.Write(pointCloudPosList.Count);

            // 각 포인트 좌표를 저장
            foreach (Vector3 point in pointCloudPosList)
            {
                writer.Write(point.x);
                writer.Write(point.y);
                writer.Write(point.z);
            }
        }
    }
    protected List<Vector3> LoadVector3ListFromBinary()
    {
        List<Vector3> list = new List<Vector3>();

        if (File.Exists(saveFilePath))
        {
            using (BinaryReader reader = new BinaryReader(File.Open(saveFilePath, FileMode.Open)))
            {
                // 저장된 리스트의 크기를 먼저 읽음
                int count = reader.ReadInt32();

                // 각 좌표를 읽어 리스트에 추가
                for (int i = 0; i < count; i++)
                {
                    float x = reader.ReadSingle();
                    float y = reader.ReadSingle();
                    float z = reader.ReadSingle();
                    list.Add(new Vector3(x, y, z));
                }
            }
        }
        else
        {
            Debug.LogWarning("No Vector3 list data file found!");
        }

        return list;
    }
}
