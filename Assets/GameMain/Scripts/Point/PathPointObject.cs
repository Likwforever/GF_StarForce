using GameFramework;
using GameFramework.ObjectPool;
using UnityEngine;

namespace StarForce
{
    public class PathPointObject : ObjectBase
    {
        public static PathPointObject Create(object target)
        {
            PathPointObject pathPointObject = ReferencePool.Acquire<PathPointObject>();
            pathPointObject.Initialize(target);
            return pathPointObject;
        }

        protected override void Release(bool isShutdown)
        {
            PathPoint pathPoint = (PathPoint)Target;
            if (pathPoint == null)
            {
                return;
            }

            Object.Destroy(pathPoint.gameObject);
        }
    }
}