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
 * 最小サンプル。
 *
 * 事前準備:
 *   cd Acr
 *   cargo build --release -p acr_dll
 *
 * 実行:
 *   cd acr-libs-samples/java
 *   mvn package
 *   java -jar target/acr-dll-sample-jar-with-dependencies.jar
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

    public static void main(String[] args) throws IOException {
        // JNA にワークスペースの target/release を検索パスとして教える。
        // 既定では acr-libs-samples の隣にある "Acr" ワークスペースを見る。
        // ACR_WORKSPACE_DIR 環境変数で Acr ワークスペースのルートを上書き可能。
        Path sampleDir = Paths.get("").toAbsolutePath();
        Path samplesRoot = sampleDir.resolve("..").normalize();
        String envWorkspace = System.getenv("ACR_WORKSPACE_DIR");
        Path acrWorkspaceRoot = (envWorkspace != null)
                ? Paths.get(envWorkspace)
                : samplesRoot.resolve("..").resolve("Acr").normalize();
        Path nativeDir = acrWorkspaceRoot.resolve("target").resolve("release");

        if (!Files.exists(nativeDir.resolve(nativeLibraryFileName()))) {
            System.err.println(
                    "native library not found: " + nativeDir.resolve(nativeLibraryFileName()) + "\n" +
                    "先に `cargo build --release -p acr_dll` を実行してください。");
            System.exit(1);
        }

        System.setProperty("jna.library.path", nativeDir.toString());
        System.setProperty("jna.encoding", "UTF-8");

        AcrRuntime lib = AcrRuntime.INSTANCE;

        String version = lib.acr_version().getString(0, "UTF-8");
        int tier = lib.acr_license_check();
        System.out.println("axr_runtime version: " + version);
        System.out.println("license tier: " + tier + " (0=unlicensed/watermarked, 1=DESIGNER, 2=DESIGNERPRO)");

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
