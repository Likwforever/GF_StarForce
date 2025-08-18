using GameFramework;
using GameFramework.Event;

namespace StarForce
{
    public abstract class BaseEvent : GameEventArgs
    {
        public static T Create<T>() where T : BaseEvent, new()
        {
            T instance = ReferencePool.Acquire<T>();
            return instance;
        }

        public override void Clear()
        {
            ReferencePool.Release(this);
        }
    }
}