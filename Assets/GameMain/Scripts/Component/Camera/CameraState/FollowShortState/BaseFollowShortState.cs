public abstract class BaseFollowShortState : CameraState<MainCameraFollowState>
{
    public abstract CameraFollowShortStateID ID { get; }


    public BaseFollowShortState(MainCameraFollowState owner) : base(owner)
    {
    }
}