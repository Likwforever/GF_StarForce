using UnityEngine;
using UnityGameFramework.Runtime;

namespace StarForce
{
    public class ItemGenerator : BaseGenerator
    {

        // 生成道具的间隔时间
        private float _generateInterval = 10f;

        // 下次生成道具的时间
        private float _nextGenerateTime = 0f;

        private BoxCollider m_PlayerMoveBoundary = null;

        public void GenerateItem()
        {
            // 在随机位置生成双发子弹道具
            Vector3 randomPosition = new Vector3(
                Random.Range(m_PlayerMoveBoundary.bounds.min.x, m_PlayerMoveBoundary.bounds.max.x),
                0f,
                Random.Range(m_PlayerMoveBoundary.bounds.min.z, m_PlayerMoveBoundary.bounds.max.z)
            );

            Log.Info("生成道具在位置: {0}", randomPosition);

            GameEntry.Entity.ShowItem(new ItemData(
                GameEntry.Entity.GenerateSerialId(),
                80000, // 道具实体ID
                3,
                ItemType.AddSpeed
            )
            {
                Position = randomPosition,
            });
        }

        public override void Initialize(params object[] args)
        {
            this.m_PlayerMoveBoundary = (BoxCollider)args[0];
        }

        public override void Update(float elapseSeconds)
        {
            if (Time.time >= this._nextGenerateTime)
            {
                this.GenerateItem();
                this._nextGenerateTime = Time.time + this._generateInterval;
            }
        }

        public override void Clear()
        {
            this.m_PlayerMoveBoundary = null;
        }
    }

}