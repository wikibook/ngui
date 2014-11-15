using UnityEngine;
using System.Collections;

public partial class TitleGM : MonoBehaviour {

    public GameObject progressBarObj, signupBtnObj, makeIDObj, messageBoxObj;

    public UIProgressBar progressBar;
    public UILabel inputLabel, messageTextLabel;

    void OnEnable()
    {
        // 저장된 사용자 key를 제거한다. //TODO : 반드시 출시전에 지원야한다.
        //PlayerPrefs.SetInt("UserKeyNo", 0);

        // facebook 초기화.
        FB.Init(InitDone);
    }

    void TurnOnObj(int objNo = 0)
    {
        progressBarObj.SetActive(objNo == 0);
        signupBtnObj.SetActive(objNo == 1);
        makeIDObj.SetActive(objNo == 2);
        messageBoxObj.SetActive(objNo == 3);
    }

    #region messageBox method
    /// <summary>
    /// 메시지 박스를 닫을 때 실행되는 액션..
    /// </summary>
    event System.Action SubmitMessageBox;
    
    /// <summary>
    /// 메시지 박스를 띄운다.
    /// </summary>
    /// <param name="msgText">표시해야할 내용.</param>
    /// <param name="submitAction">메시지 박스 닫을 때 실행될 메소드.</param>
    public void PopupWarningMessage(string msgText, System.Action submitAction=null)
    {
        messageTextLabel.text = msgText;
        messageBoxObj.SetActive(true);
        messageBoxObj.transform.GetChild(2).gameObject.SetActive(true);
        
        if(submitAction != null)
        {
            SubmitMessageBox = submitAction;
        }
        else
        {
            SubmitMessageBox = null;
        }
    }

    /// <summary>
    /// 버튼없는 메시지 박스를 띄운다.
    /// </summary>
    public void PopupOnlyWarningMessage(string msgText)
    {
        messageTextLabel.text = msgText;
        messageBoxObj.SetActive(true);
        messageBoxObj.transform.GetChild(2).gameObject.SetActive(false);

        SubmitMessageBox = null;
    }
    
    public void ClickCloseMessageBox()
    {
        messageBoxObj.SetActive(false);
        
        // 실행할 메소드가 있으면 실행한다.
        if(SubmitMessageBox != null)
        {
            SubmitMessageBox();
        }
    }
    #endregion

    public void ClickOpenInputID()
    {
        TurnOnObj(2);
    }

    public void ClickInputID()
    {
        // 아이디 길이를 체크한다.
        string inputID = inputLabel.text; 
        if( !(inputID.Length >= 3 && inputID.Length <= 14) )
        {
            PopupWarningMessage("아이디는 3~14글자로 입력해야합니다");
            return;
        }

        PopupOnlyWarningMessage("아이디 중복 검사중…");
        // 서버에 아이디를 전달한다.
        StartCoroutine(InputIDToServer( inputID ) );
    }

    IEnumerator InputIDToServer(string idText)
    {
        WWWForm form = new WWWForm();
        form.AddField("userID", idText);
    
        string url = string.Format(GameData.Instance.urlPrefix, "postUserID");
        WWW www = new WWW(url, form);
        
        yield return www;
        
        if( www.isDone && www.error == null)
        {
            Debug.Log(www.text);
            // 전달받은 데이터의 앞 5글자를 분리하여 결과 코드로 분석.
            string responseCode = www.text.Substring(0, 5);
            
            switch(responseCode)
            {
            case "query":
                // 서버에서 SQL 쿼리 에러가 발생한 경우.
                #if UNITY_EDITOR
                Debug.Log(www.text);
                #endif
                break;
            case "exist":
                // 아이디가 중복되는 경우.
                PopupWarningMessage("같은 아이디가 존재하여 사용할 수 없습니다");
                break;
            case "done0":
                // 아이디가 정상적으로 생성된 경우.
                messageBoxObj.SetActive(false);
                //생성된 아이디의 key를 저장.
                string splitUserKeyNo = www.text.Substring(5);
                int userKeyNo = System.Convert.ToInt32(splitUserKeyNo);
                PlayerPrefs.SetInt("UserKeyNo", userKeyNo);
                // 서버로부터 필요한 정보를 읽는 다음 단계 진행.
                TurnOnObj(0);
                LoadUserData();
                break;
            }
        }
    }


}
