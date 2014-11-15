using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GoogleIAB : MonoBehaviour {
    
    // licenseKey는 반드시 올바로 입력해야한다. 
    private const string licenseKey = "이곳에 자신의 라이센스 키를 입력합니다";
    private const string AndroidJavaIABClass = "com.totu.unity.GoogleIAB";
    
    //안드로이드 자바를 제어할 때 사용할 멤버 필드.
    private AndroidJavaClass googleIABjava;
    private AndroidJavaClass IAB
    {
        get
        {
            if(googleIABjava == null)
            {
                googleIABjava = new AndroidJavaClass(AndroidJavaIABClass);
                
                //  안드로이드에서 해당 클래스를 찾지 못했을 때 에러처리. 
                if(googleIABjava == null)
                {
                    throw new MissingReferenceException(
                        string.Format("안드로이드 {0} 클래스 로딩 실패", 

                                  AndroidJavaIABClass));
                }
            }
            return googleIABjava;
        }
    }

    // 결제 완료 후 실행되는 메소드 연결.
    public System.Action<string> PurchaseCompletedManagedAction;
    public System.Action<string> PurchaseCompletedUnmanagedAction;
    public System.Action<string> PurchaseCompletedSubscriptionAction;

    //인앱 상품 ID 저장.
    [SerializeField]
    private List<string> managedProducts = new List<string>();
    [SerializeField]
    private List<string> unmanagedProducts = new List<string>();
    [SerializeField]
    private List<string> subscription = new List<string>();

    //에러 메시지.
    private Dictionary<int, string> errorMsgs 
        = new Dictionary<int, string>{
        {-1005, "결제 요청 취소"},
        {2, "결제 요청 취소"}, {3, "처리할 수 없는 API 요청"}, 
        {4, "요청한 상품 처리 불가"}, {5, "개발자 에러"}, 
        {6, "심각한 에러"}, {7, "이미 보유한 아이템"}, 
        {8, "보유하지 않은 상품"}, 
        {-4885, "잘못된 결제 요청"}, {-4886, "DB에 결제 요청 기록 없음"},
        {-4887, "DB에 없는 상품 결제 요청"}, {-4888, "DB에 가격 정보 없음"}
    };

    void Awake()
    {
        GameData.Instance.googleIAB = this;
    }

    void OnDestroy()
    {
        GameData.Instance.googleIAB = null;
    }

    // 안드로이드에 전달할 제품ID json을 생성.
    string MakeProductToJsonString()
    {
        string returnStr = "{\"managedProducts\":[";
        for(int i=0;i<managedProducts.Count;++i)
        {
            returnStr += string.Format("\"{0}\",",managedProducts[i]);
        }
        if(managedProducts.Count>0)
        {
            returnStr = returnStr.Substring(0, returnStr.Length-1);
        }
        returnStr+= "],\"unmanagedProducts\":[";
        for(int i=0;i<unmanagedProducts.Count;++i)
        {
            returnStr += string.Format("\"{0}\",",unmanagedProducts[i]);
        }
        if(unmanagedProducts.Count>0)
        {
            returnStr = returnStr.Substring(0, returnStr.Length-1);
        }
        returnStr+= "],\"subscription\":[";
        for(int i=0;i<subscription.Count;++i)
        {
            returnStr += string.Format("\"{0}\",",subscription[i]);
        }
        if(subscription.Count>0)
        {
            returnStr = returnStr.Substring(0, returnStr.Length-1);
        }
        returnStr+= "]}";
        return returnStr;
    }

    #region called from Unity
    public void Init()
    {
//        if(licenseKey.Length < 30)
//        {
//            Debug.LogError("plz enter your license key");
//            return;
//        }
        #if UNITY_ANDROID
        string productIDjson = MakeProductToJsonString();
        string test = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAtQsC+hYi2lUeyJY1UjTbmVJTo8eKjRqSQTcJtdQgh3n80jwl5MwzDQN53H46vtiQBznBd6d14tOXV5j3JKnPXQ5/3BoEzE4ljMHhgOdZLAvIgSC29rrAby3XKp1xDC/7kdJ+fyujzWacLrHRmrqZtAdvZ3ETiFyGG3rPk9WWHgyrxdZu+B+60Wiqe4Njq6k3MHNoVQOnD4scIz/BD2PEtFdXT8yh5XxX7lklJ6oYVe3WS3VjJPyextsevjIBLuDQz6Q6mvwk7zQewryaQl/1O5KKLdN99zyfYP3HDNut8OX7v/J6lpPZXJU8jyUHDYMNAi9ApXNSRdw5YKVl9xF3zwIDAQAB";
        IAB.CallStatic("Init", test, gameObject.name, 
                       PlayerPrefs.GetInt("UserKeyNo"),
                       productIDjson);
        #endif
    }
    
    public void PurchaseInappItem(string sku)
    {
        #if UNITY_ANDROID
        IAB.CallStatic("PurchaseInappItem", sku);
        #endif
    }
    #endregion
    
    #region called from Android
    // 에러 메시지 처리.
    public void Error(string errorCode)
    {
        int convertErrorCode = System.Convert.ToInt32(errorCode);
        if(errorMsgs.ContainsKey(convertErrorCode))
        {
            GameData.Instance.lobbyGM.PopupDialog(
                string.Format("에러 발생\n\n에러 내용 : {0}",
                errorMsgs[convertErrorCode]), 
                LobbyGM.DialogType.one);
        }
        else
        {
            GameData.Instance.lobbyGM.PopupDialog(
                "알 수 없는 에러가 발생했습니다", 
                LobbyGM.DialogType.one);
        }
    }

    // 결제 완료 처리.
    public void PurchaseCompletedUnmanaged(string result)
    {
        if(PurchaseCompletedUnmanagedAction != null)
        {
            PurchaseCompletedUnmanagedAction(result);
        }
    }

    public void PurchaseCompletedManaged(string result)
    {
        if(PurchaseCompletedManagedAction != null)
        {
            PurchaseCompletedManagedAction(result);
        }
    }

    public void PurchaseCompletedSubscription(string result)
    {
        if(PurchaseCompletedSubscriptionAction != null)
        {
            PurchaseCompletedSubscriptionAction(result);
        }
    }
    #endregion
}
