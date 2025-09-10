using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace StarForce
{
    public static class BulletDefine
    {
        public static Dictionary<int, UnityAction<Bullet, float>> BulletAttack = new Dictionary<int, UnityAction<Bullet, float>>()
        {
            { 50000, (bullet, elapseSeconds) => {
                var bulletData = bullet.BulletData;
                bullet.CachedTransform.Translate(Vector3.forward * bulletData.Speed * elapseSeconds, Space.World);
            } },
            { 50002, (bullet, elapseSeconds) => {
            var bulletData = bullet.BulletData;
                bullet.CachedTransform.Translate(Vector3.forward * bulletData.Speed * elapseSeconds, Space.World);
            } },
        };
    }
}