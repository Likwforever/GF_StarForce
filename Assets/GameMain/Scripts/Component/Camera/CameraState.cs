public abstract class CameraState<T>
{

    protected T _owner;

    private bool _active = false;
    public bool active
    {
        get
        {
            return this._active;
        }
    }

    private bool _valid = false;
    public bool valid
    {
        get
        {
            return this._valid;
        }
    }

    public CameraState(T owner)
    {
        this._owner = owner;
    }


    public virtual void Enter()
    {
        this._active = true;
    }

    public virtual void Update()
    {

    }

    public virtual void Exit()
    {
        this._active = false;
    }

    public void SetActive(bool value)
    {
        this._active = value;
    }

    public void SetValid(bool value)
    {
        this._valid = value;
    }
}


