//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System.Collections.Generic;
using GameFramework;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace StarForce
{
    public class PlantManagerComponent : GameFrameworkComponent
    {
        [SerializeField]
        private int m_PlantTypeId = 3001;

        [SerializeField]
        private int m_PlayerMoney = 1000;

        private List<Plant> m_Plants = new List<Plant>();

        public int PlayerMoney
        {
            get
            {
                return m_PlayerMoney;
            }
            set
            {
                m_PlayerMoney = value;
            }
        }

        public int PlantCount
        {
            get
            {
                return m_Plants.Count;
            }
        }

        /// <summary>
        /// 种植植物。
        /// </summary>
        /// <param name="position">种植位置。</param>
        /// <param name="plantType">植物类型。</param>
        /// <returns>是否种植成功。</returns>
        public bool PlantAt(Vector3 position, int plantType = 10000)
        {
            // 检查是否有足够的金钱
            int cost = GetPlantCost(plantType);
            if (m_PlayerMoney < cost)
            {
                Log.Warning("Not enough money to plant. Need {0}, have {1}.", cost, m_PlayerMoney);
                return false;
            }

            // 检查位置是否已被占用
            if (IsPositionOccupied(position))
            {
                Log.Warning("Position is already occupied.");
                return false;
            }

            // 创建植物数据
            PlantData plantData = new PlantData(
                GameEntry.Entity.GenerateSerialId(),
                plantType,
                10f,   // 攻击力
                1f,    // 攻击速度
                3f,    // 攻击范围
                1,     // 武器数量
                90f,   // 旋转速度
                cost   // 成本
            );

            plantData.Position = position;

            // 显示植物实体
            GameEntry.Entity.ShowPlant(plantData);

            // 扣除金钱
            m_PlayerMoney -= cost;

            Log.Info("Planted at position {0}. Remaining money: {1}", position, m_PlayerMoney);

            return true;
        }

        /// <summary>
        /// 添加植物到管理器。
        /// </summary>
        /// <param name="plant">植物实体。</param>
        public void AddPlant(Plant plant)
        {
            if (plant != null && !m_Plants.Contains(plant))
            {
                m_Plants.Add(plant);
            }
        }

        /// <summary>
        /// 移除植物。
        /// </summary>
        /// <param name="plant">植物实体。</param>
        public void RemovePlant(Plant plant)
        {
            if (plant != null && m_Plants.Contains(plant))
            {
                m_Plants.Remove(plant);
                GameEntry.Entity.HideEntity(plant);
            }
        }

        /// <summary>
        /// 检查位置是否被占用。
        /// </summary>
        /// <param name="position">位置。</param>
        /// <returns>是否被占用。</returns>
        private bool IsPositionOccupied(Vector3 position)
        {
            foreach (var plant in m_Plants)
            {
                if (plant != null)
                {
                    float distance = Vector3.Distance(position, plant.transform.position);
                    if (distance < 1f) // 1单位内的距离认为是占用
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 获取植物成本。
        /// </summary>
        /// <param name="plantType">植物类型。</param>
        /// <returns>成本。</returns>
        private int GetPlantCost(int plantType)
        {
            // 这里可以根据植物类型返回不同的成本
            switch (plantType)
            {
                case 3001: // 基础植物
                    return 100;
                case 3002: // 高级植物
                    return 200;
                default:
                    return 100;
            }
        }

        /// <summary>
        /// 清除所有植物。
        /// </summary>
        public void ClearAllPlants()
        {
            foreach (var plant in m_Plants)
            {
                if (plant != null)
                {
                    GameEntry.Entity.HideEntity(plant);
                }
            }

            m_Plants.Clear();
        }

        /// <summary>
        /// 添加金钱。
        /// </summary>
        /// <param name="amount">金额。</param>
        public void AddMoney(int amount)
        {
            m_PlayerMoney += amount;
            Log.Info("Added {0} money. Total: {1}", amount, m_PlayerMoney);
        }

        /// <summary>
        /// 消耗金钱。
        /// </summary>
        /// <param name="amount">金额。</param>
        /// <returns>是否成功。</returns>
        public bool SpendMoney(int amount)
        {
            if (m_PlayerMoney >= amount)
            {
                m_PlayerMoney -= amount;
                return true;
            }
            return false;
        }
    }
}
