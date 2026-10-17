package com.acrossreport.sample;

import com.sun.jna.Library;
import com.sun.jna.Native;
import com.sun.jna.Pointer;
import com.sun.jna.ptr.IntByReference;

/**
 * axr_runtime（ACRのネイティブ共有ライブラリ）の FFI 境界を JNA で
 * マッピングしたもの。
 */
public interface AcrRuntime extends Library {

    AcrRuntime INSTANCE = Native.load("axr_runtime", AcrRuntime.class);

    int acr_license_activate(String email, String key);

    Pointer acr_license_last_message();

    Pointer acr_render_pdf(String templateJson, String dataJson, IntByReference outLen);

    Pointer acr_render_zip(String templateJson, String dataJson, IntByReference outLen);

    void acr_free_buffer(Pointer ptr, int len);
}
