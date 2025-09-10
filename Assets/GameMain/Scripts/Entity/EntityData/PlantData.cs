//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using GameFramework.DataTable;
using UnityEngine;

namespace StarForce
{
    [Serializable]
    public class PlantData : EntityData
    {
        [SerializeField]
        private float m_AttackDamage = 10f;

        [SerializeField]
        private float m_AttackSpeed = 1f;

        [SerializeField]
        private float m_AttackRange = 3f;

        [SerializeField]
        private int m_WeaponCount = 1;

        [SerializeField]
        private float m_RotateSpeed = 90f;

        [SerializeField]
        private int m_Cost = 100;

        [SerializeField]
        private int m_Level = 1;

        private int m_InitWeapon = 0;

        public PlantData(int entityId, int typeId, float attackDamage, float attackSpeed, float attackRange, int weaponCount, float rotateSpeed, int cost)
            : base(entityId, typeId)
        {
            m_AttackDamage = attackDamage;
            m_AttackSpeed = attackSpeed;
            m_AttackRange = attackRange;
            m_WeaponCount = weaponCount;
            m_RotateSpeed = rotateSpeed;
            m_Cost = cost;

            IDataTable<DRAircraft> dtAircraft = GameEntry.DataTable.GetDataTable<DRAircraft>();
            DRAircraft drAircraft = dtAircraft.GetDataRow(TypeId);
            if (drAircraft == null)
            {
                return;
            }

            m_InitWeapon = drAircraft.GetWeaponIdAt(0);
        }

        /// <summary>
        /// 攻击力。
        /// </summary>
        public float AttackDamage
        {
            get
            {
                return m_AttackDamage;
            }
            set
            {
                m_AttackDamage = value;
            }
        }

        /// <summary>
        /// 攻击速度。
        /// </summary>
        public float AttackSpeed
        {
            get
            {
                return m_AttackSpeed;
            }
        }

        /// <summary>
        /// 攻击范围。
        /// </summary>
        public float AttackRange
        {
            get
            {
                return m_AttackRange;
            }
        }

        /// <summary>
        /// 武器数量。
        /// </summary>
        public int WeaponCount
        {
            get
            {
                return m_WeaponCount;
            }
            set
            {
                m_WeaponCount = value;
            }
        }

        /// <summary>
        /// 旋转速度。
        /// </summary>
        public float RotateSpeed
        {
            get
            {
                return m_RotateSpeed;
            }
        }

        /// <summary>
        /// 种植成本。
        /// </summary>
        public int Cost
        {
            get
            {
                return m_Cost;
            }
        }

        /// <summary>
        /// 等级。
        /// </summary>
        public int Level
        {
            get
            {
                return m_Level;
            }
            set
            {
                m_Level = value;
            }
        }

        /// <summary>
        /// 初始武器。
        /// </summary>
        public int InitWeapon
        {
            get
            {
                return m_InitWeapon;
            }
        }
    }
}
