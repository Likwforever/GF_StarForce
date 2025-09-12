public abstract class BaseFollowBaseState : CameraState<MainCameraFollowState>
{

    public abstract CameraFollowStateID ID { get; }

    public abstract int Priority { get; }

    public abstract bool cannotBeSkipped { get; }

    protected BaseFollowBaseState(MainCameraFollowState owner) : base(owner)
    {
    }
}