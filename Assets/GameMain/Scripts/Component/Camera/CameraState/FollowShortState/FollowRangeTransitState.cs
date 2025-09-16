
//Range范围参数
using UnityEngine;

public class FollowRangeStateParamVo
{
    public float horizontalAxisVal; //绕X轴旋转值
    public float verticalAxisVal; //绕Y轴旋转值
    public float distance; //距离
    public float fadeTime; //过渡时间
};

public class FollowRangeTransitState : BaseFollowShortState
{
    private FollowRangeStateParamVo _param;

    public override CameraFollowShortStateID ID => CameraFollowShortStateID.FollowRangeTransitState;

    public override bool isSkippingBaseState => false;

    public FollowRangeTransitState(MainCameraFollowState owner) : base(owner)
    {

    }

    public override void Enter(object param = null)
    {
        base.Enter(param);
        this._param = param as FollowRangeStateParamVo;

        float fadeTime = this._param.fadeTime;

        // 镜头距离
        if (this._param.distance > 0)
        {
            if (this._param.distance > this._owner.maxDist)
            {
                this._owner.SetMaxFollowDistance(this._param.distance);
            }
            float lerpRatio = fadeTime / Mathf.Abs(this._owner.targetManualDist - this._param.distance);
            this._owner.TryLerpDistance(this._param.distance, lerpRatio);
        }

        // 镜头X轴旋转
        if (this._param.horizontalAxisVal >= -90)
        {
            float lerpRatio = fadeTime / Mathf.Abs(this._owner.horizontalAxisVal - this._param.horizontalAxisVal);
            this._owner.TryLerpHorizontalAxisVal(this._param.horizontalAxisVal, lerpRatio);
        }

        // 镜头Y轴旋转
        if (this._param.verticalAxisVal >= -180)
        {
            float curValue = CameraUtil.NormalizeVerticalAxis(this._owner.verticalAxisVal);
            float tarValue = CameraUtil.NormalizeVerticalAxis(this._param.verticalAxisVal);
            float deltaValue = Mathf.Abs(tarValue - curValue);
            if (deltaValue > 180)
            {
                if (tarValue > curValue)
                {
                    curValue += 360;
                }
                else
                {
                    tarValue += 360;
                }
            }
            float lerpRatio = fadeTime / Mathf.Abs(deltaValue);
            this._owner.TryLerpVerticalAxisVal(tarValue, lerpRatio);
        }
    }

    public override void Update()
    {
        base.Update();
        if (this._owner.IsParamChangeDone())
        {
            this.End();
        }

    }
}