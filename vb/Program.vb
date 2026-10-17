' axr_runtime（ACRのネイティブ共有ライブラリ）を VB.NET から呼び出す
' 最小サンプル。ビルドは不要です。対応プラットフォーム向けのライブラリは
' native/<rid>/ に同梱されています。
'
' 実行（未アクティベート。出力PDFにウォーターマークが入ります）:
'     cd acr-libs-samples/vb
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
    Public Function acr_version() As IntPtr
    End Function

    <DllImport(LibName, CallingConvention:=CallingConvention.Cdecl)>
    Public Function acr_license_check() As Integer
    End Function

    <DllImport(LibName, CallingConvention:=CallingConvention.Cdecl)>
    Public Function acr_license_activate(
        <MarshalAs(UnmanagedType.LPUTF8Str)> email As String,
        <MarshalAs(UnmanagedType.LPUTF8Str)> key As String) As Integer
    End Function

    <DllImport(LibName, CallingConvention:=CallingConvention.Cdecl)>
    Public Function acr_license_last_message() As IntPtr
    End Function

    <DllImport(LibName, CallingConvention:=CallingConvention.Cdecl)>
    Public Function acr_get_page_count(
        <MarshalAs(UnmanagedType.LPUTF8Str)> templateJson As String,
        <MarshalAs(UnmanagedType.LPUTF8Str)> dataJson As String) As Integer
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

    Private Function NativeLibraryFileName() As String
        If RuntimeInformation.IsOSPlatform(OSPlatform.Windows) Then
            Return "axr_runtime.dll"
        ElseIf RuntimeInformation.IsOSPlatform(OSPlatform.OSX) Then
            Return "libaxr_runtime.dylib"
        Else
            Return "libaxr_runtime.so"
        End If
    End Function

    Private Function RuntimeIdentifier() As String
        Dim isArm64 As Boolean = (RuntimeInformation.OSArchitecture = Architecture.Arm64)
        If RuntimeInformation.IsOSPlatform(OSPlatform.Windows) Then
            Return If(isArm64, "win-arm64", "win-x64")
        ElseIf RuntimeInformation.IsOSPlatform(OSPlatform.OSX) Then
            Return If(isArm64, "osx-arm64", "osx-x64")
        Else
            Return If(isArm64, "linux-arm64", "linux-x64")
        End If
    End Function

    Private Sub RegisterResolver()
        ' 同梱の native/<rid>/ からロードする（ビルド不要）。
        ' ACR_NATIVE_LIB 環境変数が設定されていれば、そのフルパスを優先する。
        Dim sampleDir As String = AppContext.BaseDirectory
        ' bin/Debug/net8.0 から見て acr-libs-samples (このリポジトリのルート) は4階層上。
        Dim samplesRoot As String = Path.GetFullPath(Path.Combine(sampleDir, "..", "..", "..", ".."))
        Dim candidate As String = Environment.GetEnvironmentVariable("ACR_NATIVE_LIB")
        If String.IsNullOrEmpty(candidate) Then
            candidate = Path.Combine(samplesRoot, "native", RuntimeIdentifier(), NativeLibraryFileName())
        End If

        NativeLibrary.SetDllImportResolver(GetType(Program).Assembly,
            Function(name As String, assembly As Reflection.Assembly, searchPath As DllImportSearchPath?) As IntPtr
                If name <> "axr_runtime" Then Return IntPtr.Zero
                If File.Exists(candidate) Then
                    Return NativeLibrary.Load(candidate)
                End If
                Throw New FileNotFoundException(
                    $"native library not found for this platform ({RuntimeIdentifier()}): {candidate}" & vbCrLf &
                    "同梱されているプラットフォームは native/ ディレクトリを確認してください。" &
                    "別の場所にあるライブラリを使う場合は環境変数 ACR_NATIVE_LIB でフルパスを指定してください。")
            End Function)
    End Sub

    Function Main() As Integer
        RegisterResolver()

        Dim version As String = Marshal.PtrToStringUTF8(Native.acr_version())
        Console.WriteLine($"axr_runtime version: {version}")

        Dim email As String = Environment.GetEnvironmentVariable("ACR_LICENSE_EMAIL")
        Dim key As String = Environment.GetEnvironmentVariable("ACR_LICENSE_KEY")
        If Not String.IsNullOrEmpty(email) AndAlso Not String.IsNullOrEmpty(key) Then
            Dim ok As Boolean = (Native.acr_license_activate(email, key) <> 0)
            Dim message As String = Marshal.PtrToStringUTF8(Native.acr_license_last_message())
            Console.WriteLine($"activation: {If(ok, "success", "failed")} ({message})")
        Else
            Console.WriteLine("activation: skipped (ACR_LICENSE_EMAIL / ACR_LICENSE_KEY not set)")
        End If

        Dim tier As Integer = Native.acr_license_check()
        Dim watermark As Boolean = (tier < 2)
        Console.WriteLine($"license tier: {tier} (0=unlicensed/watermarked, 1=DESIGNER, 2=DESIGNERPRO)")
        Console.WriteLine($"watermark: {If(watermark, "yes", "no")}")

        Dim sampleDir As String = AppContext.BaseDirectory
        ' bin/Debug/net8.0 から見て acr-libs-samples の common/ は4階層上。
        Dim commonDir As String = Path.GetFullPath(Path.Combine(sampleDir, "..", "..", "..", "..", "common"))
        Dim templateJson As String = File.ReadAllText(Path.Combine(commonDir, "template.json"), Encoding.UTF8)
        Dim dataJson As String = File.ReadAllText(Path.Combine(commonDir, "data.json"), Encoding.UTF8)

        Dim pageCount As Integer = Native.acr_get_page_count(templateJson, dataJson)
        Console.WriteLine($"page count: {pageCount}")
        If pageCount < 0 Then
            Console.Error.WriteLine("ERROR: failed to parse template/data JSON")
            Return 1
        End If

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
        Finally
            Native.acr_free_buffer(bufPtr, outLen)
        End Try

        Return 0
    End Function

End Module
