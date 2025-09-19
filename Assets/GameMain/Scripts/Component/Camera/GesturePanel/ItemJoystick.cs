using System;
using UnityEngine;

public enum JoystickState
{
    Idle,
    Drag,
}

public class ItemJoystick : MonoBehaviour
{

    public Transform center;
    public Transform inner;
    public Transform outer;
    public RectTransform touchArea;
    private JoystickState _state = JoystickState.Idle;
    public Vector2 dir;
    public JoystickState state
    {
        get => this._state;
        set => this._state = value;
    }

    public void Refresh()
    {
        bool visible = this._state == JoystickState.Drag;

        if (this.gameObject.activeSelf != visible)
        {
            this.gameObject.SetActive(visible);
        }
        if (visible)
        {
            // 检查dir是否有效
            if (Mathf.Abs(this.dir.x) > 0.001f || Mathf.Abs(this.dir.y) > 0.001f)
            {
                float angle = Mathf.Atan2(-this.dir.x, this.dir.y) * Mathf.Rad2Deg;
                this.inner.rotation = Quaternion.Euler(0, 0, angle);
                this.outer.rotation = Quaternion.Euler(0, 0, angle);
            }
            else
            {
                // 如果dir为零向量，保持默认旋转
                this.inner.rotation = Quaternion.identity;
                this.outer.rotation = Quaternion.identity;
            }
        }
    }

    public void SetActive(bool value)
    {
        this.gameObject.SetActive(value);
    }

    public void SetPosition(float touchX, float touchY)
    {
        this.transform.position = new Vector3(touchX, touchY, 0);
    }
}