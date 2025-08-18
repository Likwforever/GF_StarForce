using System.Collections.Generic;

namespace StarForce
{
    public class BuffLogicComponent : UpdateLogicComponent
    {
        private bool _needRecalculate = false; // 是否需要重新计算buff效果
        private List<int> _buffsToRemove = new List<int>(); // 需要移除的buff
        private Dictionary<int, BuffData> _activeBuffMap = new Dictionary<int, BuffData>(); // 当前生效的buff

        private Dictionary<BuffType, float> _buffModifierMap = new Dictionary<BuffType, float>(); // 缓存计算结果，避免重复计算

        public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            // 1. 更新所有buff的剩余时间
            foreach (var buff in this._activeBuffMap.Values)
            {
                buff.remainingTime -= elapseSeconds;

                // 2. 标记过期的buff
                if (buff.IsExpired())
                {
                    this._buffsToRemove.Add(buff.id);
                }
            }

            // 3. 移除过期的buff
            foreach (var buffId in this._buffsToRemove)
            {
                this.RemoveBuff(buffId);
            }
            this._buffsToRemove.Clear();


            // 4. 重新计算buff效果（如果需要）
            if (this._needRecalculate)
            {
                this.RecalculateBuffEffects();
            }
        }


        public void AddBuff(BuffData buffData)
        {
            if (this._activeBuffMap.ContainsKey(buffData.id))
            {
                // 已存在，增加层数
                var buff = this._activeBuffMap[buffData.id];
                int oldStack = buff.stack;
                buff.stack += buffData.stack;

                // 通知实体层数变化
                if (this._owner is IBuffable buffable)
                {
                    buffable.OnBuffStackChanged(buff, oldStack, buff.stack);
                }
            }
            else
            {
                // 新buff
                this._activeBuffMap.Add(buffData.id, buffData);

                // 通知实体应用buff
                if (this._owner is IBuffable buffable)
                {
                    buffable.OnBuffApplied(buffData);
                }
            }

            this._needRecalculate = true;
        }

        public void RemoveBuff(int buffId)
        {
            if (this._activeBuffMap.TryGetValue(buffId, out BuffData buffData))
            {
                this._activeBuffMap.Remove(buffId);

                // 通知实体移除buff
                if (this._owner is IBuffable buffable)
                {
                    buffable.OnBuffRemoved(buffData);
                }

                this._needRecalculate = true;
            }
        }

        /// <summary>
        /// 重新计算buff效果
        /// </summary>
        private void RecalculateBuffEffects()
        {
            if (!(this._owner is IBuffable buffable)) return;

            // 重置所有修改器
            this._buffModifierMap.Clear();

            // 计算所有buff的效果
            foreach (var buff in this._activeBuffMap.Values)
            {
                this.CalculateBuffEffect(buff, buffable);
            }

            // 应用最终效果
            this.ApplyFinalEffects(buffable);

            this._needRecalculate = false;
        }

        private void CalculateBuffEffect(BuffData buff, IBuffable target)
        {
            switch (buff.type)
            {
                case BuffType.SpeedUp:
                    this._buffModifierMap[BuffType.SpeedUp] = this._buffModifierMap.GetValueOrDefault(BuffType.SpeedUp, 1f) * (1f + buff.value * buff.stack);
                    break;
                case BuffType.DamageUp:
                    this._buffModifierMap[BuffType.DamageUp] = this._buffModifierMap.GetValueOrDefault(BuffType.DamageUp, 1f) * (1f + buff.value * buff.stack);
                    break;
                    // ... 其他类型
            }
        }

        private void ApplyFinalEffects(IBuffable target)
        {
            // 应用速度修改
            if (this._buffModifierMap.TryGetValue(BuffType.SpeedUp, out float speedModifier))
            {
                float finalSpeed = target.GetBaseSpeed() * speedModifier;
                target.SetFinalSpeed(finalSpeed);
            }
            else
            {
                target.SetFinalSpeed(target.GetBaseSpeed());
            }

            // 应用伤害修改
            if (this._buffModifierMap.TryGetValue(BuffType.DamageUp, out float damageModifier))
            {
                float finalDamage = target.GetBaseDamage() * damageModifier;
                target.SetFinalDamage(finalDamage);
            }
        }
    }
}