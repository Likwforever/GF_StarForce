using System;
using UnityEngine;
using UnityGameFramework.Runtime;

public class CameraMgr : GameFrameworkComponent
{
    public Transform followTrans;
    private Camera _cameraCom;

    private Transform _cameraTrans;

    private BaseMainCameraState _curState;
    private BaseMainCameraState _nextState;

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

        this._cameraFovParam = new CameraParameter();

        this._followState = new MainCameraFollowState(this);
        this._followState.SetupFollowAvatar(this.followTrans);
        this.TransitionToFollowState();
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
            // 默认过渡时间为TransitionLerpTime / transitionLerpSpeedRatio
            float t = this._transitionLerpTimer / CameraConfig.TransitionLerpTime;
            if (t > 1)
            {
                //过渡完毕
                this.UpdateCameraPos(this._curState.cameraPosition, this._curState.cameraForward, this._curState.cameraRotation);

                this.ResetTransitionLerp();
            }
            else
            {
                //缓冲插值t = 2 * t - t * t
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

    private void ResetTransitionLerp()
    {
        this._transitionFromPos = Vector3.zero;
        this._transitionFromForward = Vector3.zero;

        this._isDoingTransitionLerp = false;
    }

    public void TransitionToFollowState()
    {
        this.TryTransitToTargetState(this._followState);
    }

    private void TryTransitToTargetState(BaseMainCameraState targetState)
    {
        if (this._curState != null && this._curState.ID == targetState.ID)
        {
            return;
        }
        this._nextState = targetState;
    }

    internal void ChangeCameraFollowDistanceByDelta(float delta)
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
}
