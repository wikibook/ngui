using UnityEngine;
using System.Collections;

public class StoreIABUnit : StoreBaseUnit {

    // 결제 아이템 sku 저장.
    string sku ="";

	public override void Init(int pID, string targetUrl, 
                              string price, 
                              string amount="none", 
                              string etc="none")
    {
        if(nowState != StoreUnitState.none) return;

        sku = GameData.Instance.inappItemList.Find(x=>x.codeNo == pID).sku;
        Debug.Log(sku);
        UpdateLabels(price, amount, etc);

        nowState = StoreUnitState.ready;
    }

    public override void ClickPurchase()
    {
        if(nowState != StoreUnitState.ready) return;

        GameData.Instance.lobbyGM.PopupDialog(
            "결제 요청 중...", LobbyGM.DialogType.none);

        GameData.Instance.googleIAB.PurchaseInappItem(sku);
    }
}
