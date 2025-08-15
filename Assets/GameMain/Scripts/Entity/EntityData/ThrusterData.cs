//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework.DataTable;
using System;
using UnityEngine;

namespace StarForce
{
    [Serializable]
    public class ThrusterData : AccessoryObjectData
    {
        [SerializeField]
        private float m_Speed = 0f;

        [SerializeField]
        private float m_BaseSpeed = 0f;

        public ThrusterData(int entityId, int typeId, int ownerId, CampType ownerCamp)
            : base(entityId, typeId, ownerId, ownerCamp)
        {
            IDataTable<DRThruster> dtThruster = GameEntry.DataTable.GetDataTable<DRThruster>();
            DRThruster drThruster = dtThruster.GetDataRow(TypeId);
            if (drThruster == null)
            {
                return;
            }

            m_BaseSpeed = drThruster.Speed;
            m_Speed = m_BaseSpeed;
        }

        /// <summary>
        /// 当前速度。
        /// </summary>
        public float Speed
        {
            get
            {
                return m_Speed;
            }
            set
            {
                m_Speed = value;
            }
        }

        /// <summary>
        /// 基础速度。
        /// </summary>
        public float BaseSpeed
        {
            get
            {
                return m_BaseSpeed;
            }
        }
    }
}
