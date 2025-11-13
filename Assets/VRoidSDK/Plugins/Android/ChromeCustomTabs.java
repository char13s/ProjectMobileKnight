package net.pixiv.vroidsdk;

import android.content.Context;
import android.net.Uri;

import androidx.browser.customtabs.CustomTabsIntent;

/**
 * ChromeCustomTabsでブラウザを開く
 */
public class ChromeCustomTabs {
  /**
   * ChromeCustomTabsでブラウザを開く
   * @param context ソースコンテキスト
   * @param url 開くURL
   */
  public static void launch(Context context, Uri url) {
      new CustomTabsIntent.Builder()
        .setShareState(CustomTabsIntent.SHARE_STATE_OFF)
        .build()
        .launchUrl(context, url);
  }
}
