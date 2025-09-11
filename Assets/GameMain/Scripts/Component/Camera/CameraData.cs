using System;
using UnityEngine;

public class CameraParameter
{
    //计时器
    protected float _timer;
    //速度因子
    protected float _durationRatio;
    //recover持续时长
    protected float _duration;
    //插值模式
    //protected CameraLerpMode _lerpMode;
    //是否无视Vertical、Horizontal Lock的限制
    protected bool _isForce;
    //记录开始值
    protected float _startValue = 0f;
    //目标值
    private float _targetValue = 0f;
    //当前更新值
    private float _curValue = 0f;
    public float CurValue
    {
        get { return _curValue; }
    }

    //结束回调
    private Action _endCallback;
    public bool needUpdate = false;

    public bool IsDone
    {
        get { return !needUpdate || _timer > _duration; }
    }

    public bool IsForce
    {
        get { return _isForce; }
    }

    public void LerpStep()
    {
        if (_timer <= _duration)
        {
            _timer += Time.deltaTime;
            if (_timer < _duration)
            {
                // switch (_lerpMode)
                // {
                //     // case CameraLerpMode.LineLerp:
                //     //     _curValue = CameraUtil.Lerp(_startValue, _targetValue, _timer / _duration);
                //     //     break;
                //     // case CameraLerpMode.SquareLerp:
                //     //     _curValue = CameraUtil.SquareLerp(_startValue, _targetValue, _timer / _duration);
                //     //     break;
                //     // case CameraLerpMode.CubeLerp:
                //     //     _curValue = CameraUtil.CubeLerp(_startValue, _targetValue, _timer / _duration);
                //     //     break;
                // }
            }
            else
            {
                _curValue = _targetValue;
                InvokeCallback();
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
        if (needUpdate && _timer < _duration)
        {
            //return CameraUtil.Lerp(_curValue, targetVal, _timer / _duration);
        }
        return targetVal;
    }

    public void SetupLerp(
        float startValue,
        float targetValue,
        float durationRatio,
        Action endCallback = null,
        // CameraLerpMode lerpMode = CameraLerpMode.LineLerp,
        bool isForce = false
    )
    {
        _endCallback = endCallback;

        if (startValue == targetValue)
        {
            needUpdate = false;
            InvokeCallback();
        }
        else
        {
            needUpdate = true;
            _startValue = startValue;
            _curValue = startValue;
            _targetValue = targetValue;
            _durationRatio = durationRatio;
            _duration = _durationRatio * Math.Abs(startValue - targetValue);
            //_lerpMode = lerpMode;
            _isForce = isForce;
            _timer = 0f;
        }
    }

    private void InvokeCallback()
    {
        if (_endCallback != null)
        {
            _endCallback();
            _endCallback = null;
        }
    }
}



public static class CameraConfig
{
    public static float TransitionLerpTime = 0.5f; //     // 镜头过渡默认参数

    public static float TrackedOffsetPresetY = 0.3f; // 跟踪偏移预设Y

    public static float DampingXY = 0.1f; // 阻尼
    public static float DampingZ = 0.1f; // 阻尼
}

public enum CameraStateID
{
    None = 0,
    FollowState = 1,
}


public enum CameraFollowShortStateID
{
    None = 0,
    FollowManualControlRotateState = 1,
}
