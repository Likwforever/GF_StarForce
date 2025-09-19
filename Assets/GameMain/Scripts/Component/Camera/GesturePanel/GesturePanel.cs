using System;
using System.Collections;
using StarForce;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;


public class TouchInfo
{
    public int touchID;
    public float touchX;
    public float touchY;
}

public class GestureInfo
{
    public TouchInfo[] touches;
}

public class CameraRotateInfo
{
    public int touchID;
    public Vector2 lastPos;
    public Vector2 curPos;
}

public class CameraGestureInfo
{
    public bool isDoubleTouch;
    public float lastFollowDist;
    public float followFactor;
    public Vector2 rotateSpeed;
    public Vector2 rotateLerpEndTime;
    public float rotateLerpTime;
    public float rotateFactor;
    public CameraRotateInfo[] touches;
}


public class GesturePanel : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{

    #region 手势
    private GestureInfo _gestureInfo;
    private CameraGestureInfo _cameraGestureInfo;
    #endregion

    #region 摇杆
    public Player player;
    private TouchInfo _touchInfo;
    public ItemJoystick itemJoystick;
    private Vector2 _joystickTouchSize;

    // 添加摇杆持续检测相关变量
    private bool _isJoystickActive = false;
    private Vector2 _lastJoystickPosition;

    private float _nextMoveCd = 0;
    private const float DragThreshold = 30;
    #endregion

    void Start()
    {
        this.itemJoystick.SetActive(false);
        this._touchInfo = new TouchInfo() { touchID = 0, touchX = 0, touchY = 0 };
        this._joystickTouchSize = new Vector2(this.itemJoystick.touchArea.rect.width, this.itemJoystick.touchArea.rect.height);

        this._gestureInfo = new GestureInfo()
        {
            touches = new TouchInfo[2]
            {
                new TouchInfo() { touchID = 0, touchX = 0, touchY = 0 },
                new TouchInfo() { touchID = 0, touchX = 0, touchY = 0 }
            }
        };

        this._cameraGestureInfo = new CameraGestureInfo()
        {
            isDoubleTouch = false,
            lastFollowDist = 0,
            followFactor = 0.01f,
            rotateSpeed = Vector2.zero,
            rotateLerpEndTime = Vector2.zero,
            rotateLerpTime = 0.3f,
            rotateFactor = 0.1f,
            touches = new CameraRotateInfo[2]
            {
                new CameraRotateInfo()
                {
                    touchID = 0,
                    lastPos = Vector2.zero,
                    curPos = Vector2.zero
                },
                new CameraRotateInfo()
                {
                    touchID = 0,
                    lastPos = Vector2.zero,
                    curPos = Vector2.zero
                }
            }
        };
    }

    void Update()
    {
        if (this._isJoystickActive)
        {
            // 持续发送摇杆输入
            this.OnJoystickTouchMove(_lastJoystickPosition.x, _lastJoystickPosition.y, this._touchInfo.touchID);
        }

        this.UpdateCamera();
    }

    private void UpdateCamera()
    {
        float nowTime = Time.time;
        CameraRotateInfo touchInfo1 = this._cameraGestureInfo.touches[0];
        CameraRotateInfo touchInfo2 = this._cameraGestureInfo.touches[1];

        // 双指操作
        if (this._cameraGestureInfo.isDoubleTouch)
        {
            float lastTouchDist = Vector2.Distance(touchInfo1.lastPos, touchInfo2.lastPos);
            float curTouchDist = Vector2.Distance(touchInfo1.curPos, touchInfo2.curPos);

            float dist = lastTouchDist - curTouchDist;
            float offset = dist - this._cameraGestureInfo.lastFollowDist;
            this._cameraGestureInfo.lastFollowDist = dist;
            GameEntry.Camera.ChangeCameraFollowDistanceByDelta(offset * this._cameraGestureInfo.followFactor);
        }
        else
        {
            float speedX = 0;
            float speedY = 0;


            // 当结束拖拽后，对速度进行插值，模拟惯性旋转rotateLerpTime时间
            // ratio = (endTime - nowTime) / rotateLerpTime , 速度随时间衰减
            // speed = speed * ratio
            CameraGestureInfo gestureInfo = this._cameraGestureInfo;
            if (nowTime < gestureInfo.rotateLerpEndTime.x)
            {
                float ratio = (gestureInfo.rotateLerpEndTime.x - nowTime) / gestureInfo.rotateLerpTime;
                speedX = this.CameraSpeedLerp(ratio, gestureInfo.rotateSpeed.x);
            }
            if (nowTime < gestureInfo.rotateLerpEndTime.y)
            {
                float ratio = (gestureInfo.rotateLerpEndTime.y - nowTime) / gestureInfo.rotateLerpTime;
                speedY = this.CameraSpeedLerp(ratio, gestureInfo.rotateSpeed.y);
            }

            CameraRotateInfo rotateInfo = touchInfo1.touchID == 0 ? touchInfo2 : touchInfo1;
            if (rotateInfo.touchID != 0)
            {
                // 计算拖拽速度
                float dx = rotateInfo.curPos.x - rotateInfo.lastPos.x;
                float dy = rotateInfo.curPos.y - rotateInfo.lastPos.y;

                rotateInfo.lastPos.x = rotateInfo.curPos.x;
                rotateInfo.lastPos.y = rotateInfo.curPos.y;

                // 只有当拖拽速度大于惯性速度时，才进行拖拽逻辑，并刷新惯性结束时间
                if (Mathf.Abs(dx) > Mathf.Abs(speedX))
                {
                    speedX = dx;
                    gestureInfo.rotateSpeed.x = dx;
                    gestureInfo.rotateLerpEndTime.x = nowTime + gestureInfo.rotateLerpTime;
                }
                if (Mathf.Abs(dy) > Mathf.Abs(speedY))
                {
                    speedY = dy;
                    gestureInfo.rotateSpeed.y = dy;
                    gestureInfo.rotateLerpEndTime.y = nowTime + gestureInfo.rotateLerpTime;
                }
            }

            if (speedX != 0 || speedY != 0)
            {
                float factor = gestureInfo.rotateFactor;
                // 设置拖拽速度，
                // -speedY * factor 是因为y轴向上为正，但摄像机俯仰角向下为正，所以用-speedY
                // speedX * factor 是因为x轴向右为正，摄像机偏航角向右为正，所以用speedX
                GameEntry.Camera.SetManualDragControl(-speedY * factor, speedX * factor);
            }
        }
    }

    private float CameraSpeedLerp(float ratio, float speed)
    {
        return speed * ratio;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (this.IsWithinJoystickTouchRange(eventData.position.x, eventData.position.y))
        {
            this.OnJoystickTouchBegin(eventData.position.x, eventData.position.y, eventData.pointerId);
            // 开始持续检测
            this._isJoystickActive = true;
            this._lastJoystickPosition = new Vector2(eventData.position.x, eventData.position.y);
        }
        else
        {
            this.OnGestureBeginDrag(eventData.position.x, eventData.position.y, eventData.pointerId);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (this._touchInfo.touchID == eventData.pointerId)
        {
            // this.OnJoystickTouchMove(eventData.position.x, eventData.position.y, eventData.pointerId);

            // 更新摇杆位置
            this._lastJoystickPosition = new Vector2(eventData.position.x, eventData.position.y);
        }
        else
        {
            this.OnGestureDrag(eventData.position.x, eventData.position.y, eventData.pointerId);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (this._touchInfo.touchID == eventData.pointerId)
        {
            this.OnJoystickTouchEnd(eventData.position.x, eventData.position.y, eventData.pointerId);

            // 停止持续检测
            this._isJoystickActive = false;
        }
        else
        {
            this.OnGestureEndDrag(eventData.position.x, eventData.position.y, eventData.pointerId);
        }
    }

    private void OnJoystickTouchBegin(float x, float y, int touchID)
    {
        Vector2 pointer = new Vector2(x, y);

        if (this._touchInfo.touchID == 0)
        {
            this._touchInfo.touchID = touchID;
            this._touchInfo.touchX = pointer.x;
            this._touchInfo.touchY = pointer.y;

            this.itemJoystick.SetActive(true);
            this.itemJoystick.SetPosition(this._touchInfo.touchX, this._touchInfo.touchY);
        }
    }

    private void OnJoystickTouchMove(float x, float y, int touchID)
    {
        Vector2 pointer = new Vector2(x, y);

        float localPosX = pointer.x;
        float localPosY = pointer.y;
        if (this.itemJoystick.state == JoystickState.Idle)
        {
            float dx = localPosX - this._touchInfo.touchX;
            float dy = localPosY - this._touchInfo.touchY;
            float len = Mathf.Sqrt(dx * dx + dy * dy);
            //if (len >= DragThreshold)
            //{
            this.itemJoystick.state = JoystickState.Drag;
            // }

        }
        else
        {
            float dx = localPosX - this.itemJoystick.transform.position.x;
            float dy = localPosY - this.itemJoystick.transform.position.y;
            float len = Mathf.Sqrt(dx * dx + dy * dy);
            if (len > 0.001f)
            {
                this.itemJoystick.dir.x = dx / len;
                this.itemJoystick.dir.y = dy / len;
            }
            else
            {
                this.itemJoystick.dir.x = 0;
                this.itemJoystick.dir.y = 0;
            }
            this.itemJoystick.Refresh();
            this.TrySetJoystickMove(this.itemJoystick.dir.x, this.itemJoystick.dir.y, false);
        }
    }

    private void OnJoystickTouchEnd(float x, float y, int touchID)
    {
        this._touchInfo.touchID = 0;
        if (this.itemJoystick.state == JoystickState.Drag)
        {
            this.itemJoystick.state = JoystickState.Idle;
            this.TrySetJoystickMove(0, 0, true);
        }
        this.itemJoystick.Refresh();
        this.itemJoystick.SetActive(false);
    }

    private void TrySetJoystickMove(float x, float y, bool isForce)
    {
        player.SetJoystickMove(x, y);
        // if (this._nextMoveCd < Time.time || isForce)
        // {
        //     this._nextMoveCd = Time.time + 0.01f;
        //     if (player)
        //     {
        //     }
        // }
    }


    public bool IsWithinJoystickTouchRange(float x, float y)
    {
        Vector2 pointer = new Vector2(x, y);
        float touchPosX = pointer.x;
        float touchPosY = pointer.y;

        Vector2 graphPos = this.itemJoystick.touchArea.position;
        float minX = graphPos.x - this._joystickTouchSize.x * 0.5f;
        float maxX = graphPos.x + this._joystickTouchSize.x * 0.5f;
        float minY = graphPos.y - this._joystickTouchSize.y * 0.5f;
        float maxY = graphPos.y + this._joystickTouchSize.y * 0.5f;
        if (touchPosX >= minX && touchPosX <= maxX && touchPosY >= minY && touchPosY <= maxY)
        {
            return true;
        }
        return false;
    }

    #region 手势    
    private void OnGestureBeginDrag(float x, float y, int touchID)
    {
        bool isDoubleTouch = true;
        TouchInfo curTouchInfo = null;

        // 获取当前触摸点
        foreach (TouchInfo touch in this._gestureInfo.touches)
        {
            if (touch.touchID == 0)
            {
                curTouchInfo = touch;
                break;
            }
        }

        if (curTouchInfo != null)
        {
            curTouchInfo.touchID = touchID;
            curTouchInfo.touchX = x;
            curTouchInfo.touchY = y;

            // 判断是否是双指操作: 如果有一个手指的touchID为0，则不是双指操作
            foreach (TouchInfo touch in this._gestureInfo.touches)
            {
                if (touch.touchID == 0)
                {
                    isDoubleTouch = false;
                }
            }


            if (isDoubleTouch && !this._cameraGestureInfo.isDoubleTouch)
            {
                this._cameraGestureInfo.isDoubleTouch = true;
                this._cameraGestureInfo.lastFollowDist = 0;
            }

            for (int i = 0; i < this._cameraGestureInfo.touches.Length; i++)
            {
                CameraRotateInfo touchInfo = this._cameraGestureInfo.touches[i];
                if (touchInfo.touchID == 0)
                {
                    touchInfo.touchID = touchID;
                    touchInfo.lastPos.x = x;
                    touchInfo.lastPos.y = y;
                    touchInfo.curPos.x = x;
                    touchInfo.curPos.y = y;
                }
            }
        }


    }

    private void OnGestureDrag(float x, float y, int touchID)
    {
        TouchInfo curTouchInfo = null;
        foreach (TouchInfo touch in this._gestureInfo.touches)
        {
            if (touch.touchID == touchID)
            {
                curTouchInfo = touch;
                break;
            }
        }

        if (curTouchInfo != null)
        {
            curTouchInfo.touchX = x;
            curTouchInfo.touchY = y;


            for (int i = 0; i < this._cameraGestureInfo.touches.Length; i++)
            {
                CameraRotateInfo touchInfo = this._cameraGestureInfo.touches[i];
                if (touchInfo.touchID == touchID)
                {
                    touchInfo.curPos.x = x;
                    touchInfo.curPos.y = y;
                }
            }
        }
    }

    private void OnGestureEndDrag(float x, float y, int touchID)
    {
        TouchInfo curTouchInfo = null;
        bool isDoubleTouch = true;
        foreach (TouchInfo touch in this._gestureInfo.touches)
        {
            if (touch.touchID == touchID)
            {
                curTouchInfo = touch;
                isDoubleTouch = false;
            }
        }

        if (curTouchInfo != null)
        {
            curTouchInfo.touchID = 0;
            curTouchInfo.touchX = 0;
            curTouchInfo.touchY = 0;


            for (int i = 0; i < this._cameraGestureInfo.touches.Length; i++)
            {
                CameraRotateInfo touchInfo = this._cameraGestureInfo.touches[i];
                if (touchInfo.touchID == touchID)
                {
                    touchInfo.touchID = 0;
                    touchInfo.lastPos.x = 0;
                    touchInfo.lastPos.y = 0;
                    touchInfo.curPos.x = 0;
                    touchInfo.curPos.y = 0;
                }
            }
        }

        if (isDoubleTouch && this._cameraGestureInfo.isDoubleTouch)
        {
            this._cameraGestureInfo.isDoubleTouch = false;
            this._cameraGestureInfo.lastFollowDist = 0;

            for (int i = 0; i < this._cameraGestureInfo.touches.Length; i++)
            {
                CameraRotateInfo touchInfo = this._cameraGestureInfo.touches[i];
                if (touchInfo.touchID != 0)
                {
                    touchInfo.lastPos.x = touchInfo.curPos.x;
                    touchInfo.lastPos.y = touchInfo.curPos.y;
                }
            }
        }
    }
    #endregion
}