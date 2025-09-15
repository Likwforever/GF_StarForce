using System;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

public class CameraMgr : GameFrameworkComponent
{
    public Transform followTrans;
    private Camera _cameraCom;

    private Transform _cameraTrans;

    private BaseMainCameraState _curState;
    private BaseMainCameraState _nextState;

    private List<BaseMainCameraState> _cameraStack;

    private MainCameraFollowState _followState;

    private Vector3 _transitionFromPos;
    private Vector3 _transitionFromForward;

    private CameraParameter _cameraFovParam;

    private bool _isDoingTransitionLerp;
    private float _transitionLerpTimer = 0;
    private float _transitionLerpSpeedRatio = 1;

    private float _manualVerticalRatio = 1;
    private float _manualHorizontalRatio = 1;

    public Transform CameraTrans
    {
        get => this._cameraTrans;
    }

    public Camera CameraCom
    {
        get => this._cameraCom;
    }

    void Start()
    {
        this._cameraCom = Camera.main;
        this._cameraTrans = _cameraCom.transform;

        this._cameraStack = new List<BaseMainCameraState>();

        this._cameraFovParam = new CameraParameter();

        this._followState = new MainCameraFollowState(this);
        this._followState.SetupFollowAvatar(this.followTrans);
        this.TransitToFollowState();
    }

    void Update()
    {
        // 滚轮调整摄像机距离
        float mouseScrollWheel = Input.GetAxis("Mouse ScrollWheel");
        if (mouseScrollWheel != 0)
        {
            float scale = 3;
            if (this._followState.active)
            {
                this.ChangeCameraFollowDistanceByDelta(-mouseScrollWheel * scale);
            }
        }

        // 鼠标右键旋转镜头
        // if (this._wxMouseButtonSet.has(2))
        // {
        //     let movementFactor = 1.15;
        //     let dx = this._wxMouseMovement[0] * movementFactor;
        //     let dy = this._wxMouseMovement[1] * movementFactor;
        //     GameCtrl.Scene.camera.SetManualDragControl(dy, dx);
        // }

        // 测试代码
        if (Input.GetKeyDown(KeyCode.F))
        {
            this.TransitToFollowState();
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            this.TryDisableTargetState(this._followState);
        }
    }

    void LateUpdate()
    {
        if (this._nextState != null)
        {
            if (this._curState != null)
            {
                this._curState.Exit();
                //记录上个相机位置数据
                this._transitionFromPos = this._curState.cameraPosition;
                this._transitionFromForward = this._curState.cameraForward;
            }
            this._curState = this._nextState;
            this._curState.Enter();

            this._nextState = null;
        }

        // this.UpdateCameraParam();

        if (this._curState == null) return;

        this._curState.Update();

        // 无需过渡切换
        if (!this._isDoingTransitionLerp)
        {
            this.UpdateCameraPos(this._curState.cameraPosition, this._curState.cameraForward, this._curState.cameraRotation);
        }
        else
        {
            // 过渡切换
            this._transitionLerpTimer += Time.deltaTime * this._transitionLerpSpeedRatio;
            // 默认过渡时间为TransitionLerpTime
            float t = this._transitionLerpTimer / CameraConfig.TransitionLerpTime;
            if (t > 1)
            {
                //过渡完毕
                this.UpdateCameraPos(this._curState.cameraPosition, this._curState.cameraForward, this._curState.cameraRotation);

                this.ResetTransitionLerp();
            }
            else
            {
                // 缓出（Ease Out）的数学变换
                // 缓冲插值t = 2 * t - t * t
                // 起始速度较快：当 t 接近 0 时，变化率较大
                // 结束速度较慢：当 t 接近 1 时，变化率逐渐减小到 0
                // 平滑过渡：避免了线性插值可能带来的突兀感
                t *= 2 - t;
                Vector3 targetCamPos = this._transitionFromPos;
                targetCamPos = Vector3.Lerp(targetCamPos, this._curState.cameraPosition, t);
                Vector3 targetCamForward = this._transitionFromForward;
                targetCamForward = Vector3.Lerp(targetCamForward, this._curState.cameraForward, t);
                this.UpdateCameraPos(targetCamPos, targetCamForward, this._curState.cameraRotation);
            }
        }
    }

    private void UpdateCameraParam()
    {
        if (this._cameraFovParam.needUpdate)
        {
            this._cameraFovParam.LerpStep();
            this._cameraCom.fieldOfView = this._cameraFovParam.curValue;
        }

        if (this._cameraFovParam.isDone && this._cameraFovParam.needUpdate)
        {
            this._cameraFovParam.needUpdate = false;
        }
    }

    private void UpdateCameraPos(Vector3 position, Vector3 forward, float rotation)
    {
        this._cameraTrans.position = position;
        this._cameraTrans.localRotation = Quaternion.LookRotation(forward);
        this._cameraTrans.Rotate(0, 0, rotation);
    }

    /// <summary>
    /// 移除目标State
    /// </summary>
    /// <param name="targetState"></param>
    /// <param name="enableLerp"></param>
    /// <param name="lerpRatio"></param>
    public void TryDisableTargetState(BaseMainCameraState targetState, bool enableLerp = false, float lerpRatio = 1)
    {
        if (this._nextState == targetState)
        {
            this._nextState = null;
        }

        if (targetState == this._curState)
        {
            // 若目标state为当前state，则尝试获取下一个栈内state
            if (!this.TryTransitToOtherBaseState(enableLerp, lerpRatio))
            {
                this._curState.Exit();
                this._curState = null;
            }
        }
        else
        {
            // 若目标state非当前state，则直接移出栈
            for (int i = this._cameraStack.Count - 1; i >= 0; i--)
            {
                BaseMainCameraState state = this._cameraStack[i];
                if (targetState == state)
                {
                    this._cameraStack.RemoveAt(i);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 改变相机跟随距离
    /// </summary>
    /// <param name="delta"></param>
    public void ChangeCameraFollowDistanceByDelta(float delta)
    {
        if (this._isDoingTransitionLerp)
        {
            return;
        }

        if (this._followState.active)
        {
            float curDist = this._followState.targetManualDist;
            this._followState.ChangeFollowDistanceManually(curDist + delta);
        }
    }

    /// <summary>
    /// 设置手动拖拽控制
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void SetManualDragControl(float dx, float dy)
    {
        if (this._isDoingTransitionLerp)
        {
            return;
        }

        this._followState.TryStartFollowManualControlRtotate();

        if (this._followState.active && this._followState.manualRotateState.active)
        {
            this._followState.manualRotateState.SetDragDelta(dx * this._manualVerticalRatio, dy * this._manualHorizontalRatio);
        }
    }


    /// <summary>
    /// 切换至跟随状态
    /// </summary>
    public void TransitToFollowState(bool enableLerp = false, float lerpRatio = 1)
    {
        this.TryTransitToTargetBaseState(this._followState, enableLerp, lerpRatio);
    }


    /// <summary>
    /// 尝试切换至目标状态
    /// </summary>
    /// <param name="newBaseState"></param>
    /// <param name="enableLerp"></param>
    /// <param name="lerpRatio"></param>
    /// <returns></returns>
    private bool TryTransitToTargetBaseState(BaseMainCameraState newBaseState, bool enableLerp = false, float lerpRatio = 1)
    {

        // 若当前没有状态，则直接应用newBaseState
        if (this._cameraStack.Count == 0)
        {
            this._cameraStack.Add(newBaseState);
            if (enableLerp)
            {
                this.TransitWithLerp(newBaseState, lerpRatio);
            }
            else
            {
                this.Transit(newBaseState);
            }
            return true;
        }

        var curTopState = this._cameraStack[this._cameraStack.Count - 1];
        // 若尝试加入的state priority小于当前state priority则无法加入
        if (newBaseState.priority >= curTopState.priority)
        {
            int count = this._cameraStack.Count;
            if (this._cameraStack[count - 1] == newBaseState)
            {
                return false;
            }

            if (!newBaseState.saveLastBaseState)
            {
                // 弹出栈顶State
                this._cameraStack.RemoveAt(count - 1);
            }

            // 若已经存在相同State, 则弹至栈顶
            for (int i = 0; i < this._cameraStack.Count; i++)
            {
                if (this._cameraStack[i] == newBaseState)
                {
                    this._cameraStack.RemoveAt(i);
                }
            }

            this._cameraStack.Add(newBaseState);

            if (enableLerp)
            {
                this.TransitWithLerp(newBaseState, lerpRatio);
            }
            else
            {
                this.Transit(newBaseState);
            }

            return true;
        }
        return false;
    }

    private void Transit(BaseMainCameraState to)
    {
        if (this._isDoingTransitionLerp)
        {
            this.ResetTransitionLerp();
        }
        if (this._nextState != null)
        {
            this.ClearNextState();
        }
        this._nextState = to;
    }

    private void TransitWithLerp(BaseMainCameraState to, float lerpRatio = 1)
    {
        if (this._curState == null)
        {
            // 当前存在相机时才可以启用过渡效果
            this.Transit(to);
        }
        else
        {
            if (this._nextState != null)
            {
                this.ClearNextState();
            }
            this._nextState = to;
            this.SetupLerpParam(this._curState.cameraPosition, this._curState.cameraForward, lerpRatio);
        }
    }

    // 清除下一个状态
    private void ClearNextState()
    {
        this._isDoingTransitionLerp = false;
        this._nextState = null;
    }

    // 设置状态过度参数
    private void SetupLerpParam(Vector3 fromPos, Vector3 fromFoward, float lerpRatio)
    {
        this._transitionFromPos = fromPos;
        this._transitionFromForward = fromFoward;

        this._transitionLerpTimer = 0;
        this._transitionLerpSpeedRatio = lerpRatio;
        this._isDoingTransitionLerp = true;
    }

    /// <summary>
    /// 重置状态过渡参数
    /// </summary>
    private void ResetTransitionLerp()
    {
        this._transitionFromPos = Vector3.zero;
        this._transitionFromForward = Vector3.zero;

        this._isDoingTransitionLerp = false;
    }

    /// <summary>
    /// 退出当前栈顶状态，尝试切换至其他状态
    /// </summary>
    /// <param name="enableLerp"></param>
    /// <param name="lerpRatio"></param>
    /// <returns></returns>
    private bool TryTransitToOtherBaseState(bool enableLerp, float lerpRatio)
    {
        BaseMainCameraState to = null;
        // 出栈栈顶state后获取新的栈顶state
        int count = this._cameraStack.Count;
        if (count > 0)
        {
            this._cameraStack.RemoveAt(count - 1);
        }

        count = this._cameraStack.Count;
        if (count > 0)
        {
            to = this._cameraStack[count - 1];
        }

        if (to != null)
        {
            if (enableLerp)
            {
                this.TransitWithLerp(to, lerpRatio);
            }
            else
            {
                this.Transit(to);
            }
            return true;
        }
        else
        {
            // 若栈内无state，则尝试切换至follow state(默认状态)
            if (this._followState.valid)
            {
                to = this._followState;
                this.TryTransitToTargetBaseState(to, enableLerp, lerpRatio);
                return true;
            }
        }

        return false;
    }
}
