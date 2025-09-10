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
    /// <summary>
    /// 武器类。
    /// </summary>
    public class Weapon : Entity
    {
        private string[] m_AttachPoints = new string[] { "Weapon_Point1", "Weapon_Point2" };

        private float m_NextAttackTime = 0f;

        [SerializeField]
        private WeaponData m_WeaponData = null;

        public WeaponData WeaponData
        {
            get
            {
                return m_WeaponData;
            }
        }

        public int BulletCount = 0;

#if UNITY_2017_3_OR_NEWER
        protected override void OnInit(object userData)
#else
        protected internal override void OnInit(object userData)
#endif
        {
            base.OnInit(userData);
        }

#if UNITY_2017_3_OR_NEWER
        protected override void OnShow(object userData)
#else
        protected internal override void OnShow(object userData)
#endif
        {
            base.OnShow(userData);

            m_WeaponData = userData as WeaponData;
            if (m_WeaponData == null)
            {
                Log.Error("Weapon data is invalid.");
                return;
            }


            GameEntry.Entity.AttachEntity(Entity, m_WeaponData.OwnerId, this.m_AttachPoints[this.m_WeaponData.AttachPointIndex]);
        }

#if UNITY_2017_3_OR_NEWER
        protected override void OnAttachTo(EntityLogic parentEntity, Transform parentTransform, object userData)
#else
        protected internal override void OnAttachTo(EntityLogic parentEntity, Transform parentTransform, object userData)
#endif
        {
            base.OnAttachTo(parentEntity, parentTransform, userData);

            Name = Utility.Text.Format("Weapon of {0}", parentEntity.Name);
            CachedTransform.localPosition = Vector3.zero;
        }

        public void TryAttack()
        {
            if (Time.time < m_NextAttackTime)
            {
                return;
            }

            BulletCount++;
            m_NextAttackTime = Time.time + m_WeaponData.AttackInterval;
            WeaponDefine.WeaponAttack[m_WeaponData.TypeId](this);
        }
    }
}
