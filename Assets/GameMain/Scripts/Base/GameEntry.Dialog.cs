//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using UnityEngine;

namespace StarForce
{
    /// <summary>
    /// 游戏入口。
    /// </summary>
    public partial class GameEntry : MonoBehaviour
    {
        private static NativeDialogForm m_DialogFormTemplate;

        private static NativeDialogForm m_DialogFormInstance;


        public static void OpenDialog(DialogParams dialogParams)
        {
            if (m_DialogFormTemplate == null)
            {
                m_DialogFormTemplate = Resources.Load<NativeDialogForm>("UI/NativeDialogForm");
            }
            if (m_DialogFormInstance == null)
            {
                m_DialogFormInstance = Object.Instantiate(m_DialogFormTemplate);
            }

            m_DialogFormInstance.Init(dialogParams);
            m_DialogFormInstance.gameObject.SetActive(true);
        }
    }
}
