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
        private bool m_IsDead = false;
        private EnemyData m_EnemyData = null;

        private LogicComponentMgr m_LogicComponentMgr = null;

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

        protected override void OnShow(object userData)
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

            m_IsDead = false;

            this.m_LogicComponentMgr = ReferencePool.Acquire<LogicComponentMgr>();
            this.m_LogicComponentMgr.Init(this);
            this.m_LogicComponentMgr.AddComponent<MoveLogicComponent>();
            this.m_LogicComponentMgr.StartAllComponents();
        }

        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            this.m_LogicComponentMgr.Update(elapseSeconds, realElapseSeconds);
            if (m_IsDead)
            {
                this.m_LogicComponentMgr.Clear();
                return;
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
