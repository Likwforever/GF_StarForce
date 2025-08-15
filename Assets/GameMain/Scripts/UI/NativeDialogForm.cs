using UnityEngine;
using UnityEngine.UI;

namespace StarForce
{
    public class NativeDialogForm : MonoBehaviour
    {
        [SerializeField]
        private Text m_TitleText = null;

        [SerializeField]
        private Text m_MessageText = null;

        [SerializeField]
        private GameObject[] m_ModeObjects = null;

        [SerializeField]
        private Text[] m_ConfirmTexts = null;

        [SerializeField]
        private Text[] m_CancelTexts = null;

        private DialogParams _dialogParams = null;

        public void Init(DialogParams dialogParams)
        {
            this._dialogParams = dialogParams;

            m_TitleText.text = dialogParams.Title;
            m_MessageText.text = dialogParams.Message;

            foreach (var obj in m_ModeObjects)
            {
                obj.SetActive(false);
            }
            m_ModeObjects[dialogParams.Mode - 1].SetActive(true);

            foreach (var text in m_ConfirmTexts)
            {
                text.text = dialogParams.ConfirmText;
            }
            foreach (var text in m_CancelTexts)
            {
                text.text = dialogParams.CancelText;
            }
        }

        public void OnConfirmButtonClick()
        {
            this._dialogParams.OnClickConfirm?.Invoke(this._dialogParams.UserData);
            this.Reset();
        }

        public void OnCancelButtonClick()
        {
            this._dialogParams.OnClickCancel?.Invoke(this._dialogParams.UserData);
            this.Reset();
        }


        private void Reset()
        {
            this._dialogParams = null;
            this.gameObject.SetActive(false);
        }
    }
}