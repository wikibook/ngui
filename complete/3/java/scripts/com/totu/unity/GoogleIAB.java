package com.totu.unity;

import android.app.Activity;
import android.content.Intent;
import android.util.Log;

import com.android.vending.billing.IabHelper;
import com.android.vending.billing.IabResult;
import com.android.vending.billing.Inventory;
import com.android.vending.billing.Purchase;
import com.loopj.android.http.AsyncHttpClient;
import com.loopj.android.http.AsyncHttpResponseHandler;
import com.loopj.android.http.RequestParams;
import com.unity3d.player.UnityPlayer;

import org.apache.http.Header;
import org.apache.http.util.EncodingUtils;
import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.util.LinkedList;

import static com.totu.unity.ProductType.managed;
import static com.totu.unity.ProductType.subscription;
import static com.totu.unity.ProductType.unmanaged;

enum ProductType {managed, unmanaged, subscription, none };

public class GoogleIAB {
    static final String TAG = "AndroidBilling";
    static String GoogleIAB_OBJECT = "GoogleIAB";
    // 자료를 주고받을 서버주소
    static final String urlPrefix = "http://192.168.0.254/";

    private static Activity saveActivity;

    static IabHelper mHelper;

    static String saveLicenseKey;
    static int saveUserKeyNo=0;
    static boolean selfDebugMode = true;

    // 제품ID 저장.
    static LinkedList<String> managedProducts;
    static LinkedList<String> unmanagedProducts;
    static LinkedList<String> subscriptionProducts;

    public static boolean isInit = false;

    // 실행중인 유니티 Activity 반환
    static Activity getUnityActivity() {
        return UnityPlayer.currentActivity;
    }

    // 초기화
    public static void Init(String licenseKey,
                            String targetGameObjectName,
                            int userKeyNo,
                            String productIDjsonStr) {
        if( mHelper != null)
        {
            Log.d(TAG, "이미 IAB helper를 할당했습니다.");
            return;
        }

        // 필요한 매개 변수 저장
        saveLicenseKey = licenseKey;
        GoogleIAB_OBJECT = targetGameObjectName;
        saveUserKeyNo = userKeyNo;

        saveActivity = getUnityActivity();

        //IabHelper 생성
        mHelper = new IabHelper(saveActivity, saveLicenseKey);
        mHelper.enableDebugLogging(selfDebugMode);

        isInit = true;

        // 제품ID를 변환하여 등록.
        try {
            JSONObject jObject = new JSONObject(productIDjsonStr);
            SetupSkus(jObject);
        } catch (JSONException e) {
            e.printStackTrace();
        }

        mHelper.startSetup(new IabHelper.OnIabSetupFinishedListener() {
            @Override
            public void onIabSetupFinished(IabResult result) {
                Log.d(TAG, "IabHelper 초기화 완료");

                // 초기화를 실패했는지 체크
                if(!result.isSuccess())
                {
                    // 유니티로 초기화실패 상황전달
                    UnityPlayer.UnitySendMessage(
                            GoogleIAB_OBJECT,
                            "Error",
                            "" + result.getResponse());
                    return;
                }

                Log.d(TAG, "초기화성공 후 소모하지않은 인앱 상품 체크");
                mHelper.queryInventoryAsync(mGotInventoryListener);
            }
        });
    }

    // 구매 요청
    public static void PurchaseInappItem(final String sku) {

        if(!isInit) {
            Log.d(TAG, "GoogleIAB가 초기화되지않았습니다");
            return;
        }

        saveActivity.runOnUiThread(new Runnable() {
            public void run() {

                AsyncHttpClient client = new AsyncHttpClient();

                String url = urlPrefix+"farmdefence/getPayloadGoogleIAB.php";
                //서버에 전달할 데이터
                RequestParams sendParams = new RequestParams();
                sendParams.put("userKeyNo", saveUserKeyNo);
                sendParams.put("sku", sku);

                client.get(url, sendParams, new AsyncHttpResponseHandler() {
                    @Override
                    public void onSuccess(int statusCode, Header[] headers,
                                          byte[] responseBody) {

                        String developerPayload = EncodingUtils.getString(
                                responseBody, 0, responseBody.length, "UTF-8") ;

                        Log.d(TAG, "서버에서 전달받은 developer payload : "
                                + developerPayload);
                        mHelper.launchPurchaseFlow(
                                saveActivity, sku, 4885,
                                mPurchaseFinishedListener,
                                developerPayload);
                    }

                    @Override
                    public void onFailure(int statusCode, Header[] headers,
                                          byte[] responseBody, Throwable error) {
                        Log.d(TAG, "payload error");
                    }
                });
            }
        });
    }

    // 상품정보를 등록
    static void SetupSkus(JSONObject jObject) {
        managedProducts = new LinkedList<String>();
        unmanagedProducts  = new LinkedList<String>();
        subscriptionProducts  = new LinkedList<String>();
        try {
            JSONArray tempJArray = jObject.getJSONArray("managedProducts");


            for (int i = 0; i < tempJArray.length(); ++i) {
                try {
                    managedProducts.addLast(tempJArray.getString(i));
                } catch (JSONException e1) {
                    e1.printStackTrace();
                }
            }

            tempJArray = jObject.getJSONArray("unmanagedProducts");
            for(int i=0;i<tempJArray.length();++i) {
                try {
                    unmanagedProducts.addLast(tempJArray.getString(i));
                } catch (JSONException e1) {
                    e1.printStackTrace();
                }
            }
            tempJArray = jObject.getJSONArray("subscription");
            for(int i=0;i<tempJArray.length();++i) {
                try {
                    subscriptionProducts.addLast(tempJArray.getString(i));
                } catch (JSONException e1) {
                    e1.printStackTrace();
                }
            }
        } catch (JSONException e) {
            e.printStackTrace();
        }
    }

    // 결제된 상품이 어떤형식의 제품인지 체크.
    static ProductType CheckPurchaseItem(String purchasedSku) {
        // 관리되는상품
        if( managedProducts != null ) {
            for(String nowProduct : managedProducts) {
                if(nowProduct.equals(purchasedSku)) {
                    return managed;
                }
            }
        }

        //관리되지않는상품
        if( unmanagedProducts != null ) {
            for(String nowProduct : unmanagedProducts) {
                if(nowProduct.equals(purchasedSku)) {
                    return unmanaged;
                }
            }
        }

        //구독상품
        if( subscriptionProducts != null ) {
            for(String nowProduct : subscriptionProducts) {
                if(nowProduct.equals(purchasedSku)) {
                    return subscription;
                }
            }
        }

        return ProductType.none;
    }

    // 결제결과를 전송받는다
    public static void onActivityResult(
            Activity activity,
            int requestCode,
            int resultCode,
            Intent data) {

        if( selfDebugMode )
        {
            Log.d(TAG, "onActivityResult(" + requestCode + ","
                    + resultCode + "," + data);
        }

        if (!mHelper.handleActivityResult(requestCode, resultCode, data)) {

        }
        else {
            Log.d(TAG, "onActivityResult handled by IABUtil.");
        }
    }

    // 결제가 완료되면 호출된다
    static IabHelper.OnIabPurchaseFinishedListener mPurchaseFinishedListener
            = new IabHelper.OnIabPurchaseFinishedListener() {
        public void onIabPurchaseFinished(IabResult result, Purchase purchase) {
            if( selfDebugMode )
            {
                Log.d(TAG, "Purchase finished: " + result
                        + ", purchase: " + purchase);
            }
            if (result.isFailure()) {
                Log.d(TAG, "결제 에러");
                UnityPlayer.UnitySendMessage(
                        GoogleIAB_OBJECT,
                        "Error",
                        "" + result.getResponse());
                return;
            }
            CheckVerifyDeveloperPayload(purchase);
        }
    };

    // 결제를 통해서가지고 있는 제품(관리되는 상품, 구독 상품)과 관리지되않는 상품 중아직소모되지않은 상품을 정리한다.
    static IabHelper.QueryInventoryFinishedListener mGotInventoryListener
            = new IabHelper.QueryInventoryFinishedListener() {
        public void onQueryInventoryFinished(IabResult result, Inventory inventory) {
            Log.d(TAG, "인벤토리 상품 체크 완료");
            if (result.isFailure()) {
                UnityPlayer.UnitySendMessage(
                        GoogleIAB_OBJECT,
                        "Error",
                        "" + result.getResponse());
                return;
            }

            Log.d(TAG, "인벤토리 상품 체크 성공");

            //관리되는 상품 체크.
            if(managedProducts != null) {
                for(String productID : managedProducts) {
                    if(inventory.hasPurchase(productID)) {
                        Purchase managedProduct
                                = inventory.getPurchase(productID);

                        // 상품 검증
                        CheckVerifyDeveloperPayload(managedProduct);
                    }
                }
            }

            //구독 상품 체크
            if(subscriptionProducts != null) {
                for(String productID : subscriptionProducts) {
                    if(inventory.hasPurchase(productID)) {
                        Purchase subscriptionProduct
                                = inventory.getPurchase(productID);
                        // 상품 검증
                        CheckVerifyDeveloperPayload(subscriptionProduct);
                    }
                }
            }

            //관리되지않는 상품 체크.
            if(unmanagedProducts != null) {
                for(String productID : unmanagedProducts) {
                    Purchase unmanagedProduct
                            = inventory.getPurchase(productID);
                    // 상품 소모
                    if (unmanagedProduct != null) {
                        mHelper.consumeAsync(
                                unmanagedProduct,
                                mConsumeFinishedListener);
                    }
                }
            }

            Log.d(TAG, "인벤토리 상품 처리 종료");
        }
    };

    // 소모가 완료되면 호출된다
    static IabHelper.OnConsumeFinishedListener mConsumeFinishedListener
            = new IabHelper.OnConsumeFinishedListener() {
        public void onConsumeFinished(final Purchase purchase, IabResult result) {
            if( selfDebugMode )
            {
                Log.d(TAG, "소모완료. Purchase: " + purchase + ", result: " + result);
            }
            if (result.isSuccess()) {
                AsyncHttpClient client = new AsyncHttpClient();
                //서버에 전달할 데이터
                RequestParams sendParams = new RequestParams();
                sendParams.put("userKeyNo", saveUserKeyNo);
                sendParams.put("sku", purchase.getSku());
                sendParams.put("payload", purchase.getDeveloperPayload());

                String url = urlPrefix+"farmdefence/getPurchaseResultGoogleIAB.php";
                client.get(url, sendParams, new AsyncHttpResponseHandler() {
                    @Override
                    public void onSuccess(int statusCode, Header[] headers,
                                          byte[] responseBody) {
                        String result = EncodingUtils.getString(
                                responseBody, 0, responseBody.length,
                                "UTF-8") ;
                        Log.d(TAG, "purchase result : "+result);
                        if(result.equals("noResult")) {
                            // 결제한 정보가존재하지않는경우.
                            Log.d(TAG, "DB에 결제한 정보가 없습니다");
                            UnityPlayer.UnitySendMessage(
                                    GoogleIAB_OBJECT,
                                    "Error",
                                    "-4886");
                        } else if(result.equals("noSkuInfo")) {
                            //sku가  database 에 등록되지않은경우.
                            Log.d(TAG, " DB에 sku가 등록되않았습니다");
                            UnityPlayer.UnitySendMessage(
                                    GoogleIAB_OBJECT,
                                    "Error",
                                    "-4887");
                        } else if(result.equals("noPriceInfo")) {
                            // 가격 정보가등록되지 않은 경우.
                            Log.d(TAG, "가격정보가 DB에 등록되지 않았습니다");
                            UnityPlayer.UnitySendMessage(
                                    GoogleIAB_OBJECT,
                                    "Error",
                                    "-4888");
                        } else {
                            // 정상적으로 처리된 경우.
                            UnityPlayer.UnitySendMessage(
                                    GoogleIAB_OBJECT,
                                    "PurchaseCompletedUnmanaged",
                                    result);
                            Log.d(TAG, "소모 성공");
                        }
                    }

                    @Override
                    public void onFailure(int statusCode, Header[] headers,
                                          byte[] responseBody, Throwable error) {
                        Log.d(TAG, "consume error");
                    }
                });
            }
            else {
                //소모 실패한 경우
                UnityPlayer.UnitySendMessage(
                        GoogleIAB_OBJECT,
                        "Error",
                        "" + result.getResponse());
            }
            Log.d(TAG, "소모 과정 완료");
        }
    };

    // 서버에 올바른 결제인지 검증요청 후 결과 기준으로 이후 과정처러
    static void CheckVerifyDeveloperPayload(final Purchase p) {
        AsyncHttpClient client = new AsyncHttpClient();
        String url = urlPrefix+"farmdefence/verificationGoogleIAB.php";
        //서버에 전달할 데이터
        RequestParams sendParams = new RequestParams();
        sendParams.put("userKeyNo", saveUserKeyNo);
        sendParams.put("orderId", p.getOrderId());
        sendParams.put("packageName", p.getPackageName());
        sendParams.put("productId", p.getSku());
        sendParams.put("purchaseTime", p.getPurchaseTime());
        sendParams.put("purchaseState", p.getPurchaseState());
        sendParams.put("developerPayload", p.getDeveloperPayload());
        sendParams.put("purchaseToken", p.getToken());

        client.post(url, sendParams, new AsyncHttpResponseHandler() {
            @Override
            public void onSuccess(
                    int statusCode, Header[] headers,
                    byte[] responseBody) {
                String result = EncodingUtils.getString(
                        responseBody, 0, responseBody.length,
                        "UTF-8") ;

                Log.d(TAG, "verify return : " + result);

                //결제가 실패한 경우.
                if(result.equals("0")) {
                    UnityPlayer.UnitySendMessage(
                            GoogleIAB_OBJECT,
                            "Error", "-4885");
                    return;
                }


                // 결제된 상품타입 판단.
                String productID = p.getSku();
                ProductType nowPurchaseItemType = CheckPurchaseItem(productID);
                // 상품타입별 처리.
                switch(nowPurchaseItemType) {
                    case managed:
                        UnityPlayer.UnitySendMessage(
                                GoogleIAB_OBJECT,
                                "PurchaseCompletedManaged",
                                productID); //전달하는 정보는 자신의 프로젝트에 알맞게 처리합니다.
                        break;
                    case subscription:
                        UnityPlayer.UnitySendMessage(
                                GoogleIAB_OBJECT,
                                "PurchaseCompletedSubscription",
                                productID); //전달하는 정보는 자신의 프로젝트에 알맞게 처리합니다.
                        break;
                    case unmanaged:
                        mHelper.consumeAsync(p, mConsumeFinishedListener);
                        break;

                }

            }

            @Override
            public void onFailure(
                    int statusCode, Header[] headers,
                    byte[] responseBody, Throwable error) {
                Log.d(TAG, "verification error");
            }
        });
    }
}
