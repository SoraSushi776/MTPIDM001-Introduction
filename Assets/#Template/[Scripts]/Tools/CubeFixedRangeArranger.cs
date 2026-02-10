using UnityEngine;

[ExecuteInEditMode]
public class CubeFixedRangeArranger : MonoBehaviour
{
    [Header("基础参数")]
    public float totalRange = 245f; // 总坐标范围（0到245）
    public int cubeCount = 48; // 方块总数
    public float spacing = 5f; // 间距
    public bool autoCalculateSize = true; // 自动计算方块尺寸

    [Header("手动设置（关闭自动计算时生效）")]
    public float cubeSize = 1f;

    [ContextMenu("排列方块")]
    public void ArrangeCubes()
    {
        // 清空原有方块
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        // 自动计算方块尺寸
        float finalCubeSize = cubeSize;
        if (autoCalculateSize)
        {
            if (cubeCount <= 0) cubeCount = 1; // 避免除以0
            // 核心公式计算尺寸
            finalCubeSize = (totalRange - (cubeCount - 1) * spacing) / cubeCount;
            // 防止尺寸为负数（比如间距设置过大）
            finalCubeSize = Mathf.Max(finalCubeSize, 0.01f);
        }

        // 排列方块（沿X轴，从0开始到245结束）
        for (int i = 0; i < cubeCount; i++)
        {
            // 计算当前方块的X坐标（方块左边缘位置 + 方块半宽，居中）
            float xPos = i * (finalCubeSize + spacing) + finalCubeSize / 2f;
            // Y、Z轴设为0（可按需调整）
            Vector3 pos = new Vector3(xPos, 0, 0);

            // 创建方块
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.parent = transform;
            cube.transform.position = pos;
            cube.transform.localScale = new Vector3(finalCubeSize, finalCubeSize, finalCubeSize); // 立方体，长宽高一致
            cube.name = $"Cube_{i+1}";
        }
    }

    private void OnValidate()
    {
        if (autoCalculateSize)
        {
            ArrangeCubes();
        }
    }
}