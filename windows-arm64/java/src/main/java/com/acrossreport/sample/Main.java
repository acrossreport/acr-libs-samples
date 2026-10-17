package com.acrossreport.sample;

import com.sun.jna.Pointer;
import com.sun.jna.ptr.IntByReference;

import java.io.IOException;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;

/**
 * axr_runtime（ACRのネイティブ共有ライブラリ、Windows ARM64版）を Java
 * から呼び出す最小サンプル。ビルドは不要です。
 *
 * 実行（未アクティベート。出力PDFにウォーターマークが入ります）:
 *   cd windows-arm64/java
 *   mvn package
 *   java -jar target/acr-libs-sample-jar-with-dependencies.jar
 *
 * 実行（ライセンスキーでアクティベートしてから生成）:
 *   set ACR_LICENSE_EMAIL=you@example.com
 *   set ACR_LICENSE_KEY=ACR-xxxx-xxxx-xxxxxxxx-xxxxxxxx
 *   java -jar target/acr-libs-sample-jar-with-dependencies.jar
 */
public final class Main {

    private Main() {
    }

    public static void main(String[] args) throws IOException {
        // windows-arm64/java から見て windows-arm64 (このプラットフォーム用ツリーのルート) は1階層上。
        Path sampleDir = Paths.get("").toAbsolutePath();
        Path platformRoot = sampleDir.resolve("..").normalize();
        String envNativeLib = System.getenv("ACR_NATIVE_LIB");
        Path nativeDir = (envNativeLib != null)
                ? Paths.get(envNativeLib)
                : platformRoot.resolve("native");

        if (!Files.exists(nativeDir.resolve("axr_runtime.dll"))) {
            System.err.println("native library not found: " + nativeDir.resolve("axr_runtime.dll"));
            System.exit(1);
        }

        System.setProperty("jna.library.path", nativeDir.toString());
        System.setProperty("jna.encoding", "UTF-8");

        AcrRuntime lib = AcrRuntime.INSTANCE;

        String version = lib.acr_version().getString(0, "UTF-8");
        System.out.println("axr_runtime version: " + version);

        String email = System.getenv("ACR_LICENSE_EMAIL");
        String key = System.getenv("ACR_LICENSE_KEY");
        if (email != null && !email.isEmpty() && key != null && !key.isEmpty()) {
            boolean ok = lib.acr_license_activate(email, key) != 0;
            String message = lib.acr_license_last_message().getString(0, "UTF-8");
            System.out.println("activation: " + (ok ? "success" : "failed") + " (" + message + ")");
        } else {
            System.out.println("activation: skipped (ACR_LICENSE_EMAIL / ACR_LICENSE_KEY not set)");
        }

        int tier = lib.acr_license_check();
        boolean watermark = tier < 2;
        System.out.println("license tier: " + tier + " (0=unlicensed/watermarked, 1=DESIGNER, 2=DESIGNERPRO)");
        System.out.println("watermark: " + (watermark ? "yes" : "no"));

        Path commonDir = platformRoot.resolve("common");
        String templateJson = Files.readString(commonDir.resolve("template.json"), StandardCharsets.UTF_8);
        String dataJson = Files.readString(commonDir.resolve("data.json"), StandardCharsets.UTF_8);

        int pageCount = lib.acr_get_page_count(templateJson, dataJson);
        System.out.println("page count: " + pageCount);
        if (pageCount < 0) {
            System.err.println("ERROR: failed to parse template/data JSON");
            System.exit(1);
        }

        IntByReference outLen = new IntByReference();
        Pointer bufPtr = lib.acr_render_pdf(templateJson, dataJson, outLen);
        if (bufPtr == null) {
            System.err.println("ERROR: acr_render_pdf returned null");
            System.exit(1);
        }

        try {
            byte[] pdfBytes = bufPtr.getByteArray(0, outLen.getValue());
            Path outPath = sampleDir.resolve("output.pdf");
            Files.write(outPath, pdfBytes);
            System.out.println("wrote " + pdfBytes.length + " bytes -> " + outPath);
        } finally {
            lib.acr_free_buffer(bufPtr, outLen.getValue());
        }
    }
}
