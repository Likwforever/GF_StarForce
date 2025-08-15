//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace StarForce
{
    public class Enemy : Entity
    {
        private EnemyData m_EnemyData = null;
        private float m_CurrentProgress = 0f;
        private bool m_IsDead = false;

        public float Health
        {
            get
            {
                return m_EnemyData != null ? m_EnemyData.Health : 0f;
            }
        }

        public float MoveSpeed
        {
            get
            {
                return m_EnemyData != null ? m_EnemyData.MoveSpeed : 1f;
            }
        }

        public bool IsDead
        {
            get
            {
                return m_IsDead;
            }
        }

        public float PathProgress
        {
            get
            {
                return m_CurrentProgress;
            }
        }

#if UNITY_2017_3_OR_NEWER
        protected override void OnShow(object userData)
#else
        protected internal override void OnShow(object userData)
#endif
        {
            base.OnShow(userData);

            m_EnemyData = userData as EnemyData;
            if (m_EnemyData == null)
            {
                Log.Error("Enemy data is invalid.");
                return;
            }

            Name = Utility.Text.Format("[Enemy {0}]", Id);
            CachedTransform.localPosition = m_EnemyData.Position;
            CachedTransform.localRotation = Quaternion.identity;
            CachedTransform.localScale = Vector3.one;

            m_CurrentProgress = m_EnemyData.PathProgress;
            m_IsDead = false;
        }

#if UNITY_2017_3_OR_NEWER
        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
#else
        protected internal override void OnUpdate(float elapseSeconds, float realElapseSeconds)
#endif
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            if (m_IsDead)
            {
                return;
            }

            // 沿路径移动
            MoveAlongPath(elapseSeconds);
        }

        /// <summary>
        /// 沿路径移动。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间。</param>
        private void MoveAlongPath(float elapseSeconds)
        {
            // 获取路径管理器
            var pathManager = GameEntry.Point;
            if (pathManager == null)
            {
                return;
            }

            // 更新路径进度
            m_CurrentProgress += MoveSpeed * elapseSeconds / pathManager.TotalPathLength;

            // 循环路径
            if (m_CurrentProgress >= 1f)
            {
                m_CurrentProgress = 0f;
                // 可以在这里触发到达终点事件
                OnReachEnd();
            }

            // 更新位置
            Vector3 targetPosition = pathManager.GetPositionByProgress(m_CurrentProgress);
            CachedTransform.position = targetPosition;

            // 更新朝向
            if (m_CurrentProgress > 0f)
            {
                Vector3 previousPosition = pathManager.GetPositionByProgress(m_CurrentProgress - 0.01f);
                Vector3 direction = (targetPosition - previousPosition).normalized;
                if (direction != Vector3.zero)
                {
                    CachedTransform.rotation = Quaternion.LookRotation(direction);
                }
            }
        }

        /// <summary>
        /// 受到伤害。
        /// </summary>
        /// <param name="damage">伤害值。</param>
        public void TakeDamage(float damage)
        {
            if (m_IsDead || m_EnemyData == null)
            {
                return;
            }

            m_EnemyData.Health -= damage;

            if (m_EnemyData.Health <= 0f)
            {
                Die();
            }
        }

        /// <summary>
        /// 死亡。
        /// </summary>
        private void Die()
        {
            if (m_IsDead)
            {
                return;
            }

            m_IsDead = true;

            // 通知EnemyComponent怪物死亡
            GameEntry.Enemy.OnEnemyDied(this);

            // 隐藏实体
            GameEntry.Entity.HideEntity(this);
        }

        /// <summary>
        /// 到达终点。
        /// </summary>
        private void OnReachEnd()
        {
            // 可以在这里触发到达终点事件
            // 比如对玩家造成伤害
            Log.Info("Enemy {0} reached the end of path.", Id);

            GameEntry.Enemy.OnEnemyReachedEnd(this);

            // 隐藏实体
            GameEntry.Entity.HideEntity(this);
        }

        private void OnDrawGizmos()
        {
            // 在Scene视图中绘制怪物
            Gizmos.color = m_IsDead ? Color.gray : Color.red;
            Gizmos.DrawWireCube(transform.position, Vector3.one);
        }
    }
}
