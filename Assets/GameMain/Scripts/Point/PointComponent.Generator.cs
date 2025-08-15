using System.Collections.Generic;
using GameFramework.ObjectPool;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace StarForce
{
    public partial class PointComponent
    {

        [SerializeField]
        private Vector3 m_CenterPosition = Vector3.zero;

        [SerializeField]
        private float m_Radius = 10f;

        [SerializeField]
        private int m_PointCount = 8;

        [SerializeField]
        private PathPoint m_PathPointTemplate = null;

        [SerializeField]
        private int m_PathPointPoolCapacity = 16;
        private IObjectPool<PathPointObject> m_PathPointObjectPool = null;
        private List<PathPoint> m_PathPoints = new List<PathPoint>();
        private List<Vector3> m_PathPositions = new List<Vector3>();

        public Vector3 CenterPosition
        {
            get
            {
                return m_CenterPosition;
            }
        }

        public float Radius
        {
            get
            {
                return m_Radius;
            }
        }

        public int PointCount
        {
            get
            {
                return m_PointCount;
            }
        }

        public List<Vector3> PathPositions
        {
            get
            {
                return m_PathPositions;
            }
        }

        public float TotalPathLength
        {
            get;
            private set;
        }

        public bool IsPathGenerated
        {
            get
            {
                return m_PathPositions.Count > 0;
            }
        }

        /// <summary>
        /// 生成圆形路径。
        /// </summary>
        public void GenerateCircularPath()
        {
            // 清除现有路径点
            ClearPathPoints();

            // 计算路径点位置
            float angleStep = 360f / m_PointCount;
            TotalPathLength = 0f;

            for (int i = 0; i < m_PointCount; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                Vector3 position = m_CenterPosition + new Vector3(
                    Mathf.Cos(angle) * m_Radius,
                    0f,
                    Mathf.Sin(angle) * m_Radius
                );

                m_PathPositions.Add(position);

                // 创建路径点
                this.CreatePathPoint(position, i, 0f, 1f);

                // 计算路径总长度
                if (i > 0)
                {
                    TotalPathLength += Vector3.Distance(m_PathPositions[i - 1], m_PathPositions[i]);
                }
            }

            // 连接最后一个点和第一个点
            TotalPathLength += Vector3.Distance(m_PathPositions[m_PointCount - 1], m_PathPositions[0]);

            Log.Info("Generated circular path with {0} points, total length: {1}", m_PointCount, TotalPathLength);
        }

        /// <summary>
        /// 清除所有路径点。
        /// </summary>
        public void ClearPathPoints()
        {
            foreach (var pathPoint in m_PathPoints)
            {
                if (pathPoint != null)
                {
                    pathPoint.Reset();
                    m_PathPointObjectPool.Unspawn(pathPoint);
                }
            }

            m_PathPoints.Clear();
            m_PathPositions.Clear();
            TotalPathLength = 0f;
        }

        /// <summary>
        /// 添加路径点到管理器。
        /// </summary>
        /// <param name="pathPoint">路径点实体。</param>
        public void AddPathPoint(PathPoint pathPoint)
        {
            if (pathPoint != null && !m_PathPoints.Contains(pathPoint))
            {
                m_PathPoints.Add(pathPoint);
                m_PathPositions.Add(pathPoint.Position);

                // 重新计算路径总长度
                CalculateTotalPathLength();
            }
        }

        /// <summary>
        /// 根据路径进度获取位置。
        /// </summary>
        /// <param name="progress">路径进度 (0-1)。</param>
        /// <returns>位置。</returns>
        public Vector3 GetPositionByProgress(float progress)
        {
            if (m_PathPositions.Count == 0)
            {
                return m_CenterPosition;
            }

            // 确保进度在有效范围内
            progress = Mathf.Clamp01(progress);

            // 计算总距离
            float targetDistance = progress * TotalPathLength;
            float currentDistance = 0f;

            // 遍历路径段找到目标位置
            for (int i = 0; i < m_PathPositions.Count; i++)
            {
                Vector3 startPos = m_PathPositions[i];
                Vector3 endPos = m_PathPositions[(i + 1) % m_PathPositions.Count];
                float segmentLength = Vector3.Distance(startPos, endPos);

                if (currentDistance + segmentLength >= targetDistance)
                {
                    // 在当前段内插值
                    float segmentProgress = (targetDistance - currentDistance) / segmentLength;
                    return Vector3.Lerp(startPos, endPos, segmentProgress);
                }

                currentDistance += segmentLength;
            }

            // 如果超出范围，返回最后一个点
            return m_PathPositions[m_PathPositions.Count - 1];
        }

        /// <summary>
        /// 获取路径点位置。
        /// </summary>
        /// <param name="index">路径点索引。</param>
        /// <returns>位置。</returns>
        public Vector3 GetPathPointPosition(int index)
        {
            if (index >= 0 && index < m_PathPositions.Count)
            {
                return m_PathPositions[index];
            }

            return m_CenterPosition;
        }

        /// <summary>
        /// 计算路径总长度。
        /// </summary>
        private void CalculateTotalPathLength()
        {
            TotalPathLength = 0f;
            for (int i = 0; i < m_PathPositions.Count; i++)
            {
                Vector3 startPos = m_PathPositions[i];
                Vector3 endPos = m_PathPositions[(i + 1) % m_PathPositions.Count];
                TotalPathLength += Vector3.Distance(startPos, endPos);
            }
        }

        private void OnDrawGizmos()
        {
            // 绘制圆形路径
            Gizmos.color = Color.green;
            for (int i = 0; i < m_PathPositions.Count; i++)
            {
                Vector3 startPos = m_PathPositions[i];
                Vector3 endPos = m_PathPositions[(i + 1) % m_PathPositions.Count];
                Gizmos.DrawLine(startPos, endPos);
            }

            // 绘制圆心
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(m_CenterPosition, 0.5f);
        }


        private void CreatePathPoint(Vector3 position, int pointIndex, float waitTime, float moveSpeed)
        {
            PathPoint pathPoint = null;
            PathPointObject pathPointObject = m_PathPointObjectPool.Spawn();
            if (pathPointObject != null)
            {
                pathPoint = (PathPoint)pathPointObject.Target;
            }
            else
            {
                pathPoint = Instantiate(m_PathPointTemplate);
                Transform transform = pathPoint.GetComponent<Transform>();
                transform.SetParent(this.transform);
                transform.localScale = Vector3.one;
                m_PathPointObjectPool.Register(PathPointObject.Create(pathPoint), true);
            }

            pathPoint.Init(position, pointIndex, waitTime, moveSpeed);
            m_PathPoints.Add(pathPoint);
        }
    }
}

