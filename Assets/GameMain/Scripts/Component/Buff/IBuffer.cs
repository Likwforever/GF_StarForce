namespace StarForce
{
    public interface IBuffable
    {
        void OnBuffApplied(BuffData buff); // buff应用
        void OnBuffRemoved(BuffData buff); // buff移除
        void OnBuffStackChanged(BuffData buff, int oldStack, int newStack); // buff层数变化

        float GetBaseSpeed();
        float GetBaseDamage();
        float GetBaseDefense();

        // 设置最终属性值
        void SetFinalSpeed(float speed);
        void SetFinalDamage(float damage);
        void SetFinalDefense(float defense);
    }
}