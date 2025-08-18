using GameFramework.Event;

namespace StarForce
{
    public class EnemyReachEndEvent : BaseEvent
    {
        public static readonly int EventId = typeof(EnemyReachEndEvent).GetHashCode();
        public override int Id => EventId;

        public override void Clear()
        {

        }
    }
}