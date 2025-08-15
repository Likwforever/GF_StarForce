//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using UnityEngine;

namespace StarForce
{
    [Serializable]
    public class EnemyData : EntityData
    {
        [SerializeField]
        private float m_Health = 100f;

        [SerializeField]
        private float m_MoveSpeed = 2f;

        [SerializeField]
        private float m_Damage = 10f;

        [SerializeField]
        private int m_Reward = 10;

        [SerializeField]
        private float m_PathProgress = 0f;

        public EnemyData(int entityId, int typeId, float health, float moveSpeed, float damage, int reward)
            : base(entityId, typeId)
        {
            m_Health = health;
            m_MoveSpeed = moveSpeed;
            m_Damage = damage;
            m_Reward = reward;
        }

        /// <summary>
        /// 怪物血量。
        /// </summary>
        public float Health
        {
            get
            {
                return m_Health;
            }
            set
            {
                m_Health = value;
            }
        }

        /// <summary>
        /// 移动速度。
        /// </summary>
        public float MoveSpeed
        {
            get
            {
                return m_MoveSpeed;
            }
        }

        /// <summary>
        /// 攻击力。
        /// </summary>
        public float Damage
        {
            get
            {
                return m_Damage;
            }
        }

        /// <summary>
        /// 击杀奖励。
        /// </summary>
        public int Reward
        {
            get
            {
                return m_Reward;
            }
        }

        /// <summary>
        /// 路径进度。
        /// </summary>
        public float PathProgress
        {
            get
            {
                return m_PathProgress;
            }
            set
            {
                m_PathProgress = value;
            }
        }
    }
}
