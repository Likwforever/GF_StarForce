

using StarForce;
using UnityEngine;

/// <summary>
/// 跟随角色寻路状态
/// 当角色寻路时，相机跟随角色寻路，并且朝向目标点或目标Npc
/// </summary>
public class FollowAvatarPathFindingState : BaseFollowBaseState
{
    private bool _needRecenter = false; // 是否需要回正
    private Vector3 _targetPos; // 目标点
    private Vector3 _cameraForward; // 相机朝向
    private Vector3 _recenterForward; // 重置朝向

    private Quaternion _tempQuater; // 临时四元数
    private Quaternion _finalQuater; // 最终四元数
    private Vector3 _lookDirection; // 朝向

    private float _recenterSpeed = 30f; // 重置速度

    private float _startCondition = CameraConfig.PathFindingStateBeginCondition; // 开始重置条件
    private float _finalHorizontalAxisVal = CameraConfig.PathFindingStateTargetHorizontalAxis; // 最终水平轴旋转值
    private float _finalVerticalAxisValOffset = CameraConfig.PathFindingStateTargetVerticalAxisOffset; // 最终垂直轴旋转值偏移
    public FollowAvatarPathFindingState(MainCameraFollowState owner) : base(owner)
    {
    }

    public override CameraFollowStateID ID => CameraFollowStateID.FollowAvatarPathFindingState;

    public override int Priority => 1;

    public override bool cannotBeSkipped => false;

    public override void Enter(object param = null)
    {
        base.Enter(param);

        this._targetPos = GameEntry.Camera.FindTargetPos();
    }

    public override void Update()
    {
        base.Update();

        this._cameraForward = new Vector3(this._cameraForward.x, 0, this._cameraForward.z);
        this._cameraForward.Normalize();

        this._recenterForward = this._targetPos - this._owner.followPos;
        this._recenterForward.y = 0;
        this._recenterForward.Normalize();

        if (this._recenterForward.magnitude <= 0.1f) return;

        float finalAngle = this._finalVerticalAxisValOffset;
        // 计算叉积
        Vector3 cross = Vector3.Cross(this._recenterForward, this._cameraForward);
        // 相机在人物左边
        if (cross.y < 0)
        {
            finalAngle = this._finalVerticalAxisValOffset * -1;
        }
        // 记录目标角度的欧拉角
        this._tempQuater = Quaternion.Euler(0, finalAngle, 0);

        this._lookDirection = this._recenterForward;
        this._lookDirection = this._tempQuater * this._lookDirection;
        this._lookDirection.Normalize();

        // 计算最终四元数
        this._finalQuater = Quaternion.LookRotation(this._lookDirection);
        float finalVerticalAxisVal = this._finalQuater.eulerAngles.y;

        // 记录当前相机欧拉角
        float curEulerY = this._owner.verticalAxisVal;

        float speed = Mathf.Min(this._recenterSpeed * Time.deltaTime, Mathf.Abs(curEulerY - finalVerticalAxisVal));

        this._lookDirection.y = this._owner.cameraForward.y;
        this._lookDirection.Normalize();

        // 判断顺逆时针旋转
        float offsetEulerY = finalVerticalAxisVal - curEulerY;
        if (offsetEulerY > 180)
        {
            offsetEulerY -= 360;
        }
        else if (offsetEulerY < -180)
        {
            offsetEulerY += 360;
        }
        if (offsetEulerY < 0)
        {
            speed *= -1;
        }

        if (Mathf.Abs(offsetEulerY) > this._startCondition && !this._needRecenter)
        {
            this._needRecenter = true;
        }
        if (Mathf.Abs(curEulerY - finalVerticalAxisVal) < 0.001)
        {
            this._needRecenter = false;
        }

        // 开始回正
        if (this._needRecenter)
        {
            curEulerY += speed;
        }

        float rotateX = CameraUtil.Lerp(this._owner.horizontalAxisVal, this._finalHorizontalAxisVal, 0.02f);
        this._owner.SetAixsVal(rotateX, curEulerY);
    }
}