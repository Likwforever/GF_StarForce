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
    public class Enemy : TargetableObject
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


        private void OnDrawGizmos()
        {
            // 在Scene视图中绘制怪物
            Gizmos.color = m_IsDead ? Color.gray : Color.red;
            Gizmos.DrawWireCube(transform.position, Vector3.one);
        }

        public override ImpactData GetImpactData()
        {
            return new ImpactData(m_EnemyData.Camp, m_EnemyData.HP, 0, 0);
        }
    }
}
