//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using UnityEngine;
using UnityGameFramework.Runtime;

namespace StarForce
{
    public partial class PointComponent : GameFrameworkComponent
    {

        private GameObject m_TestCube = null;

        [SerializeField]
        private float m_MoveSpeed = 2f;
        private float m_CurrentProgress = 0f;

        private void Start()
        {
            this.m_PathPointObjectPool = GameEntry.ObjectPool.CreateSingleSpawnObjectPool<PathPointObject>("PathPoint", this.m_PathPointPoolCapacity);
        }

        public void Test()
        {
            this.GenerateCircularPath();
            if (m_TestCube == null)
            {
                // 创建一个简单的Cube作为测试对象
                m_TestCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                m_TestCube.name = "TestCube";
                m_TestCube.transform.localScale = Vector3.one * 0.5f;

                // 设置材质颜色
                Renderer renderer = m_TestCube.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = Color.blue;
                }
            }

            // 将测试Cube放置在路径起点
            if (m_TestCube != null)
            {
                m_TestCube.transform.position = this.GetPositionByProgress(0f);
            }
        }

        private void Update()
        {
            if (m_TestCube == null)
            {
                return;
            }

            // 更新路径进度
            m_CurrentProgress += m_MoveSpeed * Time.deltaTime / this.TotalPathLength;

            // 循环路径
            if (m_CurrentProgress >= 1f)
            {
                m_CurrentProgress = 0f;
            }

            // 更新Cube位置
            Vector3 targetPosition = this.GetPositionByProgress(m_CurrentProgress);
            m_TestCube.transform.position = targetPosition;

            // 让Cube朝向移动方向
            if (m_CurrentProgress > 0f)
            {
                Vector3 previousPosition = this.GetPositionByProgress(m_CurrentProgress - 0.01f);
                Vector3 direction = (targetPosition - previousPosition).normalized;
                if (direction != Vector3.zero)
                {
                    m_TestCube.transform.rotation = Quaternion.LookRotation(direction);
                }
            }
        }

        private void OnGUI()
        {
            if (this != null)
            {
                GUI.Label(new Rect(10, 10, 300, 20), $"Path Progress: {m_CurrentProgress:F2}");
                GUI.Label(new Rect(10, 30, 300, 20), $"Total Length: {this.TotalPathLength:F2}");
                GUI.Label(new Rect(10, 50, 300, 20), $"Point Count: {this.PointCount}");
                GUI.Label(new Rect(10, 70, 300, 20), $"Move Speed: {m_MoveSpeed:F1}");

                if (GUI.Button(new Rect(10, 100, 100, 30), "Regenerate"))
                {
                    this.GenerateCircularPath();
                    m_CurrentProgress = 0f;
                }
            }
        }
    }
}
