
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace StarForce
{
    public static class WeaponDefine
    {
        public static int currentBulletCount = 0;
        public static Dictionary<int, UnityAction<Weapon>> WeaponAttack = new Dictionary<int, UnityAction<Weapon>>()
        {
            { 30000, (weapon) =>
            {
                var weaponData = weapon.WeaponData;
                GameEntry.Entity.ShowBullet(new BulletData(GameEntry.Entity.GenerateSerialId(), weaponData.BulletId, weaponData.OwnerId, weaponData.OwnerCamp, weaponData.Attack, weaponData.BulletSpeed)
                {
                    Position = weapon.CachedTransform.position,
                });
                GameEntry.Sound.PlaySound(weaponData.BulletSoundId);
            } },

            { 30001, (weapon) =>
            {
                var weaponData = weapon.WeaponData;
                for(int i = 1; i <= 10; i++)
                {
                    GameEntry.Entity.ShowBullet(new BulletData(GameEntry.Entity.GenerateSerialId(), weaponData.BulletId, weaponData.OwnerId, weaponData.OwnerCamp, weaponData.Attack, weaponData.BulletSpeed)
                    {
                        Position = weapon.CachedTransform.position + GetInitPosition(i),
                    });

                }
                GameEntry.Sound.PlaySound(weaponData.BulletSoundId);
            } },
        };

        public static Vector3 GetInitPosition(int index)
        {

            float radius = 5f; // 距离原点5个单位
            float angleStep = 360f / 10; // 每把武器之间的角度间隔


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
    }
}