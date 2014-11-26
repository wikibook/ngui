 using UnityEngine;
using System.Collections;

// 유저 인터페이스를 통해서 호출되는 메소드를 관리.
public partial class GamePlayManager {
    // 일시 정지 화면.
    public GameObject pauseWindow;

    public UILabel speedButtonTextLb;

    public void ClickPauseButton()
    {
        //게임을 일시 정지 시킨다.
        Time.timeScale = 0;
        //일시 정지 화면을 화면에 나타나도록 한다.
        pauseWindow.SetActive(true);
    }

    public void ClickSpeedButton()
    {
        // 게임이 정지된 상태라면 더이상 처리하지 않는다.
        if( Time.timeScale == 0.0f) return; 

        // 현재 배속을 참조하여 배속 변경.
        if(Time.timeScale == 1.0f)
        {
            Time.timeScale = 2.0f;
            speedButtonTextLb.text = "2x";
        }
        else
        {
            Time.timeScale = 1.0f;
            speedButtonTextLb.text = "1x";
        }
    }

    public void ClickPauseReloadButton()
    {
        // 중복으로 로딩되지 못하도록 한다.
        if( nowGameState == GameState.loading ) return; 
        nowGameState = GameState.loading;

        Time.timeScale = 1;
        // 씬을 다시 로딩하여 새로 게임이 시작되도록 한다.
        Application.LoadLevel("PlayScene");
    }
    
    public void ClickPausePlayButton()
    {
        // 게임 재개.
        if( speedButtonTextLb.text == "2x" )
        {
            Time.timeScale = 2.0f;
        }
        else
        {
            Time.timeScale = 1.0f;
        }

        // 일시 정지 화면을 화면에서 사라지도록 한다.
        pauseWindow.SetActive(false);
    }
    
    public void ClickPauseHomeButton()
    {
        // 다른 씬으로 전환한다.
        Application.LoadLevelAsync("LobbyScene");
    }

    // 로비씬으로 전환할 때 사용한다.
    void LoadReadyScene(bool isPrepareGame)
    {
        // isPrepareGame를 활용하여 로비씬의 준비상태를 분기할 수 있도록 한다.
        GameData.Instance.isPrepareGame = isPrepareGame;

        if(nowGameState == GameState.gameOver)
        {
            nowGameState = GameState.wait;
            StartCoroutine(SendResultToServer());
        }
    }

    public void ClickResultHomeButton()
    {
        LoadReadyScene(false);
    }

    public void ClickReGameButton ()
    {
        LoadReadyScene(true);
    }
 
    IEnumerator SendResultToServer()
    {
        string url = string.Format(GameData.Instance.urlPrefix, "putResult");
        WWWForm form = new WWWForm();
        int userKeyNo = PlayerPrefs.GetInt("UserKeyNo");
        form.AddField("userKeyNo", userKeyNo);
        form.AddField("getCoins", getCoins);
        form.AddField("score", score);
        
        WWW www = new WWW(url, form);
        
        yield return www;
        
        if( www.isDone && www.error == null)
        {
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
            case "done0":

                Debug.Log(www.text);
                //코인 및 최고 점수 갱신.
                responseCode = www.text.Substring(5, 7);
                GameData.Instance.userdata.coins
                    = System.Convert.ToInt32(responseCode);

                responseCode = www.text.Substring(12);
                GameData.Instance.userdata.highScore
                    = System.Convert.ToInt32(responseCode);

                // LobbyScene으로 전환한다.
                Application.LoadLevelAsync("LobbyScene");
                break;
            }
        }
    }
}
