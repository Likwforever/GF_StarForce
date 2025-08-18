//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework;
using GameFramework.Event;
using UnityGameFramework.Runtime;

namespace StarForce
{
    public class TowerGame : GameBase
    {
        private int m_MaxEnemyCount = 10;

        private GeneratorManager _generatorMgr = null;

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

            GameEntry.Event.Subscribe(EnemyReachEndEvent.EventId, OnEnemyReachEnd);

            Log.Info("TowerGame initialized.");

            // 初始化生成器
            this._generatorMgr = ReferencePool.Acquire<GeneratorManager>();
            // 路径生成
            GameEntry.PathPoint.GenerateCircularPath();
            // 怪物生成
            this._generatorMgr.AddGenerator<EnemyGenerator>(this.m_MaxEnemyCount);
        }

        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
            base.Update(elapseSeconds, realElapseSeconds);

            this._generatorMgr.Update(elapseSeconds);
        }

        public override void Shutdown()
        {
            base.Shutdown();
        }


        private void OnEnemyReachEnd(object sender, GameEventArgs e)
        {
            GameOver = true;
        }
    }
}
