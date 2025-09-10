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
    public class Plant : Entity
    {
        private PlantData m_PlantData = null;
        private LogicComponentMgr m_LogicComponentMgr = null;
        private PlantAttackLogicComponent m_PlantAttack = null;

        public float AttackDamage
        {
            get
            {
                return m_PlantData != null ? m_PlantData.AttackDamage : 0f;
            }
        }

        public float AttackSpeed
        {
            get
            {
                return m_PlantData != null ? m_PlantData.AttackSpeed : 1f;
            }
        }

        public float AttackRange
        {
            get
            {
                return m_PlantData != null ? m_PlantData.AttackRange : 3f;
            }
        }

        public int WeaponCount
        {
            get
            {
                return m_PlantData != null ? m_PlantData.WeaponCount : 1;
            }
        }

        public float RotateSpeed
        {
            get
            {
                return m_PlantData != null ? m_PlantData.RotateSpeed : 90f;
            }
        }

        public int Level
        {
            get
            {
                return m_PlantData != null ? m_PlantData.Level : 1;
            }
        }

        public int InitWeapon
        {
            get
            {
                return m_PlantData != null ? m_PlantData.InitWeapon : 0;
            }
        }

        protected override void OnShow(object userData)
        {
            base.OnShow(userData);

            m_PlantData = userData as PlantData;
            if (m_PlantData == null)
            {
                Log.Error("Plant data is invalid.");
                return;
            }

            Name = Utility.Text.Format("[Plant {0}]", Id);
            CachedTransform.localPosition = m_PlantData.Position;
            CachedTransform.localRotation = Quaternion.identity;
            CachedTransform.localScale = Vector3.one;

            // 初始化逻辑组件管理器
            this.m_LogicComponentMgr = ReferencePool.Acquire<LogicComponentMgr>();
            this.m_LogicComponentMgr.Init(this);

            // 添加攻击逻辑组件
            this.m_PlantAttack = this.m_LogicComponentMgr.AddComponent<PlantAttackLogicComponent>();

            // 启动所有组件
            this.m_LogicComponentMgr.StartAllComponents();
        }

        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            this.m_LogicComponentMgr.Update(elapseSeconds, realElapseSeconds);
        }

        protected override void OnHide(bool isShutdown, object userData)
        {
            base.OnHide(isShutdown, userData);

            if (this.m_LogicComponentMgr != null)
            {
                this.m_LogicComponentMgr.Clear();
                ReferencePool.Release(this.m_LogicComponentMgr);
                this.m_LogicComponentMgr = null;
            }
        }

        protected override void OnAttached(EntityLogic childEntity, Transform parentTransform, object userData)
        {
            base.OnAttached(childEntity, parentTransform, userData);
            if (childEntity is Weapon)
            {
                this.m_PlantAttack.AddWeapon(childEntity as Weapon);
            }
        }

        /// <summary>
        /// 升级植物。
        /// </summary>
        public void Upgrade()
        {
            if (m_PlantData != null)
            {
                m_PlantData.Level++;
                m_PlantData.WeaponCount++;
                m_PlantData.AttackDamage *= 1.5f;

                Log.Info("Plant {0} upgraded to level {1}.", Id, m_PlantData.Level);
            }
        }

        private void OnDrawGizmos()
        {
            // 在Scene视图中绘制植物
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, Vector3.one);

            // 绘制攻击范围
            if (m_PlantData != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, m_PlantData.AttackRange);
            }
        }
    }
}
