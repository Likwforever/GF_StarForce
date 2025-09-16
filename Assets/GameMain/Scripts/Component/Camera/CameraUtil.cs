
using System;
using UnityEngine;

public static class CameraUtil
{
    // 最小值
    public static float Epsilon = 0.00001f;
    // 阻尼系数
    public static float KLogNegligibleResidual = -4.605170186f;

    public static float Lerp(float start, float end, float t)
    {
        if (t > 1)
        {
            t = 1;
        }
        if (t < 0)
        {
            t = 0;
        }

        return start + (end - start) * t;
    }

    // 平方函数
    public static float SquareLerp(float start, float end, float t)
    {
        t = -t * t + 2 * t;
        return CameraUtil.Lerp(start, end, t);
    }

    // 立方函数
    public static float CubeLerp(float start, float end, float t)
    {
        t = t * t * t;
        return CameraUtil.Lerp(start, end, t);
    }
    /**
	 * @description: 应用阻尼
	 * @param {number} initial 修改值
	 * @param {number} dampTime
	 * @param {number} deltaTime
	 * @return {*}
	 */
    public static float Damp(float initial, float dampTime, float deltaTime)
    {
        if (dampTime < CameraUtil.Epsilon || Math.Abs(initial) < CameraUtil.Epsilon)
        {
            return 0;
        }
        // 若deltaTime为0则取消damp阻尼
        if (deltaTime < CameraUtil.Epsilon)
        {
            return 0;
        }

        float k = -CameraUtil.KLogNegligibleResidual / dampTime;
        float result = initial * Mathf.Exp(-k * deltaTime);
        if (Math.Abs(result - initial) < CameraUtil.Epsilon)
        {
            result = 0;
        }
        return result;
    }

    /// <summary>
    /// 归一化垂直轴旋转值
    /// </summary>
    /// <param name="val"></param>
    /// <returns></returns>
    public static float NormalizeVerticalAxis(float val)
    {
        if (val > 360)
        {
            val -= Mathf.Floor(val / 360) * 360;
        }
        else
        {
            val += Mathf.Floor(-val / 360) * 360 + 360;
        }

        if (Mathf.Abs(val - 360) < Epsilon)
        {
            val = 0;
        }

        return val;
    }

    /// <summary>
    /// 归一化水平轴旋转值
    /// 返回值范围：-180 ~ 180
    /// </summary>
    /// <param name="val"></param>
    /// <returns></returns>
    public static float NormalizeHorizontalAxis(float val)
    {
        if (val > 180)
        {
            val -= 360;
        }
        return val;
    }


    /// <summary>
    /// 在单位圆内随机一个点
    /// </summary>
    /// <returns></returns>
    public static Vector2 RandomInsideUnitCircle()
    {
        // Unity中的API
        // UnityEngine.Random.insideUnitCircle.normalized;

        // 自己实现的API
        float angle = UnityEngine.Random.Range(0, 2 * Mathf.PI); // 随机角度
        float radius = Mathf.Sqrt(UnityEngine.Random.Range(0f, 1f)); // 随机半径
        float x = radius * Mathf.Cos(angle);
        float y = radius * Mathf.Sin(angle);
        Vector2 result = new Vector2(x, y);
        return result;

    }
}