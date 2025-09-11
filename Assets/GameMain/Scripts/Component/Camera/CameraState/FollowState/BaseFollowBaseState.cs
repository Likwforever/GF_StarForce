public abstract class BaseFollowBaseState : CameraState<MainCameraFollowState>
{

    public abstract CameraStateID ID { get; }

    public abstract int Priority { get; }

    protected BaseFollowBaseState(MainCameraFollowState owner) : base(owner)
    {
    }
}