using UnityEngine;
using UnityGameFramework.Runtime;

namespace StarForce
{
    public class MoveLogicComponent : UpdateLogicComponent
    {
        private float m_MoveSpeed = 2.0f;
        private float m_CurrentProgress = 0f;

        public override void OnAdd(object owner)
        {
            base.OnAdd(owner);
        }

        public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            this.MoveAlongPath(elapseSeconds);
        }

        /// <summary>
        /// 沿路径移动。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间。</param>
        private void MoveAlongPath(float elapseSeconds)
        {
            var pathPoint = GameEntry.PathPoint;
            if (pathPoint == null)
            {
                return;
            }

            // 更新路径进度
            m_CurrentProgress += this.m_MoveSpeed * elapseSeconds / pathPoint.TotalPathLength;

            // 循环路径
            if (m_CurrentProgress >= 1f)
            {
                m_CurrentProgress = 0f;
                // 可以在这里触发到达终点事件
                OnReachEnd();
            }

            // 更新位置
            Vector3 targetPosition = pathPoint.GetPositionByProgress(m_CurrentProgress);
            (this._owner as Enemy).CachedTransform.position = targetPosition;

            // 更新朝向
            if (m_CurrentProgress > 0f)
            {
                Vector3 previousPosition = pathPoint.GetPositionByProgress(m_CurrentProgress - 0.01f);
                Vector3 direction = (targetPosition - previousPosition).normalized;
                if (direction != Vector3.zero)
                {
                    (this._owner as Enemy).CachedTransform.rotation = Quaternion.LookRotation(direction);
                }
            }
        }

        /// <summary>
        /// 到达终点。
        /// </summary>
        private void OnReachEnd()
        {
            // 可以在这里触发到达终点事件
            // 比如对玩家造成伤害
            Log.Info("Enemy {0} reached the end of path.", (this._owner as Enemy).Id);

            GameEntry.Event.Fire(this, BaseEvent.Create<EnemyReachEndEvent>());

            // 隐藏实体
            GameEntry.Entity.HideEntity(this._owner as Entity);
        }
    }
}