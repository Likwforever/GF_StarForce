using GameFramework;

namespace StarForce
{
    public class BuffData : IReference
    {
        public int id { get; set; }
        public int stack { get; set; }
        public float value { get; set; }
        public BuffType type { get; set; }

        public float remainingTime { get; set; }

        public bool IsExpired()
        {
            return this.remainingTime <= 0;
        }


        public void Reset()
        {
            this.id = 0;
            this.type = BuffType.None;
            this.stack = 0;
            this.value = 0;
            this.remainingTime = 0;
        }

        public void Clear()
        {
            this.Reset();
        }
    }
}