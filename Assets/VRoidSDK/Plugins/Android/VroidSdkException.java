package net.pixiv.vroidsdk;

/**
 * VRoidSDKによる進行不能な例外
 */
public class VroidSdkException extends RuntimeException {
  public VroidSdkException(String message) {
    super(message);
  }
}
