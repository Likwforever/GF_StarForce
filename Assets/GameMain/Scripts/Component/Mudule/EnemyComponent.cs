//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace StarForce
{
    public class EnemyComponent : GameFrameworkComponent
    {
        [SerializeField]
        private int m_EnemyTypeId = 60000;
        private List<int> m_EnemyIds = new List<int>();
        private int m_EnemyKilledCount = 0;
        private int m_EnemyReachedEndCount = 0;

        public int EnemyCount
        {
            get
            {
                return m_EnemyIds.Count;
            }
        }

        public int EnemyKilledCount
        {
            get
            {
                return m_EnemyKilledCount;
            }
        }

        public int EnemyReachedEndCount
        {
            get
            {
                return m_EnemyReachedEndCount;
            }
        }

        /// <summary>
        /// 生成怪物。
        /// </summary>
        public void SpawnEnemy()
        {
            // 创建怪物数据
            EnemyData enemyData = new EnemyData(
                GameEntry.Entity.GenerateSerialId(),
                m_EnemyTypeId,
                100f,  // 血量
                20f,    // 移动速度
                10f,   // 攻击力
                10     // 击杀奖励
            );

            // 设置初始位置为路径起点
            enemyData.Position = GameEntry.Point.GetPositionByProgress(0f);

            // 显示怪物实体
            GameEntry.Entity.ShowEnemy(enemyData);

            this.AddEnemy(enemyData.Id);

            Log.Info("Spawned enemy {0}.", enemyData.Id);
        }

        /// <summary>
        /// 添加怪物到管理器。
        /// </summary>
        /// <param name="enemy">怪物实体。</param>
        public void AddEnemy(int enemyId)
        {
            if (enemyId != 0 && !m_EnemyIds.Contains(enemyId))
            {
                m_EnemyIds.Add(enemyId);
            }
        }

        /// <summary>
        /// 怪物死亡回调。
        /// </summary>
        /// <param name="enemy">死亡的怪物。</param>
        public void OnEnemyDied(Enemy enemy)
        {
            if (enemy != null)
            {
                m_EnemyKilledCount++;
                Log.Info("Enemy {0} died. Total killed: {1}", enemy.Id, m_EnemyKilledCount);
            }
        }

        /// <summary>
        /// 怪物到达终点回调。
        /// </summary>
        /// <param name="enemy">到达终点的怪物。</param>
        public void OnEnemyReachedEnd(Enemy enemy)
        {
            if (enemy != null)
            {
                m_EnemyReachedEndCount++;
                Log.Info("Enemy {0} reached end. Total reached: {1}", enemy.Id, m_EnemyReachedEndCount);
            }
        }

        /// <summary>
        /// 清除所有怪物。
        /// </summary>
        public void ClearAllEnemies()
        {
            foreach (var enemyId in m_EnemyIds)
            {
                if (GameEntry.Entity.HasEntity(enemyId))
                {
                    GameEntry.Entity.HideEntity(GameEntry.Entity.GetEntity(enemyId));
                }
            }

            m_EnemyIds.Clear();
        }

        /// <summary>
        /// 重置统计数据。
        /// </summary>
        public void ResetStats()
        {
            m_EnemyKilledCount = 0;
            m_EnemyReachedEndCount = 0;
        }
    }
}
