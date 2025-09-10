//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using UnityEngine;
using UnityGameFramework.Runtime;

namespace StarForce
{
    public class PlantTest : MonoBehaviour
    {
        [SerializeField]
        private bool m_EnablePlanting = true;

        [SerializeField]
        private KeyCode m_PlantKey = KeyCode.Space;

        private void Update()
        {
            if (!m_EnablePlanting || GameEntry.PlantManager == null)
            {
                return;
            }

            // 按空格键种植植物
            if (Input.GetKeyDown(m_PlantKey))
            {
                PlantAtMousePosition();
            }
        }

        private void PlantAtMousePosition()
        {
            // 获取鼠标在世界坐标中的位置
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Vector3 plantPosition = hit.point;
                plantPosition.y = 0f; // 确保植物在地面上

                // 尝试种植植物
                bool success = GameEntry.PlantManager.PlantAt(plantPosition);

                if (success)
                {
                    Debug.Log($"Successfully planted at {plantPosition}");
                }
                else
                {
                    Debug.Log("Failed to plant. Check money or position.");
                }
            }
        }

        private void OnGUI()
        {
            if (GameEntry.PlantManager != null)
            {
                GUI.Label(new Rect(10, 10, 300, 20), $"Money: {GameEntry.PlantManager.PlayerMoney}");
                GUI.Label(new Rect(10, 30, 300, 20), $"Plants: {GameEntry.PlantManager.PlantCount}");
                GUI.Label(new Rect(10, 50, 300, 20), $"Press {m_PlantKey} to plant");
            }
        }
    }
}
