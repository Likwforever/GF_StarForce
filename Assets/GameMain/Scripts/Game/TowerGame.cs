//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using UnityEngine;
using UnityGameFramework.Runtime;

namespace StarForce
{
    public class TowerGame : GameBase
    {

        private float m_SpawnTimer = 0f;
        private float m_SpawnInterval = 1f;
        private int m_MaxEnemyCount = 10;

        public override GameMode GameMode
        {
            get
            {
                return GameMode.Tower;
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            Log.Info("TowerGame initialized.");

            GameEntry.Point.GenerateCircularPath();
        }

        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
            base.Update(elapseSeconds, realElapseSeconds);

            // 生成新怪物
            m_SpawnTimer += Time.deltaTime;
            if (m_SpawnTimer >= m_SpawnInterval && GameEntry.Enemy.EnemyCount < m_MaxEnemyCount)
            {
                GameEntry.Enemy.SpawnEnemy();
                m_SpawnTimer = 0f;
            }
        }

        public override void Shutdown()
        {
            base.Shutdown();

            // 清理所有怪物
            if (GameEntry.Enemy != null)
            {
                GameEntry.Enemy.ClearAllEnemies();
            }
        }
    }
}
