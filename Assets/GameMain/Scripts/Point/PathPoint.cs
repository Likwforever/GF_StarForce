using UnityEngine;

namespace StarForce
{
    public class PathPoint : MonoBehaviour
    {

        [SerializeField]
        private int m_PointIndex = 0;

        [SerializeField]
        private float m_WaitTime = 0f;

        [SerializeField]
        private float m_MoveSpeed = 1f;



        public int PointIndex
        {
            get
            {
                return this.m_PointIndex;
            }
        }

        public Vector3 Position
        {
            get
            {
                return transform.position;
            }
        }

        public float WaitTime
        {
            get
            {
                return this.m_WaitTime;
            }
        }

        public float MoveSpeed
        {
            get
            {
                return this.m_MoveSpeed;
            }
        }


        public void Init(Vector3 position, int pointIndex, float waitTime, float moveSpeed)
        {
            this.m_PointIndex = pointIndex;
            this.m_WaitTime = waitTime;
            this.m_MoveSpeed = moveSpeed;

            transform.position = position;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        private void OnDrawGizmos()
        {
            // 在Scene视图中绘制路径点
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.5f);

            // 绘制路径点索引
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up, PointIndex.ToString());
#endif
        }

        private void OnDrawGizmosSelected()
        {
            // 选中时显示详细信息
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.7f);
        }

        public void Reset()
        {
            transform.position = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }
    }
}
