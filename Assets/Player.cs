
using StarForce;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float moveSpeed = 5f;

    private void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // 获取摄像机的前方向和右方向（忽略y分量，保证在水平面移动）
        Vector3 camForward = GameEntry.Camera.CameraTrans.forward;
        camForward.y = 0;
        camForward.Normalize();

        Vector3 camRight = GameEntry.Camera.CameraTrans.right;
        camRight.y = 0;
        camRight.Normalize();

        // 按住W永远朝向摄像机正前方
        Vector3 move = camForward * v + camRight * h;

        if (move.magnitude > 1f)
        {
            move = move.normalized;
        }

        transform.position += move * moveSpeed * Time.deltaTime;
    }

}
