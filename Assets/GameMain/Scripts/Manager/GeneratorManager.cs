using System.Collections.Generic;
using GameFramework;

namespace StarForce
{
    public class GeneratorManager : IReference
    {
        private List<BaseGenerator> m_Generators = new List<BaseGenerator>();

        public T AddGenerator<T>(params object[] args) where T : BaseGenerator, new()
        {
            T generator = ReferencePool.Acquire<T>();
            generator.Initialize(args);
            this.m_Generators.Add(generator);

            return generator;
        }

        public void Clear()
        {
            foreach (var generator in this.m_Generators)
            {
                generator.Clear();
            }
            this.m_Generators.Clear();
        }

        public void Update(float elapseSeconds)
        {
            foreach (var generator in this.m_Generators)
            {
                generator.Update(elapseSeconds);
            }
        }
    }
}