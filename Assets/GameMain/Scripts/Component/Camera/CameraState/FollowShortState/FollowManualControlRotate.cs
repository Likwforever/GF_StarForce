using UnityEngine;

public class FollowManualControlRotateState : BaseFollowShortState
{
    public override CameraFollowShortStateID ID => CameraFollowShortStateID.FollowManualControlRotateState;
    public FollowManualControlRotateState(MainCameraFollowState owner) : base(owner)
    {

    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("FollowManualControlRotate Enter");
    }

    public override void Update()
    {
        base.Update();
    }

    public override void Exit()
    {
        base.Exit();
        Debug.Log("FollowManualControlRotate Exit");
    }

    public void SetDragDelta(float dx, float dy)
    {
        float curVertical = this._owner.verticalAxisVal;
        float curHorizontal = this._owner.horizontalAxisVal;

        this._owner.SetAixsVal(curHorizontal + dx, curVertical + dy);
    }

}