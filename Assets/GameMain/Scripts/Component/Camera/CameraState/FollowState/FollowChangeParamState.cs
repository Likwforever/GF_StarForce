using System;
using UnityEngine.Events;

public class FollowChangeParamState : BaseFollowBaseState
{
    private CameraParameter _cameraDist;
    private CameraParameter _cameraVerticalAxis;
    private CameraParameter _cameraHorizontalAxis;

    public FollowChangeParamState(MainCameraFollowState owner) : base(owner)
    {
        this._cameraDist = new CameraParameter();
        this._cameraVerticalAxis = new CameraParameter();
        this._cameraHorizontalAxis = new CameraParameter();
    }

    public override CameraFollowStateID ID => CameraFollowStateID.FollowChangeParamState;

    public override int Priority => 0;

    public override bool cannotBeSkipped => false;

    public override void Update()
    {
        if (this._cameraVerticalAxis.needUpdate)
        {
            this._cameraVerticalAxis.LerpStep();
            this._owner.SetVerticalAxisVal(this._cameraVerticalAxis.curValue, this._cameraVerticalAxis.isForce);
        }
        if (this._cameraHorizontalAxis.needUpdate)
        {
            this._cameraHorizontalAxis.LerpStep();
            this._owner.SetHorizontalAxisVal(this._cameraHorizontalAxis.curValue, this._cameraHorizontalAxis.isForce);
        }

        if (this._cameraDist.needUpdate)
        {
            this._cameraDist.LerpStep();
            this._owner.ChangeFollowDistance(this._cameraDist.curValue);
        }

        if (this._cameraVerticalAxis.isDone && this._cameraHorizontalAxis.isDone && this._cameraDist.isDone)
        {
            this.Exit();
        }
    }

    public override void Exit()
    {
        base.Exit();

        this.CancelAllParamChange();
    }

    public void TryChangeVerticalAxisVal(float targetVerticalAxis, float durationRatio, UnityAction endCallback, CameraLerpMode lerpMode, bool isForce = false, bool clearpreviouserp = false)
    {
        float startVal = this._owner.verticalAxisVal;
        if (!this._cameraVerticalAxis.isDone && !clearpreviouserp)
        {
            startVal = this._cameraVerticalAxis.GetLerpProcessByTargetVal(targetVerticalAxis);
        }

        startVal = CameraUtil.NormalizeVerticalAxis(startVal);
        this._cameraVerticalAxis.SetupLerp(startVal, targetVerticalAxis, durationRatio, endCallback, lerpMode, isForce);
        if (this._cameraVerticalAxis.needUpdate)
        {
            this.SetActive(true);
        }
    }

    public void TryChangeHorizontalAxisVal(float targetHorizontalAxis, float durationRatio, UnityAction endCallback, CameraLerpMode lerpMode, bool isForce = false, bool clearpreviouserp = false)
    {
        float startVal = this._owner.horizontalAxisVal;
        if (!this._cameraHorizontalAxis.isDone && !clearpreviouserp)
        {
            startVal = this._cameraHorizontalAxis.GetLerpProcessByTargetVal(targetHorizontalAxis);
        }

        startVal = CameraUtil.NormalizeHorizontalAxis(startVal);
        this._cameraHorizontalAxis.SetupLerp(startVal, targetHorizontalAxis, durationRatio, endCallback, lerpMode, isForce);
        if (this._cameraHorizontalAxis.needUpdate)
        {
            this.SetActive(true);
        }
    }

    public void TryChangeDistance(float targetDist, float durationRatio, UnityAction endCallback, CameraLerpMode lerpMode)
    {
        this._cameraDist.SetupLerp(this._owner.targetManualDist, targetDist, durationRatio, endCallback, lerpMode);
        if (this._cameraDist.needUpdate)
        {
            this.Enter();
        }
    }

    public void CancelAllParamChange()
    {
        this._cameraDist.needUpdate = false;
        this._cameraVerticalAxis.needUpdate = false;
        this._cameraHorizontalAxis.needUpdate = false;
    }

    public void CancelVerticalAxisLerp()
    {
        this._cameraVerticalAxis.needUpdate = false;
    }

    public void CancelHorizontalAxisLerp()
    {
        this._cameraHorizontalAxis.needUpdate = false;
    }

    public void CancelDistanceLerp()
    {
        this._cameraDist.needUpdate = false;
    }
}