using UnityEngine;
using System.Collections;

public class StoreItemUnit : StoreBaseUnit {

    [SerializeField]
    private UIToggle useToggle;
    ItemData tempItemData;

    int itemIndex = -1;
    bool saveUse = false;

    #region check use toggle
    // itemList의 index를 찾는다.
    void FindItemIndex()
    {
        if(itemIndex == -1)
        {
            for(int i=0;i<GameData.Instance.itemList.Count;++i)
            {
                if(GameData.Instance.itemList[i].itemNo == productID)
                {
                    itemIndex = i;
                    break;
                }
            }
        }
    }
    
    // 아이템 사용 여부를 변경할 때 사용.
    public void ChangeUseToggle()
    {
        FindItemIndex();
        if(itemIndex != -1)
        {
            tempItemData = GameData.Instance.itemList[itemIndex];
            if( useToggle.value != saveUse)
            {
                saveUse = useToggle.value;
                tempItemData.use = useToggle.value;
                GameData.Instance.itemList[itemIndex] = tempItemData;
            }
        }
        else
        {
            useToggle.value = false;
        }
    }
    
    // 아이템의 최초 사용 여부를 설정합니다.
    void OnEnable()
    {
        FindItemIndex();
        if(itemIndex != -1)
        {
            tempItemData = GameData.Instance.itemList[itemIndex];
            if(tempItemData.use)
            {
                saveUse = true;
                useToggle.value = true;
            }
        }
    }
    #endregion

    public override void ClickPurchase()
    {
        if(nowState != StoreUnitState.ready) return;
        nowState = StoreUnitState.wait;
        GameData.Instance.lobbyGM.loadScreenObj.SetActive(true);
        StartCoroutine(RequestBuy());

        Debug.Log(productID);
    }
    
    protected IEnumerator RequestBuy()
    {
        WWWForm form = new WWWForm();
        int userKeyNo = PlayerPrefs.GetInt("UserKeyNo");
        form.AddField("userKeyNo", userKeyNo);
        form.AddField("pID", productID);
        Debug.Log(productID);
        Debug.Log(url);
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

                int resultAmount 
                    = GameData.Instance.store.ConvertXmlToItemAmount(www.text);

                if(GameData.Instance.itemList.Exists(x=>x.itemNo == productID))
                {
                    FindItemIndex();
                    // 이미 아이템을 보유한 경우.
                    ItemData tempItemData = GameData.Instance.itemList[itemIndex];
                    tempItemData.amount = resultAmount;
                    GameData.Instance.itemList[itemIndex] = tempItemData;
                }
                else
                {
                    // 새로 아이템을 구매하게된 경우.
                    tempItemData = new ItemData();
                    tempItemData.amount = resultAmount;
                    tempItemData.itemNo = productID;
                    tempItemData.use = false;
                    GameData.Instance.itemList.Add(tempItemData);
                }

                UpdateLabels(GameData.Instance.priceDic[productID].price, 
                             resultAmount.ToString());
                break;
            }
        }
    }
}
