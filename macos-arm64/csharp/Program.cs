// axr_runtime（ACRのネイティブ共有ライブラリ、macOS ARM64 (Apple Silicon)版）を
// C# / .NET から呼び出す最小サンプル。ビルドは不要です。
//
// 実行:
//     cd macos-arm64/csharp
//     dotnet run

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace AcrLibsSample;

internal static class Native
{
    private const string LibName = "axr_runtime";

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
    private static void RegisterResolver()
    {
        // macos-arm64/csharp から見て windows-arm64 など (このプラットフォーム用ツリーのルート) は1階層上。
        var sampleDir = AppContext.BaseDirectory;
        var platformRoot = Path.GetFullPath(Path.Combine(sampleDir, "..", "..", "..", ".."));
        var candidate = Environment.GetEnvironmentVariable("ACR_NATIVE_LIB")
            ?? Path.Combine(platformRoot, "native", "libaxr_runtime.dylib");

        NativeLibrary.SetDllImportResolver(typeof(Program).Assembly, (name, assembly, searchPath) =>
        {
            if (name != "axr_runtime") return IntPtr.Zero;
            if (File.Exists(candidate))
            {
                return NativeLibrary.Load(candidate);
            }
            throw new FileNotFoundException($"native library not found: {candidate}");
        });
    }

    private static int Main()
    {
        RegisterResolver();

        var sampleDir = AppContext.BaseDirectory;
        var commonDir = Path.GetFullPath(Path.Combine(sampleDir, "..", "..", "..", "..", "common"));
        var templateJson = File.ReadAllText(Path.Combine(commonDir, "template.json"), Encoding.UTF8);
        var dataJson = File.ReadAllText(Path.Combine(commonDir, "data.json"), Encoding.UTF8);

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
