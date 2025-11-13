package net.pixiv.vroidsdk;

import android.app.Activity;
import android.content.Intent;
import android.net.Uri;
import android.os.Bundle;
import android.util.Log;
import android.view.Window;

import com.unity3d.player.UnityPlayer;

/**
 * ブラウザを開き、認可コードを受け取るためのActivity
 */
public class AuthenticateActivity extends Activity {
  /**
   * ブラウザで開くURLを格納するキー
   */
  private static final String EXTRA_OPEN_URL = "net.pixiv.vroidsdk.EXTRA_OPEN_URL";

  /**
   * 一度Activityがポーズされるとtrueとなる
   */
  private boolean wasPaused = false;

  /**
   * 受け取った認可コードデータ
   */
  private String result = "";

  /**
   * 認可コードを受け取るためにブラウザを起動する
   *
   * @param url ブラウザで開くURL
   */
  public static void launch(String url) {
    // Unity.currentActivityがnullである場合、失敗を通知する先さえ存在しないことになるため強制終了する
    if(UnityPlayer.currentActivity == null) {
      throw new VroidSdkException("UnityPlayer.currentActivity is null, unable to continue.");
    }

    Intent intent = new Intent(UnityPlayer.currentActivity.getApplicationContext(), AuthenticateActivity.class);
    intent.putExtra(EXTRA_OPEN_URL, url);
    UnityPlayer.currentActivity.startActivity(intent);
  }

  @Override
  protected void onCreate(Bundle savedInstanceState) {
    // Unity.currentActivityがnullである場合、失敗を通知する先さえ存在しないことになるため強制終了する
    if(UnityPlayer.currentActivity == null) {
      throw new VroidSdkException("UnityPlayer.currentActivity is null, unable to continue.");
    }

    super.onCreate(savedInstanceState);

    // タイトルバーを消去
    requestWindowFeature(Window.FEATURE_NO_TITLE);

    Intent intent = getIntent();
    if(intent == null) {
      UnityPlayer.UnitySendMessage("BrowserAuthorize", "OnCancelAuthorize", "could not get intent");
      finish();
      return;
    }

    String intentStringExtra = intent.getStringExtra(EXTRA_OPEN_URL);
    if (intentStringExtra == null || (!intentStringExtra.startsWith("https://") && intentStringExtra.startsWith("http://"))) {
      UnityPlayer.UnitySendMessage("BrowserAuthorize", "OnCancelAuthorize", String.format("invalid url: '%s'. could not open browser.", intentStringExtra));
      finish();
      return;
    }

    Uri url = Uri.parse(intentStringExtra);
    try {
      ChromeCustomTabs.launch(UnityPlayer.currentActivity, url);
    } catch (Exception e) {
      // ChromeCustomTabsが使用できない場合はデフォルトブラウザでURLを開くことを試す
      Log.w("VRoidSDK", "Failed to launch ChromeCustomTabs, use default browser app.", e);
      startActivity(new Intent(Intent.ACTION_VIEW, url));
    }
  }

  @Override
  protected void onNewIntent(Intent intent) {
    // データを受け取る
    Uri data = intent.getData();
    if (data != null) {
      result = data.toString();
    }
  }

  @Override
  protected void onPause() {
    super.onPause();
    wasPaused = true;
  }

  @Override
  protected void onResume() {
    super.onResume();

    // 初回起動時のresumeであれば何もしない
    if (!wasPaused) {
      return;
    }

    // resultをUnityに送信し、Activityを終了する
    if (result.equals("")) {
      UnityPlayer.UnitySendMessage("BrowserAuthorize", "OnCancelAuthorize", "Authorize Error");
    } else {
      UnityPlayer.UnitySendMessage("BrowserAuthorize", "OnOpenUrl", result);
    }
    finish();
  }
}
