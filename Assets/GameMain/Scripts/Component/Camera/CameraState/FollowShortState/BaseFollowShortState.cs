public abstract class BaseFollowShortState : CameraState<MainCameraFollowState>
{
    public abstract CameraFollowShortStateID ID { get; }

    public abstract bool isSkippingBaseState { get; }

    public BaseFollowShortState(MainCameraFollowState owner) : base(owner)
    {
    }

    protected void End()
    {
        this._owner.TryRemoveShortState();
    }
}