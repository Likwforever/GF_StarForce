public class FollowManualControlRotate : BaseFollowShortState
{
    public override CameraFollowShortStateID ID => CameraFollowShortStateID.FollowManualControlRotateState;
    public FollowManualControlRotate(MainCameraFollowState owner) : base(owner)
    {

    }

    public void SetDragDelta(float dx, float dy)
    {
        float curVertical = this._owner.verticalAxisVal;
        float curHorizontal = this._owner.horizontalAxisVal;

        this._owner.SetAixsVal(curHorizontal + dx, curVertical + dy);
    }

}