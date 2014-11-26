using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StoreBaseUnit : MonoBehaviour {

    // 가격, 수량, 추가 정보 순으로 할당.
    [SerializeField]
    protected List<UILabel> infoLabels = new List<UILabel>();
    
    // 구매를 처리하는 제품 번호.
    protected int productID = 0;
    protected string url;
    
    protected enum StoreUnitState { none, ready, wait };
    protected StoreUnitState nowState = StoreUnitState.none;

    // 초기화 여부 체크.
    public bool isReady 
    {
        get 
        {
            return (nowState != StoreUnitState.none);
        }
    }
    
    // 초기화.
    public virtual void Init(int pID, string targetUrl, 
                     string price, 
                     string amount="none", 
                     string etc="none")
    {
        if(nowState != StoreUnitState.none) return;

        productID = pID;
        url = string.Format(GameData.Instance.urlPrefix, targetUrl);

        UpdateLabels(price, amount, etc);

        nowState = StoreUnitState.ready;
    }

    protected void UpdateLabels(string price, 
                                string amount="none", 
                                string etc="none")
    {
        infoLabels[0].text = price;
        if(amount != "none") infoLabels[1].text = amount;
        if(etc != "none") infoLabels[2].text = etc;
    }

    public virtual void ClickPurchase()
    {
        if(nowState != StoreUnitState.ready) return;
        nowState = StoreUnitState.wait;
        GameData.Instance.lobbyGM.loadScreenObj.SetActive(true);
        StartCoroutine(RequestPurchase());
    }

    protected IEnumerator RequestPurchase()
    {
        WWWForm form = new WWWForm();
        int userKeyNo = PlayerPrefs.GetInt("UserKeyNo");
        form.AddField("userKeyNo", userKeyNo);
        form.AddField("pID", productID);

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
            case "none1":
                // 코인이 부족한 경우.
                nowState = StoreUnitState.ready;
                GameData.Instance.lobbyGM.loadScreenObj.SetActive(false);
                GameData.Instance.lobbyGM.PopupDialog(
                    "코인이 부족합니다.\n코인을 구매하시겠습니까?", 
                    LobbyGM.DialogType.two,
                    GameData.Instance.store.OpenCoinShop);
                break;
            case "none2":
                // 보석이 부족한 경우.
                nowState = StoreUnitState.ready;
                GameData.Instance.lobbyGM.loadScreenObj.SetActive(false);
                GameData.Instance.lobbyGM.PopupDialog(
                    "보석이 부족합니다.\n보석을 구매하시겠습니까?", 
                    LobbyGM.DialogType.two,
                    GameData.Instance.store.OpenGemShop);
                break;
            default:
                // 처리 완료.
                nowState = StoreUnitState.ready;
                GameData.Instance.lobbyGM.loadScreenObj.SetActive(false);
                GameData.Instance.lobbyGM.PopupDialog(
                    "구매가 완료되었습니다.", 
                    LobbyGM.DialogType.one);

                GameData.Instance.store.ConvertXmlToUserCoreData(www.text);
                break;
            }
        }
    }
}
