using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarForce
{
    public class PlantAttackLogicComponent : UpdateLogicComponent
    {
        private Plant m_Plant = null;
        private List<Weapon> m_Weapons = new List<Weapon>();
        private Transform m_WeaponPoint = null;
        private int m_CurrentWeaponIndex = 0;

        public override void OnStart()
        {
            base.OnStart();
            m_Plant = this._owner as Plant;
            this.m_WeaponPoint = m_Plant.CachedTransform.Find("Armor Point");

            this.InitWeapons();
        }

        public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            if (m_Plant == null)
            {
                return;
            }

            this.WeaponRotate(elapseSeconds);
        }

        public override void Clear()
        {
            base.Clear();
            m_Plant = null;
        }

        private void InitWeapons()
        {
            m_Weapons.Clear();
            m_CurrentWeaponIndex = 0;

            GameEntry.Entity.ShowPlantWeapon(new WeaponData(GameEntry.Entity.GenerateSerialId(), m_Plant.InitWeapon, m_Plant.Id, CampType.Player));
        }

        public void AddWeapon(Weapon plantWeapon)
        {
            m_Weapons.Add(plantWeapon);
        }

        /// <summary>
        /// 初始化武器位置
        /// 以植物为圆点，武器围绕植物旋转，武器间隔平均
        /// 1把武器：12点钟方向
        /// 2把武器：12点和6点钟方向
        /// 3把武器：12点、3点、9点钟方向
        /// 以此类推...
        /// </summary>
        public Vector3 GetWeaponInitPosition(int index)
        {
            if (m_Weapons.Count == 0)
                return Vector3.zero;

            float radius = 5f; // 距离原点5个单位
            float angleStep = 360f / m_Plant.WeaponCount; // 每把武器之间的角度间隔


            // 计算当前武器的角度（从12点钟方向开始，顺时针）
            float angle = index * angleStep;

            // 将角度转换为弧度
            float angleRad = angle * Mathf.Deg2Rad;

            // 计算武器在圆周上的位置
            // Unity坐标系：Z轴向前（12点钟），X轴向右（3点钟），Y轴向上
            float x = Mathf.Sin(angleRad) * radius;
            float z = Mathf.Cos(angleRad) * radius;

            return new Vector3(x, 0, z);

        }

        /// <summary>
        /// 武器旋转
        /// 让武器节点一直绕着y轴旋转
        /// </summary>
        private void WeaponRotate(float elapseSeconds)
        {
            this.m_WeaponPoint.localRotation = Quaternion.Euler(0, m_WeaponPoint.localRotation.eulerAngles.y + 30 * elapseSeconds, 0);
        }
    }
}
