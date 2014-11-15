using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public partial class TitleGM : MonoBehaviour {

    void InitDone()
    {
        Debug.Log("Facebook SDK 초기화 완료");

        int userKeyNo = PlayerPrefs.GetInt("UserKeyNo");
        
        if( userKeyNo > 0 )
        {
            TurnOnObj(0);

            // 사용자 정보 로딩 시작.
            LoadUserData();
        }
        else
        {
            // 가입 버튼 활성화.
            TurnOnObj(1);
        }
    }

    // 페이스북 로그인 요청 버튼.
    public void ClickLoginWithFacebook()
    {
        FB.Login(callback:LoginComplete);
    }
    
    // 로그인 완료 시 실행.
    void LoginComplete(FBResult result)
    {
        if(FB.IsLoggedIn) {
            // 페이스북에 로그인되면 페이스북 ID와 사용자 이름을 요청.
            FB.API("me?fields=id,name", Facebook.HttpMethod.GET, GetIDComplete);
        } else {
            // 로그인 실패 시 에러 출력.
            string errorMsg = string.Format("로그인 실패\nerror : {0}", result);
            PopupWarningMessage(errorMsg);
        }
    }
    
    // 페이스북 아이디와 사용자 이름 결과 처리.
    void GetIDComplete(FBResult result)
    {
        if(FB.IsLoggedIn 
           && PlayerPrefs.GetInt("UserKeyNo") == 0)
        {
            Dictionary<string, object> userInfo = 
                Facebook.MiniJSON.Json.Deserialize(result.Text) 
                    as Dictionary<string, object>;
            
            // 테스트로 사용자 이름을 출력해본다.
            Debug.Log(userInfo["name"]);

            // ID 와 사용자 이름으로 서버에 등록한다.
            PopupOnlyWarningMessage("아이디 등록중…");
            string facebookName = System.Convert.ToString( userInfo["name"]);
            string facebookID = System.Convert.ToString( userInfo["id"]);
            // 서버에 아이디를 전달한다.
            StartCoroutine(InputIDToServerWithFacebook( facebookName, facebookID ) );
        }
    }
    
    IEnumerator InputIDToServerWithFacebook(string idText, string facebookID)
    {
        WWWForm form = new WWWForm();
        form.AddField("userID", idText);
        form.AddField("facebookID", facebookID);
        
        string url = string.Format(GameData.Instance.urlPrefix, "postUserIDWithFB");
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
