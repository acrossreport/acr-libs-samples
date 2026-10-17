' axr_runtime（ACRのネイティブ共有ライブラリ、macOS (Intel/x64)版）を
' VB.NET から呼び出す最小サンプル。ビルドは不要です。
'
' 実行（未アクティベート。出力PDFにウォーターマークが入ります。
' output.pdf を開いて確認してください）:
'     cd macos-x64/vb
'     dotnet run
'
' 実行（ライセンスキーでアクティベートしてから生成）:
'     ACR_LICENSE_EMAIL=you@example.com ACR_LICENSE_KEY=ACR-xxxx-xxxx-xxxxxxxx-xxxxxxxx dotnet run

Imports System
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Text

Module Native
    Private Const LibName As String = "axr_runtime"

    <DllImport(LibName, CallingConvention:=CallingConvention.Cdecl)>
    Public Function acr_license_activate(
        <MarshalAs(UnmanagedType.LPUTF8Str)> email As String,
        <MarshalAs(UnmanagedType.LPUTF8Str)> key As String) As Integer
    End Function

    <DllImport(LibName, CallingConvention:=CallingConvention.Cdecl)>
    Public Function acr_license_last_message() As IntPtr
    End Function

    <DllImport(LibName, CallingConvention:=CallingConvention.Cdecl)>
    Public Function acr_render_pdf(
        <MarshalAs(UnmanagedType.LPUTF8Str)> templateJson As String,
        <MarshalAs(UnmanagedType.LPUTF8Str)> dataJson As String,
        <Out> ByRef outLen As Integer) As IntPtr
    End Function

    <DllImport(LibName, CallingConvention:=CallingConvention.Cdecl)>
    Public Sub acr_free_buffer(ptr As IntPtr, len As Integer)
    End Sub
End Module

Module Program

    Private Sub RegisterResolver()
        ' macos-x64/vb から見て macos-x64 (このプラットフォーム用ツリーのルート) は1階層上。
        Dim sampleDir As String = AppContext.BaseDirectory
        Dim platformRoot As String = Path.GetFullPath(Path.Combine(sampleDir, "..", "..", "..", ".."))
        Dim candidate As String = Environment.GetEnvironmentVariable("ACR_NATIVE_LIB")
        If String.IsNullOrEmpty(candidate) Then
            candidate = Path.Combine(platformRoot, "native", "libaxr_runtime.dylib")
        End If

        NativeLibrary.SetDllImportResolver(GetType(Program).Assembly,
            Function(name As String, assembly As Reflection.Assembly, searchPath As DllImportSearchPath?) As IntPtr
                If name <> "axr_runtime" Then Return IntPtr.Zero
                If File.Exists(candidate) Then
                    Return NativeLibrary.Load(candidate)
                End If
                Throw New FileNotFoundException($"native library not found: {candidate}")
            End Function)
    End Sub

    Function Main() As Integer
        RegisterResolver()

        Dim email As String = Environment.GetEnvironmentVariable("ACR_LICENSE_EMAIL")
        Dim key As String = Environment.GetEnvironmentVariable("ACR_LICENSE_KEY")
        If Not String.IsNullOrEmpty(email) AndAlso Not String.IsNullOrEmpty(key) Then
            Dim ok As Boolean = (Native.acr_license_activate(email, key) <> 0)
            Dim message As String = Marshal.PtrToStringUTF8(Native.acr_license_last_message())
            Console.WriteLine($"activation: {If(ok, "success", "failed")} ({message})")
        Else
            Console.WriteLine("activation: skipped (ACR_LICENSE_EMAIL / ACR_LICENSE_KEY not set)")
        End If

        Dim sampleDir As String = AppContext.BaseDirectory
        Dim commonDir As String = Path.GetFullPath(Path.Combine(sampleDir, "..", "..", "..", "..", "common"))
        Dim templateJson As String = File.ReadAllText(Path.Combine(commonDir, "template.json"), Encoding.UTF8)
        Dim dataJson As String = File.ReadAllText(Path.Combine(commonDir, "data.json"), Encoding.UTF8)

        Dim outLen As Integer
        Dim bufPtr As IntPtr = Native.acr_render_pdf(templateJson, dataJson, outLen)
        If bufPtr = IntPtr.Zero Then
            Console.Error.WriteLine("ERROR: acr_render_pdf returned null")
            Return 1
        End If

        Try
            Dim pdfBytes(outLen - 1) As Byte
            Marshal.Copy(bufPtr, pdfBytes, 0, outLen)
            Dim outPath As String = Path.Combine(AppContext.BaseDirectory, "output.pdf")
            File.WriteAllBytes(outPath, pdfBytes)
            Console.WriteLine($"wrote {pdfBytes.Length} bytes -> {outPath}")
            Console.WriteLine("output.pdf を開いて、ウォーターマークの有無を確認してください。")
        Finally
            Native.acr_free_buffer(bufPtr, outLen)
        End Try

        Return 0
    End Function

End Module
