
using UnityEngine;
using UnityGameFramework.Runtime;

namespace StarForce
{
    public class PlantBullet : Bullet
    {
        private Vector3 m_InitPosition = Vector3.zero;

        public Vector3 InitPosition
        {
            get
            {
                return this.m_InitPosition;
            }
            set
            {
                this.m_InitPosition = value;
            }
        }

        protected override void OnShow(object userData)
        {
            base.OnShow(userData);
        }

        protected override void OnAttachTo(EntityLogic childEntity, UnityEngine.Transform parentTransform, object userData)
        {
            base.OnAttachTo(childEntity, parentTransform, userData);
            CachedTransform.localPosition = this.m_InitPosition;
            CachedTransform.localRotation = Quaternion.identity;
        }
    }
}