
using System;
using StarForce;
using UnityEngine;

public class FollowCloseShotState : BaseFollowBaseState
{

    private float _focalTime;

    private float _enterCamVerticalAxis;
    private float _enterCamHorizontalAxis;

    public override CameraFollowStateID ID => CameraFollowStateID.FollowCloseShotState;

    public override int Priority => 1;

    public FollowCloseShotState(MainCameraFollowState owner) : base(owner)
    {
    }

    public override void Enter()
    {
        base.Enter();
        this.CancelClosingParam();
        this._focalTime = CameraConfig.FocalTime;

        this._enterCamVerticalAxis = this._owner.verticalAxisVal;
        this._enterCamHorizontalAxis = this._owner.horizontalAxisVal;

        Vector3 avatarForward = this._owner.followDir;
        avatarForward *= -1;
        Quaternion targetRotation = Quaternion.LookRotation(avatarForward, Vector3.up);

        this._owner.SetMinFollowDistance(CameraConfig.FocalMinDist);
        this.DoLerpMove(targetRotation.eulerAngles.y, targetRotation.eulerAngles.x, this._owner.minDist);


        Debug.Log("FollowCloseShotState Enter");
        // 开启景深效果
        // GameEntry.Camera.CameraCom.depthOfField.enabled = true;
    }

    public override void Exit()
    {
        this._owner.SetMinFollowDistance(CameraConfig.MinCamDistance);

        this.CancelClosingParam();
        this.DoLerpMove(this._enterCamVerticalAxis, this._enterCamHorizontalAxis, CameraConfig.RecoverCloseShotDist);
        base.Exit();
        Debug.Log("FollowCloseShotState Exit");
    }


    private void DoLerpMove(float targetVerticalAxis, float targetHorizontalAxis, float targetDist)
    {
        float verticalOffset = Math.Abs(targetVerticalAxis - this._owner.verticalAxisVal);

        if (verticalOffset > 180)
        {
            if (this._owner.verticalAxisVal > targetVerticalAxis)
            {
                targetVerticalAxis += 360;
            }
            else
            {
                targetVerticalAxis -= 360;
            }
        }

        float speed = Math.Abs(targetVerticalAxis - this._owner.verticalAxisVal) / this._focalTime;

        this._owner.TryLerpVerticalAxisVal(targetVerticalAxis, 1 / speed, null, CameraLerpMode.SquareLerp);

        speed =
            Math.Abs(CameraUtil.NormalizeHorizontalAxis(targetHorizontalAxis) - CameraUtil.NormalizeHorizontalAxis(this._owner.horizontalAxisVal)) /
            this._focalTime;
        this._owner.TryLerpHorizontalAxisVal(targetHorizontalAxis, 1 / speed, null, CameraLerpMode.SquareLerp, true);

        speed = Math.Abs(this._owner.targetManualDist - targetDist) / this._focalTime;
        this._owner.TryLerpDistance(targetDist, 1 / speed, null, CameraLerpMode.SquareLerp);
    }

    private void CancelClosingParam()
    {
        this._owner.CancelHorizontalAxisLerp();
        this._owner.CancelVerticalAxisLerp();
        this._owner.CancelDistanceLerp();
    }
}