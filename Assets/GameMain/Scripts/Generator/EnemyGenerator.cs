using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace StarForce
{
    public class EnemyGenerator : BaseGenerator
    {
        private int m_EnemyTypeId = 60000;
        private List<int> m_EnemyIds = new List<int>();
        private int m_EnemyKilledCount = 0;
        private int m_EnemyReachedEndCount = 0;

        private float m_SpawnTimer = 0f;
        private float m_SpawnInterval = 1f;
        private int m_MaxEnemyCount = 10;

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

        public override void Initialize(params object[] args)
        {
            base.Initialize(args);
            this.m_MaxEnemyCount = (int)args[0];
        }

        public override void Update(float elapseSeconds)
        {
            base.Update(elapseSeconds);

            // 生成新怪物
            this.m_SpawnTimer += elapseSeconds;
            if (this.m_SpawnTimer >= this.m_SpawnInterval && this.EnemyCount < this.m_MaxEnemyCount)
            {
                this.SpawnEnemy();
                this.m_SpawnTimer = 0f;
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
            enemyData.Position = GameEntry.PathPoint.GetPathPointPosition(0);

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
            if (enemyId != 0 && !this.m_EnemyIds.Contains(enemyId))
            {
                this.m_EnemyIds.Add(enemyId);
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
                this.m_EnemyKilledCount++;
                Log.Info("Enemy {0} died. Total killed: {1}", enemy.Id, this.m_EnemyKilledCount);
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
                this.m_EnemyReachedEndCount++;
                Log.Info("Enemy {0} reached end. Total reached: {1}", enemy.Id, this.m_EnemyReachedEndCount);
            }
        }

        /// <summary>
        /// 清除所有怪物。
        /// </summary>
        public void ClearAllEnemies()
        {
            foreach (var enemyId in this.m_EnemyIds)
            {
                if (GameEntry.Entity.HasEntity(enemyId))
                {
                    GameEntry.Entity.HideEntity(GameEntry.Entity.GetEntity(enemyId));
                }
            }

            this.m_EnemyIds.Clear();
        }

        /// <summary>
        /// 重置统计数据。
        /// </summary>
        public void ResetStats()
        {
            m_EnemyKilledCount = 0;
            m_EnemyReachedEndCount = 0;
        }

        public void Shutdown()
        {
            this.ResetStats();
            this.ClearAllEnemies();
        }


        public override void Clear()
        {
            this.ResetStats();
            this.ClearAllEnemies();
        }
    }
}
