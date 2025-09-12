using StarForce;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float moveSpeed = 5f;

    void Update()
    {
        // 获取主相机
        Transform cam = GameEntry.Camera.CameraTrans;
        if (cam == null) return;

        // 获取相机的正方向和右方向（忽略y分量，只在xz平面移动）
        Vector3 camForward = cam.forward;
        camForward.y = 0;
        camForward.Normalize();

        Vector3 camRight = cam.right;
        camRight.y = 0;
        camRight.Normalize();

        // 使用Unity标准输入轴
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 moveDir = camForward * v + camRight * h;
        if (moveDir.sqrMagnitude > 0.0001f)
        {
            moveDir.Normalize();
            // 角色始终朝向移动方向
            transform.forward = moveDir;
            // 沿移动方向移动
            transform.position += moveDir * moveSpeed * Time.deltaTime;
        }
    }

}