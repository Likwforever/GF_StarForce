

using GameFramework.Fsm;
using UnityEngine;

public abstract class BaseMainCameraState : CameraState<CameraMgr>
{
    public abstract CameraStateID ID { get; }

    public abstract int Priority { get; }

    //相机位置
    public Vector3 cameraPosition;
    //相机朝向
    public Vector3 cameraForward;
    //相机旋转(绕Z轴)
    public float cameraRotation;

    //是否保存栈中上一个BaseState
    public bool saveLastBaseState = true;

    protected BaseMainCameraState(CameraMgr owner) : base(owner)
    {

        this.cameraPosition = Vector3.zero;
        this.cameraForward = Vector3.zero;
        this.cameraRotation = 0;
    }
}
