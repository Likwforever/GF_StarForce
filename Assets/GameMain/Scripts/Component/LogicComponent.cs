using GameFramework;

namespace StarForce
{
    public class LogicComponent : IReference
    {
        protected object _owner = null;

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

        public virtual void Clear()
        {
            _owner = null;
        }


        public virtual void OnAdd(object owner)
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