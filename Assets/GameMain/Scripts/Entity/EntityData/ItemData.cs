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
    public class ItemData : EntityData
    {
        [SerializeField]
        private int _duration = 0;

        [SerializeField]
        private ItemType _itemType = ItemType.None;

        public ItemData(int entityId, int typeId, int duration, ItemType itemType)
            : base(entityId, typeId)
        {
            this._duration = duration;
            this._itemType = itemType;
        }

        public int Duration
        {
            get => this._duration;
            set => this._duration = value;
        }

        public ItemType ItemType
        {
            get => this._itemType;
            set => this._itemType = value;
        }
    }

    public enum ItemType
    {
        None = 0,
        AddSpeed = 1,
    }
}
