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
    /// <summary>
    /// 道具类。
    /// </summary>
    public class Item : Entity
    {
        [SerializeField]
        private ItemData m_ItemData = null;

        [SerializeField]
        private float m_RotateSpeed = 30f;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
        }


        protected override void OnShow(object userData)
        {
            base.OnShow(userData);

            m_ItemData = userData as ItemData;
            if (m_ItemData == null)
            {
                Log.Error("Item data is invalid.");
                return;
            }
        }

        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            // 道具旋转效果
            CachedTransform.Rotate(Vector3.up, m_RotateSpeed * elapseSeconds, Space.World);
        }

        private void OnTriggerEnter(Collider other)
        {
            Entity entity = other.gameObject.GetComponent<Entity>();
            if (entity == null)
            {
                return;
            }

            // 只有玩家飞机可以拾取道具
            if (entity is MyAircraft)
            {
                OnPickup((MyAircraft)entity);
            }
        }

        private void OnPickup(MyAircraft player)
        {
            // 应用道具效果
            player.ApplyItemEffect(m_ItemData.ItemType, m_ItemData.Duration);

            // 隐藏道具
            GameEntry.Entity.HideEntity(this.Entity);
        }
    }
}
