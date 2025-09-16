using StarForce;
using UnityEngine;

public class FollowManualControlRotateState : BaseFollowShortState
{
    private float _exitTime = 0;
    public override CameraFollowShortStateID ID => CameraFollowShortStateID.FollowManualControlRotateState;
    public override bool isSkippingBaseState => true;
    public FollowManualControlRotateState(MainCameraFollowState owner) : base(owner)
    {

    }

    public override void Enter(object param = null)
    {
        base.Enter();
        this._exitTime = Time.time + 2;
    }

    public override void Update()
    {
        base.Update();
        if (Time.time > this._exitTime)
        {
            this.End();
        }
    }

    public override void Exit()
    {
        base.Exit();
    }

    public void SetDragDelta(float dx, float dy)
    {
        float curVertical = this._owner.verticalAxisVal;
        float curHorizontal = this._owner.horizontalAxisVal;

        this._owner.SetAixsVal(curHorizontal + dx, curVertical + dy);
    }

}