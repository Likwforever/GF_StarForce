using GameFramework;
using UnityGameFramework.Runtime;

namespace StarForce
{
    public class LogicComponent : IReference
    {
        protected EntityLogic _owner = null;

        protected LogicComponentType _componentType;

        private bool _enable = true;

        public bool Enable
        {
            get => _enable;
            set
            {
                if (this._enable != value)
                {
                    this._enable = value;
                    if (this._enable)
                    {
                        OnEnable();
                    }
                    else
                    {
                        OnDisable();
                    }
                }
            }
        }

        public LogicComponentType componentType
        {
            get => _componentType;
        }

        public virtual void Clear()
        {
            _owner = null;
        }


        public virtual void OnAdd(EntityLogic owner)
        {
            this._owner = owner;
        }

        public virtual void OnStart()
        {
        }

        public virtual void OnRemove()
        {
        }

        public virtual void OnEnable()
        {

        }

        public virtual void OnDisable()
        {

        }
    }
}