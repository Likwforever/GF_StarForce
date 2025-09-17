using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FollowData
{
    public Vector3 followCenterPos; // 跟随中心点
    public Vector3 cameraPosition; // 相机位置
    public Vector3 cameraForward; // 相机朝向
}

public class CorrectedData
{
    public Vector3 correctedFollowCenter; // 修正后的跟随中心点
    public Vector3 correctedCameraPos; // 修正后的相机位置
    public Vector3 correctedCameraForward; // 修正后的相机朝向
    public Vector2 correctedRotation; // 修正后的相机旋转
    public float correctedDist; // 修正后的相机距离
    public float correctedDistLerp; // 修正后的相机距离插值
}
public class MainCameraFollowState : BaseMainCameraState
{

    public override CameraStateID id => CameraStateID.FollowState;

    public override int priority => 1;  // 优先级: 在切换状态时，目标状态的优先级小于当前状态的优先级，则无法切换至目标状态

    private Vector3 _lastFrameCameraPosition;
    private Vector3 _lastFrameFollowCenterPos;
    private float _trackedOffectCorrectedY;
    private Vector3 _trackedOffsetPreset; // 跟踪偏移预设
    private float _minDist; // 最小距离
    public float minDist => this._minDist;
    private float _maxDist; // 最大距离
    public float maxDist => this._maxDist;
    private Vector3 _followPointPos;

    public FollowData _followData;
    private FollowData _lastFrameFollowData;

    private CorrectedData _correctedData;

    private Vector3 _followPos;
    public Vector3 followPos
    {
        get
        {
            if (this._followPos == null)
            {
                this._followPos = Vector3.zero;
            }
            if (this._followTrans == null)
            {
                return Vector3.zero;
            }

            this._followPos = this._followTrans.position;
            return this._followPos;
        }
    }

    private Transform _followTrans;
    public Transform followTrans
    {
        get
        {
            return this._followTrans;
        }
    }

    public Vector3 followDir
    {
        get
        {
            return this._followTrans.forward;
        }
    }

    private Vector3 _lookAtPointPos;
    public Vector3 lookAtPointPos
    {
        get
        {
            return this._lookAtPointPos;
        }
    }

    private Vector2 _axisVal;
    private float _targetManualDist;

    private float _minHorizontalAxisVal;
    private float _maxHorizontalAxisVal;

    //Base Follow State Map
    private Dictionary<CameraFollowStateID, BaseFollowBaseState> _stateMap;

    // 单独更新状态
    private FollowChangeParamState _changeParamState;

    // BaseFollowStates
    private BaseFollowBaseState _curState;
    private BaseFollowBaseState _lastState;
    private BaseFollowBaseState _nextState;

    // BaseFollowShortStates
    private BaseFollowShortState _curShortState;
    public FollowRangeTransitState rangeTransitState;
    public FollowManualControlRotateState manualRotateState;

    // 碰撞检测
    private Vector3 _hitPos;
    //判断是否第一次碰撞，第一次碰撞直接位移到碰撞点
    private bool _isFirstHit = true;
    private float _hitDistance;

    // 阻尼
    private float _dampingXY;
    private float _dampingZ;

    private bool _avatarMoving; // 是否移动

    private bool _manualDistChange; // 是否手动改变距离

    private float _followDistLerp = 1;

    public float targetManualDist
    {
        get => this._targetManualDist;
    }

    public float verticalAxisVal
    {
        get => this._axisVal.y;
    }
    public float horizontalAxisVal
    {
        get => this._axisVal.x;
    }

    public MainCameraFollowState(CameraMgr owner) : base(owner)
    {
        this._followData = new FollowData();
        this._lastFrameFollowData = new FollowData();
        this._correctedData = new CorrectedData();

        // 初始化相机参数
        this._minDist = CameraConfig.MinCamDistance;
        this._maxDist = CameraConfig.MaxCamDistance;
        this._minHorizontalAxisVal = CameraConfig.MinHorizontalAxisVal;
        this._maxHorizontalAxisVal = CameraConfig.MaxHorizontalAxisVal;
        this._targetManualDist = CameraConfig.NormalCamDistance;
        this._trackedOffsetPreset = new Vector3(0, CameraConfig.TrackedOffsetPresetY, 0);

        // 初始化阻尼参数
        this._dampingXY = CameraConfig.DampingXY;
        this._dampingZ = CameraConfig.DampingZ;

        // 初始化状态
        // base state
        this._stateMap = new Dictionary<CameraFollowStateID, BaseFollowBaseState>();
        this.AddState(new FollowCloseShotState(this));
        this.AddState(new FollowAvatarPathFindingState(this));
        // short state
        this.rangeTransitState = new FollowRangeTransitState(this);
        this.manualRotateState = new FollowManualControlRotateState(this);
        // param state
        this._changeParamState = new FollowChangeParamState(this);

    }


    private void InitCameraParam()
    {
        this._trackedOffectCorrectedY = CameraUtil.Lerp(0, this._trackedOffsetPreset.y, (this._targetManualDist - this._minDist) / (this._maxDist - this._minDist));

        this._followPointPos = this.followPos;
        this._followPointPos.y += this._trackedOffectCorrectedY;
        this._lookAtPointPos = this.followPos;
        this._lookAtPointPos.y += this._trackedOffectCorrectedY;

        this.CollectData();

        this._correctedData.correctedDist = this._targetManualDist;
        this.cameraPosition = this._followData.cameraPosition;
        this.cameraForward = this._followData.cameraForward;

    }

    public override void Update()
    {
        base.Update();
        // 记录上一帧的相机位置和跟随中心点
        this._lastFrameCameraPosition = this.cameraPosition;
        this._lastFrameFollowCenterPos = this._followData.followCenterPos;

        // 动态控制相机跟随Y轴偏移
        // 这样可以使得相机的缩放更加自然
        this._trackedOffectCorrectedY = CameraUtil.Lerp(
            0.1f,
            this._trackedOffsetPreset.y,
            (this._correctedData.correctedDist - this._minDist) / (this._maxDist - this._minDist) // 当前距离占可控距离的比例
        );

        this._followPointPos = this.followPos;
        this._followPointPos.y += this._trackedOffectCorrectedY;
        this._lookAtPointPos = this.followPos;
        this._lookAtPointPos.y += this._trackedOffectCorrectedY;

        //判断跟随目标是否产生位移
        this._avatarMoving = false;
        if (Vector3.Distance(this._followPointPos, this._lastFrameFollowCenterPos) > 0.01)
        {
            this._avatarMoving = true;
            //this.SetBaseStateValid(CameraFollowStateID.FollowCloseShotState, false);
        }
        else
        {
            //this.SetBaseStateValid(CameraFollowStateID.FollowCloseShotState, true);
        }

        if (this._changeParamState.active)
        {
            this._changeParamState.Update();
        }

        // if (this._recoveringState.active)
        // {
        //     this._recoveringState.Update();
        // }


        // 状态更新与切换
        if (this._nextState != null)
        {
            this._curState?.Exit();
            this._lastState = this._curState;
            this._curState = this._nextState;
            this._curState.Enter();
            this._nextState = null;
        }

        if (this._curShortState != null)
        {
            this._curShortState.Update();
            // 再判断一次shortState是否被移除，因为update中short state会执行exit
            if (this._curShortState != null && this._curState != null)
            {
                // 如果shortState不需要跳过baseState，或者baseState不能被跳过，则更新baseState
                if (!this._curShortState.isSkippingBaseState || this._curState.cannotBeSkipped)
                {
                    this._curState.Update();
                }
            }
        }
        else if (this._curState != null)
        {
            this._curState.Update();
        }


        // 收集数据
        this.CollectData();

        // 数据修正
        // 都将使用correctedData进行修正，防止数据污染
        float targetDist = this._targetManualDist;
        this._correctedData.correctedRotation.x = this._axisVal.x;
        this._correctedData.correctedRotation.y = this._axisVal.y;
        this._correctedData.correctedDistLerp = this._followDistLerp;
        this._correctedData.correctedCameraPos = this._followData.cameraPosition;
        this._correctedData.correctedCameraForward = this._followData.cameraForward;
        this._correctedData.correctedFollowCenter = this._followData.followCenterPos;

        // 碰撞检测
        Vector3 startPos = this._followData.followCenterPos;
        Vector3 endPos = this._followData.cameraPosition;

        RaycastHit hitInfo;
        Vector3 rayDir = (endPos - startPos).normalized;
        float rayDist = Vector3.Distance(startPos, endPos);

        // 这里假设相机碰撞检测忽略Trigger，使用默认Layer
        bool hit = Physics.Raycast(startPos, rayDir, out hitInfo, rayDist, LayerMask.GetMask("Ground"), QueryTriggerInteraction.Ignore);

        // 为什么？这里碰撞后对相机进行了位置修正，还能够检测到碰撞？
        // 因为碰撞检测是基于相机位置进行的，而不是基于修正后的位置，这就是使用correctedData的好处
        if (hit)
        {
            // 0.2f是碰撞偏移,因为相机需要有一个偏移量，否则会直接碰撞到物体
            Vector3 temp = hitInfo.point + hitInfo.normal * 0.2f;

            // 第一次碰撞：直接设置碰撞点
            if (this._isFirstHit)
            {
                this._isFirstHit = false;
                this._hitPos = temp;
            }
            // 非第一次碰撞：进行缓动，防止相机频繁抖动(平面可能区别不大，但是对于凹凸不平的地面，效果更明显)
            this._hitPos = Vector3.Lerp(this._hitPos, temp, 0.2f);
            this._correctedData.correctedCameraPos = this._hitPos;

            this._hitDistance = Vector3.Distance(this._hitPos, this._followData.followCenterPos);
            this._correctedData.correctedDist = this._hitDistance;
            targetDist = this._hitDistance;

            // 修正相机的朝向
            // 此处为什么只修改水平轴的旋转？
            // 因为水平轴的旋转直接影响相机与目标的距离
            // 碰撞检测主要关注距离问题，水平轴旋转对距离影响最大
            // 垂直轴旋转主要影响视角，对碰撞距离影响较小
            Vector3 lookDirection = (this._followData.followCenterPos - this._correctedData.correctedCameraPos).normalized;
            Quaternion correctedDir = Quaternion.LookRotation(lookDirection, Vector3.up);
            this._correctedData.correctedRotation.x = CameraUtil.NormalizeHorizontalAxis(correctedDir.eulerAngles.x);
        }
        // 没有碰撞,但有碰撞信息，则需要进行碰撞缓冲过程
        else if (this._hitDistance != 0)
        {
            if (!this._isFirstHit)
            {
                this._isFirstHit = true;
            }
            // hitDistance不为0时，进行碰撞缓冲过程（即相机慢回）
            if (Math.Abs(this._correctedData.correctedDist - targetDist) > 0.1)
            {
                this._correctedData.correctedDistLerp = 0.01f;
            }
            else
            {
                this._hitDistance = 0;
            }
            if (this._manualDistChange)
            {
                this._correctedData.correctedDistLerp = this._followDistLerp;
                this._hitDistance = 0;
            }
        }

        // 进行距离缓冲
        this._correctedData.correctedDist = CameraUtil.Lerp(this._correctedData.correctedDist, targetDist, this._correctedData.correctedDistLerp);

        // 计算屏幕空间阻尼
        // 处理问题：相机跟随的抖动和不稳定，使得相机能够平稳的过渡到目标位置
        // cameraScreenOffset计算的是上一帧相机位置相对于当前相机朝向的屏幕空间偏移
        Vector3 cameraScreenOffset = this.GetOrthoOffsetToScreenBounds(this._lookAtPointPos, this._lastFrameCameraPosition, this.cameraForward);
        //     //##################暂时性提交，阻尼包围限制##################
        //     // this._owner.cameraCom.RestrictFollowCameraOffsetToBoundRange(
        //     // 	this._dampBoundX,
        //     // 	this._dampBoundY,
        //     // 	cameraScreenOffset.x,
        //     // 	cameraScreenOffset.y,
        //     // 	cameraScreenOffset.z,
        //     // 	this._tempNumRef1,
        //     // 	this._tempNumRef2,
        //     // 	this._tempNumRef3
        //     // );
        //     // let posX: number = puerts.$unref<number>(this._tempNumRef1);
        //     // let posY: number = puerts.$unref<number>(this._tempNumRef2);
        //     // let posZ: number = puerts.$unref<number>(this._tempNumRef3);
        //     //##################暂时性提交，阻尼包围限制##################

        // 让相机平滑的移动到目标位置
        cameraScreenOffset.y = CameraUtil.Damp(cameraScreenOffset.y, this._dampingXY, Time.deltaTime);
        cameraScreenOffset.x = CameraUtil.Damp(cameraScreenOffset.x, this._dampingXY, Time.deltaTime);
        cameraScreenOffset.z = CameraUtil.Damp(cameraScreenOffset.z, this._dampingXY, Time.deltaTime);

        // 计算距离修正值，用于调整相机与目标的距离
        float cameraZOffset =
                this.GetScreenZOffset(this._followPointPos, this._lookAtPointPos, this._lastFrameCameraPosition, this.cameraForward) -
                this._correctedData.correctedDist;
        // 对距离修正值也进行缓动
        cameraZOffset = CameraUtil.Damp(cameraZOffset, this._dampingZ, Time.deltaTime);

        // 仅在没发生碰撞的时候进行阻尼优化
        if (this._hitDistance == 0)
        {
            this._correctedData.correctedDist += cameraZOffset;
            this._correctedData.correctedFollowCenter += cameraScreenOffset;
        }

        // 计算修正后相机真正的位置
        Vector3 temp3Vec = this.CalculateCameraWorldPos(
            this._correctedData.correctedRotation,
            this._correctedData.correctedDist,
            this._correctedData.correctedFollowCenter
        );
        this._correctedData.correctedCameraPos = temp3Vec;

        this.cameraPosition = this._correctedData.correctedCameraPos;
        this.cameraForward = this._correctedData.correctedCameraForward;

        this._manualDistChange = false;
    }

    private void CollectData()
    {
        // save last frame
        this._lastFrameFollowData.followCenterPos = this._followData.followCenterPos;
        this._lastFrameFollowData.cameraForward = this._followData.cameraForward;
        this._lastFrameFollowData.cameraPosition = this._followData.cameraPosition;

        // Calculate Follow
        this._followData.followCenterPos = this._followPointPos;
        Vector3 cameraWorldPos = this.CalculateCameraWorldPos(this._axisVal, this._targetManualDist, this._followPointPos);
        this._followData.cameraPosition = cameraWorldPos;

        // Calculate LookAt
        Vector3 forwardDir = this._lookAtPointPos - this._followData.cameraPosition;
        forwardDir = forwardDir.normalized;
        this._followData.cameraForward = forwardDir;
    }

    private Vector3 CalculateCameraWorldPos(Vector2 rotation, float dist, Vector3 followCenterPos)
    {
        Quaternion tempQuater = Quaternion.Euler(rotation.x, rotation.y, 0);
        Vector3 tempVec = tempQuater * Vector3.forward;

        tempVec = tempVec * dist;
        tempVec = followCenterPos - tempVec;
        return tempVec;
    }

    /// <summary>
    /// 获取target在屏幕空间中的偏移
    /// </summary>
    /// <param name="lookAtPos"></param>
    /// <param name="lastFrameCamPos"></param>
    /// <param name="curCameraforward"></param>
    /// <returns></returns>
    private Vector3 GetOrthoOffsetToScreenBounds(Vector3 lookAtPos, Vector3 lastFrameCamPos, Vector3 curCameraforward)
    {
        float curForwardMagnitude = curCameraforward.magnitude;
        Vector3 lastFrameForward = lookAtPos - lastFrameCamPos;
        float dotProduct = Vector3.Dot(lastFrameForward, curCameraforward);
        Vector3 projection = curCameraforward * dotProduct / curForwardMagnitude;

        //计算得到基于屏幕空间平面的xy向量，结果为世界坐标系向量
        Vector3 result = lastFrameForward - projection;

        result.x = -result.x;
        result.y = -result.y;
        result.z = -result.z;

        return result;
    }

    /// <summary>
    /// 获取屏幕Z轴偏移
    /// </summary>
    /// <param name="followPos"></param>
    /// <param name="lookAtPos"></param>
    /// <param name="lastFrameCamPos"></param>
    /// <param name="forward"></param>
    /// <returns></returns>
    private float GetScreenZOffset(Vector3 followPos, Vector3 lookAtPos, Vector3 lastFrameCamPos, Vector3 forward)
    {
        Vector3 lastFrameDist = lookAtPos - lastFrameCamPos;
        Vector3 lastFrameDir = lastFrameDist.normalized;
        float dotProduct = Vector3.Dot(lastFrameDir, forward);
        // 浮点数误差，要对dot结果进行四舍五入，否则计算不正确
        dotProduct = Mathf.Round(dotProduct * 1000) / 1000;
        if (Math.Abs(dotProduct - 1) <= float.MinValue)
        {
            return Vector3.Distance(followPos, lastFrameCamPos);
        }
        dotProduct = Vector3.Dot(lastFrameDist, forward);
        float magnitude = forward.magnitude;
        // 仅获取相机forward方向的矢量
        Vector3 projection = forward * dotProduct / magnitude;

        // 结果不能约
        float result = projection.magnitude;
        return result;
    }

    private void AddState(BaseFollowBaseState state)
    {
        CameraFollowStateID id = state.ID;
        if (this._stateMap.ContainsKey(id))
        {
            Debug.LogError("MainCameraFollowState.AddState(): state already exists.");
            return;
        }
        state.SetValid(true);
        this._stateMap[id] = state;
    }

    /// <summary>
    /// 设置绕水平轴旋转的值，即垂直方向旋转
    /// </summary>
    /// <param name="val"></param>
    /// <param name="isForce"></param>
    public void SetHorizontalAxisVal(float val, bool isForce = false)
    {
        // if (!isForce)
        // {
        //     return;
        // }
        this._axisVal.x = Mathf.Clamp(CameraUtil.NormalizeHorizontalAxis(val), this._minHorizontalAxisVal, this._maxHorizontalAxisVal);
    }

    /// <summary>
    /// 改变跟随距离
    /// </summary>
    /// <param name="dist"></param>
    public void ChangeFollowDistance(float dist)
    {
        this._targetManualDist = Mathf.Clamp(dist, this._minDist, this._maxDist);
    }
    /// <summary>
    /// 设置绕垂直轴旋转的值，即水平方向旋转
    /// </summary>
    /// <param name="val"></param>
    /// <param name="isForce"></param>
    public void SetVerticalAxisVal(float val, bool isForce = false)
    {
        // if (!isForce)
        // {
        //     return;
        // }
        this._axisVal.y = CameraUtil.NormalizeVerticalAxis(val);
    }


    /**
     * 设置相机旋转
     * @param horizontal 绕水平轴旋转的值
     * @param vertical 绕垂直轴旋转的值
     */
    public void SetAixsVal(float horizontal, float vertical)
    {
        this.SetVerticalAxisVal(vertical);
        this.SetHorizontalAxisVal(horizontal);
    }

    /// <summary>
    /// 设置最小跟随距离
    /// </summary>
    /// <param name="minDist"></param>
    public void SetMinFollowDistance(float minDist)
    {
        this._minDist = minDist;
        if (this._targetManualDist < this._minDist) this._targetManualDist = this._minDist;
    }

    public void SetMaxFollowDistance(float maxDist)
    {
        this._minDist = maxDist;
        if (this._targetManualDist > this._maxDist) this._targetManualDist = this._maxDist;
    }


    /// <summary>
    /// 设置跟随目标
    /// </summary>
    /// <param name="followTrans"></param>
    public void SetupFollowAvatar(Transform followTrans)
    {
        this._followTrans = followTrans;
        this._lastFrameFollowCenterPos = followTrans.position;

        this.InitCameraParam();
    }

    /// <summary>
    /// 改变相机跟随距离
    /// </summary>
    /// <param name="dist"></param>
    public void ChangeFollowDistanceManually(float dist)
    {
        this._manualDistChange = true;
        // 判断是缩放
        float offset = dist - this.targetManualDist;

        this.ChangeFollowDistance(dist);


        // 进入近景模式（仅能在没有在进行距离插值时进入，避免参数错误）
        if (this._targetManualDist <= CameraConfig.EnterCloseShotDist && offset < 0)
        {
            this.TransitBaseState(CameraFollowStateID.FollowCloseShotState);
            // this.SetBaseStateValid(CameraFollowStateID.FollowCloseShotState, false);
        }
        if (this._targetManualDist > CameraConfig.EnterCloseShotDist && offset > 0)
        {
            // this.SetBaseStateValid(CameraFollowStateID.FollowCloseShotState, true);
        }
        //退出近景模式
        if (
            this._curState != null &&
            this._curState.ID == CameraFollowStateID.FollowCloseShotState &&
            offset > 0 &&
            this._targetManualDist > CameraConfig.ExitCloseShotDist
        )
        {
            this.TryToTransitToOtherBaseState();
        }
    }

    /// <summary>
    /// 尝试开始手动控制旋转
    /// </summary>
    public void TryStartFollowManualControlRtotate()
    {
        if (this.active)
        {
            this.AddOrReplaceShortState(this.manualRotateState);
        }
    }

    /// <summary>
    /// 切换ShortState
    /// </summary>
    /// <param name="shortState"></param>
    public void AddOrReplaceShortState(BaseFollowShortState shortState, bool skipPreviousState = true, object param = null)
    {
        if (this._curShortState != null)
        {
            if (skipPreviousState && this._curShortState.ID != shortState.ID)
            {
                this._curShortState.Exit();
                this._curShortState = shortState;
                this._curShortState.Enter(param);

            }
        }
        if (this._curShortState == null)
        {
            this._curShortState = shortState;
            this._curShortState.Enter(param);
        }
    }

    public void TryRemoveShortState()
    {
        if (this._curShortState != null)
        {
            this._curShortState.Exit();
            this._curShortState = null;
        }
    }


    /// <summary>
    /// 切换baseState
    /// </summary>
    /// <param name="toStateId"></param>
    public void TransitBaseState(CameraFollowStateID toStateId)
    {
        BaseFollowBaseState toState = this._stateMap[toStateId];

        // 处理有效状态的情况
        bool flag = true;
        if (toState != null)
        {
            if (this._curState == null)
            {
                this._nextState = toState;
                return;
            }

            // if (!this.GetBaseStateValid(toStateId))
            // {
            //     flag = false;
            //     console.error("MainCameraFollowState.TransitBaseState(): Invalid state id.", toStateId);
            // }

            if (this._curState != null && toStateId == this._curState.ID)
            {
                flag = false;
                this._nextState = null;
            }

            if (flag)
            {
                this._nextState = toState;
                return;
            }
        }
    }

    public void TryToTransitToOtherBaseState()
    {
        CameraFollowStateID targetBaseStateId = this.GetTargetBaseStateId();

        if (targetBaseStateId != CameraFollowStateID.None)
        {
            this.TransitBaseState(targetBaseStateId);
        }
        else
        {
            if (this._curState != null)
            {
                this._curState.Exit();
                this._curState = null;
            }
        }
    }

    // 获取基础Base相机，CloseShot相机这类型虽然也为Base相机，但是为特殊Base相机
    private CameraFollowStateID GetTargetBaseStateId()
    {
        return CameraFollowStateID.None;
    }

    #region Param State
    /**
	 * 取消所有参数过渡
	 */
    public void CancelAllPramChange()
    {
        this._changeParamState.CancelAllParamChange();
    }

    public void CancelVerticalAxisLerp()
    {
        this._changeParamState.CancelVerticalAxisLerp();
    }

    public void CancelHorizontalAxisLerp()
    {
        this._changeParamState.CancelHorizontalAxisLerp();
    }

    public void CancelDistanceLerp()
    {
        this._changeParamState.CancelDistanceLerp();
    }
    /// <summary>
    /// 尝试插值设置垂直轴旋转值
    /// </summary>
    /// <param name="targetVerticalAxis"></param>
    /// <param name="durationRatio"></param>
    /// <param name="endCallback"></param>
    /// <param name="lerpMode"></param>
    /// <param name="isForce"></param>
    public void TryLerpVerticalAxisVal(float targetVerticalAxis, float durationRatio, UnityAction endCallback = null, CameraLerpMode lerpMode = CameraLerpMode.LineLerp, bool isForce = false)
    {
        targetVerticalAxis = CameraUtil.NormalizeVerticalAxis(targetVerticalAxis);

        if (Math.Abs(targetVerticalAxis - this.verticalAxisVal) > 180)
        {
            if (this.verticalAxisVal > targetVerticalAxis)
            {
                targetVerticalAxis += 360;
            }
            else
            {
                targetVerticalAxis -= 360;
            }
        }

        this._changeParamState.TryChangeVerticalAxisVal(targetVerticalAxis, durationRatio, endCallback, lerpMode, isForce);
    }

    /// <summary>
    /// 尝试插值设置水平轴旋转值
    /// </summary>
    /// <param name="targetHorizontalAxis"></param>
    /// <param name="durationRatio"></param>
    /// <param name="endCallback"></param>
    /// <param name="lerpMode"></param>
    /// <param name="isForce"></param>
    public void TryLerpHorizontalAxisVal(float targetHorizontalAxis, float durationRatio, UnityAction endCallback = null, CameraLerpMode lerpMode = CameraLerpMode.LineLerp, bool isForce = false)
    {
        this._changeParamState.TryChangeHorizontalAxisVal(targetHorizontalAxis, durationRatio, endCallback, lerpMode, isForce);
    }

    /// <summary>
    /// 尝试插值设置距离
    /// </summary>
    /// <param name="targetDist"></param>
    /// <param name="durationRatio"></param>
    /// <param name="endCallback"></param>
    /// <param name="lerpMode"></param>
    public void TryLerpDistance(float targetDist, float durationRatio, UnityAction endCallback = null, CameraLerpMode lerpMode = CameraLerpMode.LineLerp)
    {
        this._changeParamState.TryChangeDistance(targetDist, durationRatio, endCallback, lerpMode);
    }

    /// <summary>
    /// 是否完成参数过渡
    /// </summary>
    /// <returns></returns>
    public bool IsParamChangeDone()
    {
        return this._changeParamState.IsParamChangeDone();
    }
    #endregion

}