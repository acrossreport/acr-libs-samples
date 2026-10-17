// axr_runtime（ACRのネイティブ共有ライブラリ）を C# / .NET から呼び出す
// 最小サンプル。ビルドは不要です。対応プラットフォーム向けのライブラリは
// native/<rid>/ に同梱されています。
//
// 実行:
//     cd acr-libs-samples/csharp
//     dotnet run

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace AcrLibsSample;

internal static class Native
{
    // acr_dll/Cargo.toml の [lib] name = "axr_runtime" が生成物のベース名。
    // DllImport に拡張子やプラットフォーム依存の "lib" プレフィックスは書かない。
    // 実際のパス解決は AppContext.SetDllImportResolver (Program.cs) で行う。
    private const string LibName = "axr_runtime";

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr acr_version();

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int acr_license_check();

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int acr_get_page_count(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string templateJson,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string dataJson);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr acr_render_pdf(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string templateJson,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string dataJson,
        out int outLen);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void acr_free_buffer(IntPtr ptr, int len);
}

internal static class Program
{
    private static string NativeLibraryFileName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return "axr_runtime.dll";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return "libaxr_runtime.dylib";
        return "libaxr_runtime.so";
    }

    private static string RuntimeIdentifier()
    {
        var isArm64 = RuntimeInformation.OSArchitecture == Architecture.Arm64;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return isArm64 ? "win-arm64" : "win-x64";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return isArm64 ? "osx-arm64" : "osx-x64";
        return isArm64 ? "linux-arm64" : "linux-x64";
    }

    private static void RegisterResolver()
    {
        // 同梱の native/<rid>/ からロードする（ビルド不要）。
        // ACR_NATIVE_LIB 環境変数が設定されていれば、そのフルパスを優先する。
        var sampleDir = AppContext.BaseDirectory;
        // bin/Debug/net8.0 から見て acr-libs-samples (このリポジトリのルート) は4階層上。
        var samplesRoot = Path.GetFullPath(Path.Combine(sampleDir, "..", "..", "..", ".."));
        var fileName = NativeLibraryFileName();
        var candidate = Environment.GetEnvironmentVariable("ACR_NATIVE_LIB")
            ?? Path.Combine(samplesRoot, "native", RuntimeIdentifier(), fileName);

        NativeLibrary.SetDllImportResolver(typeof(Program).Assembly, (name, assembly, searchPath) =>
        {
            if (name != "axr_runtime") return IntPtr.Zero;
            if (File.Exists(candidate))
            {
                return NativeLibrary.Load(candidate);
            }
            throw new FileNotFoundException(
                $"native library not found for this platform ({RuntimeIdentifier()}): {candidate}\n" +
                "同梱されているプラットフォームは native/ ディレクトリを確認してください。" +
                "別の場所にあるライブラリを使う場合は環境変数 ACR_NATIVE_LIB でフルパスを指定してください。");
        });
    }

    private static int Main()
    {
        RegisterResolver();

        var version = Marshal.PtrToStringUTF8(Native.acr_version());
        var tier = Native.acr_license_check();
        Console.WriteLine($"axr_runtime version: {version}");
        Console.WriteLine($"license tier: {tier} (0=unlicensed/watermarked, 1=DESIGNER, 2=DESIGNERPRO)");

        var sampleDir = AppContext.BaseDirectory;
        var commonDir = Path.GetFullPath(Path.Combine(sampleDir, "..", "..", "..", "..", "common"));
        var templateJson = File.ReadAllText(Path.Combine(commonDir, "template.json"), Encoding.UTF8);
        var dataJson = File.ReadAllText(Path.Combine(commonDir, "data.json"), Encoding.UTF8);

        var pageCount = Native.acr_get_page_count(templateJson, dataJson);
        Console.WriteLine($"page count: {pageCount}");
        if (pageCount < 0)
        {
            Console.Error.WriteLine("ERROR: failed to parse template/data JSON");
            return 1;
        }

        var bufPtr = Native.acr_render_pdf(templateJson, dataJson, out var outLen);
        if (bufPtr == IntPtr.Zero)
        {
            Console.Error.WriteLine("ERROR: acr_render_pdf returned null");
            return 1;
        }

        try
        {
            var pdfBytes = new byte[outLen];
            Marshal.Copy(bufPtr, pdfBytes, 0, outLen);
            var outPath = Path.Combine(AppContext.BaseDirectory, "output.pdf");
            File.WriteAllBytes(outPath, pdfBytes);
            Console.WriteLine($"wrote {pdfBytes.Length} bytes -> {outPath}");
        }
        finally
        {
            Native.acr_free_buffer(bufPtr, outLen);
        }

        return 0;
    }
}
