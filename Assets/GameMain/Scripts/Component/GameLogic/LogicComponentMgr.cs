using System.Collections.Generic;
using GameFramework;
using UnityGameFramework.Runtime;

namespace StarForce
{
    public class LogicComponentMgr : IReference
    {
        private Entity _owner = null;
        private List<LogicComponent> _components = new List<LogicComponent>();
        private Dictionary<LogicComponentType, LogicComponent> _componentDict = new Dictionary<LogicComponentType, LogicComponent>();

        private List<UpdateLogicComponent> _updatableComponents = new List<UpdateLogicComponent>();

        public void Init(Entity owner)
        {
            this._owner = owner;
        }

        public void Clear()
        {
            this._owner = null;
            this.RemoveAllComponents();
        }

        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            foreach (var component in this._updatableComponents)
            {
                if (component.Enable)
                {
                    component.OnUpdate(elapseSeconds, realElapseSeconds);
                }
            }
        }

        public T GetComponent<T>(LogicComponentType componentType) where T : LogicComponent
        {
            if (this._componentDict.TryGetValue(componentType, out LogicComponent component))
            {
                return component as T;
            }
            return null;
        }

        public T AddComponent<T>() where T : LogicComponent, new()
        {
            T component = ReferencePool.Acquire<T>();
            LogicComponentType type = component.componentType;
            if (this._componentDict.TryGetValue(type, out LogicComponent oldComponent))
            {
                Log.Info($"ComponentManager: AddComponent Repeat: {type}");
                component.Clear();
                return null;
            }

            this._components.Add(component);
            this._componentDict[type] = component;
            component.OnAdd(this._owner);

            return component;
        }

        public void RemoveComponent(LogicComponentType type)
        {
            if (this._componentDict.TryGetValue(type, out LogicComponent component))
            {
                for (int i = 0; i < this._components.Count; ++i)
                {
                    if (this._components[i].componentType == type)
                    {
                        this._components.RemoveAt(i);
                        break;
                    }
                }
            }
            for (int i = 0; i < this._updatableComponents.Count; ++i)
            {
                if (this._updatableComponents[i].componentType == type)
                {
                    this._updatableComponents.RemoveAt(i);
                    break;
                }
            }
            this._componentDict.Remove(type);
            component.OnRemove();
            component.Clear();
            ReferencePool.Release(component);
        }


        public void RemoveAllComponents()
        {
            foreach (var component in this._components)
            {
                component.OnRemove();
                component.Clear();

                ReferencePool.Release(component);
            }

            this._components.Clear();
            this._componentDict.Clear();
            this._updatableComponents.Clear();
        }

        public void StartAllComponents()
        {
            for (int i = 0; i < this._components.Count; ++i)
            {
                LogicComponent component = this._components[i];
                if (component is UpdateLogicComponent)
                {
                    this._updatableComponents.Add(component as UpdateLogicComponent);
                }
                component.OnStart();
            }
        }
    }
}