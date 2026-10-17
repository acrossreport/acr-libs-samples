package com.acrossreport.sample;

import com.sun.jna.Pointer;
import com.sun.jna.ptr.IntByReference;

import java.io.IOException;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;

/**
 * axr_runtime（ACRのネイティブ共有ライブラリ）を Java から呼び出す
 * 最小サンプル。ビルドは不要です。対応プラットフォーム向けのライブラリは
 * native/<rid>/ に同梱されています。
 *
 * 実行（未アクティベート。出力PDFにウォーターマークが入ります）:
 *   cd acr-libs-samples/java
 *   mvn package
 *   java -jar target/acr-libs-samples-jar-with-dependencies.jar
 *
 * 実行（ライセンスキーでアクティベートしてから生成）:
 *   ACR_LICENSE_EMAIL=you@example.com ACR_LICENSE_KEY=ACR-xxxx-xxxx-xxxxxxxx-xxxxxxxx \
 *       java -jar target/acr-libs-samples-jar-with-dependencies.jar
 */
public final class Main {

    private Main() {
    }

    private static String nativeLibraryFileName() {
        String os = System.getProperty("os.name", "").toLowerCase();
        if (os.contains("win")) {
            return "axr_runtime.dll";
        } else if (os.contains("mac")) {
            return "libaxr_runtime.dylib";
        } else {
            return "libaxr_runtime.so";
        }
    }

    private static String runtimeIdentifier() {
        String os = System.getProperty("os.name", "").toLowerCase();
        String arch = System.getProperty("os.arch", "").toLowerCase();
        boolean isArm64 = arch.contains("aarch64") || arch.contains("arm64");
        if (os.contains("win")) {
            return isArm64 ? "win-arm64" : "win-x64";
        } else if (os.contains("mac")) {
            return isArm64 ? "osx-arm64" : "osx-x64";
        } else {
            return isArm64 ? "linux-arm64" : "linux-x64";
        }
    }

    public static void main(String[] args) throws IOException {
        // 同梱の native/<rid>/ を JNA の検索パスとして教える（ビルド不要）。
        // ACR_NATIVE_LIB 環境変数が設定されていれば、そのディレクトリを優先する。
        Path sampleDir = Paths.get("").toAbsolutePath();
        Path samplesRoot = sampleDir.resolve("..").normalize();
        String envNativeLib = System.getenv("ACR_NATIVE_LIB");
        Path nativeDir = (envNativeLib != null)
                ? Paths.get(envNativeLib)
                : samplesRoot.resolve("native").resolve(runtimeIdentifier());

        if (!Files.exists(nativeDir.resolve(nativeLibraryFileName()))) {
            System.err.println(
                    "native library not found for this platform (" + runtimeIdentifier() + "): " +
                    nativeDir.resolve(nativeLibraryFileName()) + "\n" +
                    "同梱されているプラットフォームは native/ ディレクトリを確認してください。\n" +
                    "別の場所にあるライブラリを使う場合は環境変数 ACR_NATIVE_LIB でそのディレクトリを指定してください。");
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

        Path commonDir = samplesRoot.resolve("common");
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
