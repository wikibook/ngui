using UnityEngine;
using System.Collections;

public partial class LobbyGM : MonoBehaviour {

    public enum DialogType { none, one, two };

    event System.Action Submit;

    // 경고창 관련 게임 오브젝트.
    public GameObject dialogRootObj, 
    closeButtonObj, submitButtonObj, submitButtonObj1;
    public UILabel messageTextLabel;
    
    /// <summary>
    /// 경고창을 띄운다.
    /// </summary>
    /// <param name="msgText"> 표시해야할 내용. </param>
    /// <param name="dialogType"> 버튼 숫자 결정 </param>
    /// <param name="submitAction"> 경고창 실행 버튼 클릭 시 실행될 메소드. </param>
    public void PopupDialog(
        string msgText, 
        DialogType dialogType=DialogType.two, 
        System.Action submitAction=null)
    {
        messageTextLabel.text = msgText;
        dialogRootObj.SetActive(true);
        
        switch(dialogType)
        {
        case DialogType.none:
            closeButtonObj.transform.parent.gameObject.SetActive(false);
            break;
        case DialogType.one:
            closeButtonObj.transform.parent.gameObject.SetActive(true);
            closeButtonObj.SetActive(false);
            submitButtonObj.SetActive(false);
            submitButtonObj1.SetActive(true);
            break;
        case DialogType.two:
            closeButtonObj.transform.parent.gameObject.SetActive(true);
            closeButtonObj.SetActive(true);
            submitButtonObj.SetActive(true);
            submitButtonObj1.SetActive(false);
            break;
        }
        
        if(submitAction != null)
        {
            Submit = submitAction;
        }
        else
        {
            Submit = null;
        }
    }
    
    // 경고창 실행 버튼 클릭 시 작동.
    public void ClickSubmitDialog()
    {
        dialogRootObj.SetActive(false);
        
        if(Submit != null)
        {
            Submit();
        }
    }
    
    // 경고창 닫기 버튼 클릭 시 작동.
    public void ClickCloseDialog()
    {
        dialogRootObj.SetActive(false);

        if(Submit != null)
        {
            Submit = null;
        }
    }
}
