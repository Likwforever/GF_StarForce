//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework;
using GameFramework.DataTable;
using GameFramework.Event;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace StarForce
{
    public class SurvivalGame : GameBase
    {
        private float m_ElapseSeconds = 0f;
        private MyAircraft m_MyAircraft = null;

        public override GameMode GameMode
        {
            get
            {
                return GameMode.Survival;
            }
        }

        public ScrollableBackground SceneBackground
        {
            get;
            private set;
        }


        public override void Initialize()
        {
            GameEntry.Event.Subscribe(ShowEntitySuccessEventArgs.EventId, OnShowEntitySuccess);
            GameEntry.Event.Subscribe(ShowEntityFailureEventArgs.EventId, OnShowEntityFailure);

            SceneBackground = Object.FindObjectOfType<ScrollableBackground>();
            if (SceneBackground == null)
            {
                Log.Warning("Can not find scene background.");
                return;
            }

            SceneBackground.VisibleBoundary.gameObject.GetOrAddComponent<HideByBoundary>();
            GameEntry.Entity.ShowMyAircraft(new MyAircraftData(GameEntry.Entity.GenerateSerialId(), 10000)
            {
                Name = "My Aircraft",
                Position = Vector3.zero,
            });

            GameOver = false;
            m_MyAircraft = null;
        }

        public override void Shutdown()
        {
            GameEntry.Event.Unsubscribe(ShowEntitySuccessEventArgs.EventId, OnShowEntitySuccess);
            GameEntry.Event.Unsubscribe(ShowEntityFailureEventArgs.EventId, OnShowEntityFailure);
        }

        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
            base.Update(elapseSeconds, realElapseSeconds);

            if (m_MyAircraft != null && m_MyAircraft.IsDead)
            {
                GameOver = true;
                return;
            }

            m_ElapseSeconds += elapseSeconds;
            if (m_ElapseSeconds >= 1f)
            {
                m_ElapseSeconds = 0f;
                this.GenerateAsteroid();
            }
        }

        private void GenerateAsteroid()
        {
            IDataTable<DRAsteroid> dtAsteroid = GameEntry.DataTable.GetDataTable<DRAsteroid>();
            float randomPositionX = SceneBackground.EnemySpawnBoundary.bounds.min.x + SceneBackground.EnemySpawnBoundary.bounds.size.x * (float)Utility.Random.GetRandomDouble();
            float randomPositionZ = SceneBackground.EnemySpawnBoundary.bounds.min.z + SceneBackground.EnemySpawnBoundary.bounds.size.z * (float)Utility.Random.GetRandomDouble();
            GameEntry.Entity.ShowAsteroid(new AsteroidData(GameEntry.Entity.GenerateSerialId(), 60000 + Utility.Random.GetRandom(dtAsteroid.Count))
            {
                Position = new Vector3(randomPositionX, 0f, randomPositionZ),
            });
        }

        protected virtual void OnShowEntitySuccess(object sender, GameEventArgs e)
        {
            ShowEntitySuccessEventArgs ne = (ShowEntitySuccessEventArgs)e;
            if (ne.EntityLogicType == typeof(MyAircraft))
            {
                m_MyAircraft = (MyAircraft)ne.Entity.Logic;
            }
        }

        protected virtual void OnShowEntityFailure(object sender, GameEventArgs e)
        {
            ShowEntityFailureEventArgs ne = (ShowEntityFailureEventArgs)e;
            Log.Warning("Show entity failure with error message '{0}'.", ne.ErrorMessage);
        }
    }
}
