' axr_runtime（ACRのネイティブ共有ライブラリ）を VB.NET から呼び出す
' 最小サンプル。
'
' 事前準備:
'     cd Acr
'     cargo build --release -p acr_dll
'
' 実行:
'     cd acr-libs-samples/vb
'     dotnet run

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

    Private Sub RegisterResolver()
        ' 既定では acr-libs-samples の隣にある "Acr" ワークスペースの
        ' target/release からロードする。ACR_WORKSPACE_DIR 環境変数で
        ' Acr ワークスペースのルートを上書きできる。
        Dim sampleDir As String = AppContext.BaseDirectory
        ' bin/Debug/net8.0 から見て acr-libs-samples の親(ACR_DEV)は5階層上。
        Dim samplesParent As String = Path.GetFullPath(Path.Combine(sampleDir, "..", "..", "..", "..", ".."))
        Dim acrWorkspaceRoot As String = Environment.GetEnvironmentVariable("ACR_WORKSPACE_DIR")
        If String.IsNullOrEmpty(acrWorkspaceRoot) Then
            acrWorkspaceRoot = Path.Combine(samplesParent, "Acr")
        End If
        Dim candidate As String = Path.Combine(acrWorkspaceRoot, "target", "release", NativeLibraryFileName())

        NativeLibrary.SetDllImportResolver(GetType(Program).Assembly,
            Function(name As String, assembly As Reflection.Assembly, searchPath As DllImportSearchPath?) As IntPtr
                If name <> "axr_runtime" Then Return IntPtr.Zero
                If File.Exists(candidate) Then
                    Return NativeLibrary.Load(candidate)
                End If
                Return NativeLibrary.Load(name, assembly, searchPath)
            End Function)
    End Sub

    Function Main() As Integer
        RegisterResolver()

        Dim version As String = Marshal.PtrToStringUTF8(Native.acr_version())
        Dim tier As Integer = Native.acr_license_check()
        Console.WriteLine($"axr_runtime version: {version}")
        Console.WriteLine($"license tier: {tier} (0=unlicensed/watermarked, 1=DESIGNER, 2=DESIGNERPRO)")

        Dim sampleDir As String = AppContext.BaseDirectory
        ' bin/Debug/net8.0 から見て acr-libs-samples/common は4階層上 + common。
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
