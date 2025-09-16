
using System;
using System.Collections.Generic;
using UnityEngine;

public class ActCameraShakeEffect
{
    public float shakeDuration; // 震屏时长
    public float shakeRange; // 震屏范围
    public float shakeDirX; // 震屏方向X
    public float shakeDirY; // 震屏方向Y
    public float shakeDirZ; // 震屏方向Z
    public float disturbanceRatio; // 震屏干扰因子
    public int stepFrame; // 震屏步帧(每隔n帧重新计算震屏方向)
    public bool clearPreviousList; // 是否清除之前的震屏效果
    public Transform caster; // 震屏施法者
    public float fadeIn; // 震屏淡入
    public float fadeOut; // 震屏淡出
}

public class CameraShakeEntry
{
    public float shakeTimer; // 震屏计时器
    public float shakeDuration; // 震屏时长
    public float shakeRange; // 震屏范围
    public float shakeDirX; // 震屏方向X
    public float shakeDirY; // 震屏方向Y
    public float shakeDirZ; // 震屏方向Z
    public float disturbanceRatio; // 震屏干扰因子
    public int stepFrame; // 震屏步帧
    public int stepFrameCounter; // 震屏步帧计数器
    public Transform caster; // 震屏施法者
    public float fadeIn; // 震屏淡入
    public float fadeOut; // 震屏淡出
}

public class CameraShakeComponent
{
    private CameraMgr _owner;

    private List<CameraShakeEntry> _shakeEntryList;
    private CameraShakeEntry _largestShakeEntryThisFrame;

    // 震屏参数
    private float _shakeTimer;
    private float _shakeDuration;
    private float _shakeRange;
    private float _shakeDirX;
    private float _shakeDirY;
    private float _shakeDirZ;
    private float _disturbanceRatio;
    private int _shakeStepFrame;
    private int _shakeFrameCounter;
    private Transform _caster;
    private float _fadeIn;
    private float _fadeOut;


    private float _targetShakeDirX;
    private float _targetShakeDirY;
    private float _targetShakeDirZ;

    public CameraShakeComponent(CameraMgr owner)
    {
        this._owner = owner;
        this._shakeEntryList = new List<CameraShakeEntry>();
    }

    public void ActCameraShakeEffect(ActCameraShakeEffect shakeEffect)
    {
        // 同一帧只取震屏强的因子
        if (this._largestShakeEntryThisFrame == null || shakeEffect.shakeRange > this._largestShakeEntryThisFrame.shakeRange)
        {
            CameraShakeEntry shakeEntry = new CameraShakeEntry()
            {
                shakeTimer = shakeEffect.shakeDuration,
                shakeDuration = shakeEffect.shakeDuration,
                shakeRange = shakeEffect.shakeRange,
                shakeDirX = shakeEffect.shakeDirX,
                shakeDirY = shakeEffect.shakeDirY,
                shakeDirZ = shakeEffect.shakeDirZ,
                disturbanceRatio = shakeEffect.disturbanceRatio,
                stepFrameCounter = 1,
                stepFrame = shakeEffect.stepFrame,
                caster = shakeEffect.caster,
                fadeIn = shakeEffect.fadeIn,
                fadeOut = shakeEffect.fadeOut,
            };

            int insertIndex = -1;
            for (int i = 0; i < this._shakeEntryList.Count; i++)
            {
                if (this._shakeEntryList[i] == null)
                {
                    insertIndex = i;
                    this._shakeEntryList[i] = shakeEntry;
                    break;
                }
            }
            if (insertIndex == -1)
            {
                this._shakeEntryList.Add(shakeEntry);
                insertIndex = this._shakeEntryList.Count - 1;
            }

            if (this.SeekLargestShakeEntryIndex() == insertIndex || shakeEffect.clearPreviousList)
            {
                if (shakeEffect.clearPreviousList)
                {
                    this._shakeEntryList.Clear();
                }

                // 应用震屏因子
                this.SetupCameraShake(shakeEntry);
            }

            this._largestShakeEntryThisFrame = shakeEntry;
        }
    }

    private int SeekLargestShakeEntryIndex()
    {
        int result = -1;
        float temp = 0;
        for (int i = 0; i < this._shakeEntryList.Count; i++)
        {
            CameraShakeEntry entry = this._shakeEntryList[i];
            if (entry != null)
            {
                // 权重由幅度、剩余震屏时间决定
                float priority = entry.shakeRange * (entry.shakeTimer / entry.shakeDuration);
                if (priority > temp)
                {
                    result = i;
                    temp = priority;
                }
            }
        }
        return result;
    }

    public void SetupCameraShake(CameraShakeEntry shakeEntry)
    {
        this._fadeIn = shakeEntry.fadeIn;
        this._caster = shakeEntry.caster;
        this._fadeOut = shakeEntry.fadeOut;
        this._shakeDirX = shakeEntry.shakeDirX;
        this._shakeDirY = shakeEntry.shakeDirY;
        this._shakeDirZ = shakeEntry.shakeDirZ;
        this._shakeTimer = shakeEntry.shakeTimer;
        this._shakeRange = shakeEntry.shakeRange;
        this._shakeStepFrame = shakeEntry.stepFrame;
        this._shakeDuration = shakeEntry.shakeDuration;
        this._disturbanceRatio = shakeEntry.disturbanceRatio;
        this._shakeFrameCounter = shakeEntry.stepFrameCounter;
    }


    /// <summary>
    /// 更新震屏
    /// </summary>
    public void UpdateCameraShake()
    {
        if (this._shakeTimer > 0)
        {
            this._shakeFrameCounter--;
            if (this._shakeFrameCounter <= 0)
            {
                this._shakeFrameCounter = this._shakeStepFrame; // 重置步帧计数器

                // 干扰方向
                Vector2 temp = CameraUtil.RandomInsideUnitCircle();
                Vector3 randomShake = new Vector3(temp.x, temp.y).normalized;
                randomShake *= this._shakeRange * this._disturbanceRatio; // 干扰方向强度

                // 震屏方向
                Vector3 targetShake = new Vector3(this._shakeDirX, this._shakeDirY, this._shakeDirZ).normalized;
                targetShake *= this._shakeRange;

                // 加上扰动方向
                targetShake += randomShake;

                // 计算强度衰减
                float strength = CameraConfig.MaxShakeStrength;
                float passedTime = this._shakeDuration - this._shakeTimer;
                float remainingTime = this._shakeTimer;
                if (this._fadeIn > 0 && passedTime <= this._fadeIn)
                {
                    // 需要渐入
                    strength = Mathf.Clamp(passedTime / this._fadeIn, CameraConfig.MinShakeStrength, CameraConfig.MaxShakeStrength);
                }
                else if (this._fadeOut > 0 && remainingTime <= this._fadeOut)
                {
                    // 需要渐出
                    strength = Mathf.Clamp(remainingTime / this._fadeOut, CameraConfig.MinShakeStrength, CameraConfig.MaxShakeStrength);
                }
                else if (this._fadeIn == 0 && this._fadeOut == 0)
                {
                    // 默认衰减
                    strength = Mathf.Clamp(this._shakeTimer / this._shakeDuration, CameraConfig.MinShakeStrength, CameraConfig.MaxShakeStrength);
                }

                targetShake *= strength * strength; // 震屏方向强度

                this._targetShakeDirX = targetShake.x;
                this._targetShakeDirY = targetShake.y;
                this._targetShakeDirZ = targetShake.z;

                Vector3 pointer;
                if (this._caster)
                {
                    pointer = this._caster.TransformDirection(this._targetShakeDirX, this._targetShakeDirY, this._targetShakeDirZ);
                }
                else
                {
                    // 当震屏施法者为空时，使用相机作为震屏施法者
                    // 震屏的方向是相对于相机，需要把坐标系转换为世界坐标系
                    // 简单解释：相机向左震动，是以相机为坐标系，而相机真正的位移需要转换为世界坐标系
                    pointer = this._owner.CameraTrans.TransformDirection(this._targetShakeDirX, this._targetShakeDirY, this._targetShakeDirZ);
                }

                this._owner.CameraTrans.localPosition += pointer;
            }
        }
        this._shakeTimer -= Time.deltaTime;

        // 只要震屏列表中有震屏效果结束，就需要重新寻找最大的震屏效果
        bool flag = false;
        for (int i = 0; i < this._shakeEntryList.Count; i++)
        {
            CameraShakeEntry entry = this._shakeEntryList[i];
            if (entry != null)
            {
                entry.shakeTimer -= Time.deltaTime;
                if (entry.shakeTimer <= 0)
                {
                    this._shakeEntryList[i] = null;
                    flag = true;
                }
            }
        }

        if (flag)
        {
            int index = this.SeekLargestShakeEntryIndex();
            if (index >= 0)
            {
                this.SetupCameraShake(this._shakeEntryList[index]);
            }
        }

        this._largestShakeEntryThisFrame = null;
    }

    public void ClearAllShakeEntry()
    {
        this._shakeEntryList.Clear();
        this._largestShakeEntryThisFrame = null;
        this._shakeTimer = 0;
        this._shakeDuration = 0;
        this._shakeRange = 0;
        this._shakeDirX = 0;
        this._shakeDirY = 0;
        this._shakeDirZ = 0;
        this._disturbanceRatio = 0;
        this._shakeStepFrame = 0;
        this._shakeFrameCounter = 0;
        this._caster = null;
        this._fadeIn = 0;
        this._fadeOut = 0;
        this._targetShakeDirX = 0;
        this._targetShakeDirY = 0;
        this._targetShakeDirZ = 0;
    }
}