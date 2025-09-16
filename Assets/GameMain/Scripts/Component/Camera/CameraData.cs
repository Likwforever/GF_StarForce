using System;
using UnityEngine;
using UnityEngine.Events;

public class CameraParameter
{
    //计时器
    protected float _timer;
    //速度因子
    protected float _durationRatio;
    //recover持续时长
    protected float _duration;
    //插值模式
    protected CameraLerpMode _lerpMode;
    //是否无视Vertical、Horizontal Lock的限制
    protected bool _isForce;
    public bool isForce => this._isForce;
    //记录开始值
    protected float _startValue = 0f;
    //目标值
    private float _targetValue = 0f;
    //当前更新值
    private float _curValue = 0f;
    public float curValue => this._curValue;
    //结束回调
    private UnityAction _endCallback;
    public bool needUpdate = false;

    public bool isDone
    {
        get { return !needUpdate || this._timer > this._duration; }
    }

    public void SetupLerp(
    float startValue,
    float targetValue,
    float durationRatio,
    UnityAction endCallback = null,
    CameraLerpMode lerpMode = CameraLerpMode.LineLerp,
    bool isForce = false
)
    {
        this._endCallback = endCallback;

        if (startValue == targetValue)
        {
            this.needUpdate = false;
            this.InvokeCallback();
        }
        else
        {
            this.needUpdate = true;

            this._startValue = startValue;
            this._curValue = startValue;
            this._targetValue = targetValue;
            this._durationRatio = durationRatio;
            this._duration = this._durationRatio * Math.Abs(startValue - targetValue);
            this._lerpMode = lerpMode;
            this._isForce = isForce;
            this._timer = 0f;
        }
    }


    public void LerpStep()
    {
        if (this._timer <= this._duration)
        {
            this._timer += Time.deltaTime;
            if (this._timer < this._duration)
            {
                switch (this._lerpMode)
                {
                    case CameraLerpMode.LineLerp:
                        this._curValue = CameraUtil.Lerp(this._startValue, this._targetValue, this._timer / this._duration);
                        break;
                    case CameraLerpMode.SquareLerp:
                        this._curValue = CameraUtil.SquareLerp(this._startValue, this._targetValue, this._timer / this._duration);
                        break;
                    case CameraLerpMode.CubeLerp:
                        this._curValue = CameraUtil.CubeLerp(this._startValue, this._targetValue, this._timer / this._duration);
                        break;
                }
            }
            else
            {
                this._curValue = this._targetValue;
                this.InvokeCallback();
            }
        }
    }

    /// <summary>
    /// 若希望在原有的过渡过程中修改新的目标值，需要先通过该函数获取对应进度下新目标对应的结果值
    /// </summary>
    /// <param name="targetVal"></param>
    /// <returns></returns>
    public float GetLerpProcessByTargetVal(float targetVal)
    {
        if (needUpdate && this._timer < this._duration)
        {
            return CameraUtil.Lerp(this._curValue, targetVal, this._timer / this._duration);
        }
        return targetVal;
    }

    private void InvokeCallback()
    {
        if (this._endCallback != null)
        {
            this._endCallback();
            this._endCallback = null;
        }
    }
}



public static class CameraConfig
{
    //镜头距离默认参数
    public static float NormalCamDistance = 5; // 正常距离
    public static float MinCamDistance = 2; // 最小距离
    public static float MaxCamDistance = 30; // 最大距离
    public static float MinHorizontalAxisVal = -60; // 最小水平轴旋转值
    public static float MaxHorizontalAxisVal = 60; // 最大水平轴旋转值
    public static float TransitionLerpTime = 0.5f; // 镜头过渡默认参数

    public static float TrackedOffsetPresetY = 0.3f; // 跟踪偏移预设Y

    public static float DampingXY = 0.1f; // 阻尼
    public static float DampingZ = 0.1f; // 阻尼

    public static float EnterCloseShotDist = 3f; // 进入近景模式距离

    public static float ExitCloseShotDist = 2.5f;  //退出近景模式的条件

    public static float RecoverCloseShotDist = 5f; //恢复近景模式时的距离

    public static float FocalTime = 1f; //过度时间

    public static float FocalMinDist = 2f; //进入近景模式后的最近距离


    // 震屏相关参数
    public static float MaxShakeStrength = 0.9f; // 震屏最大Strength值
    public static float MinShakeStrength = 0.1f; // 震屏最小Strength值
    public static float CameraFollowAvatarAndTargetAngle = 30f; // 战斗相机最终回正角度
}

public enum CameraStateID
{
    None = 0,
    FollowState = 1,
}

public enum CameraFollowStateID
{
    None = 0,
    FollowCloseShotState = 1,
    FollowChangeParamState = 2,
}


public enum CameraFollowShortStateID
{
    None = 0,
    FollowManualControlRotateState = 1,
}


public enum CameraLerpMode
{
    LineLerp = 0,
    SquareLerp = 1,
    CubeLerp = 2,
}
