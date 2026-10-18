[English](README.md) | [日本語](README.ja.md) | [Français](README.fr.md)

# axr_runtime embedding sample — Windows (x64)

No build required. `native/axr_runtime.dll` is included.

## 1. Python (ctypes)

No extra dependencies needed (only the standard library's `ctypes`).

```bash
cd python
python render_sample.py
```

## 2. C# (.NET, P/Invoke)

Requires the .NET 10 SDK.

```bash
cd csharp
dotnet run
```

## 3. VB.NET (.NET, P/Invoke)

Requires the .NET 10 SDK.

```bash
cd vb
dotnet run
```

## 4. Java (JNA)

Requires JDK 11+ and Maven. `pom.xml` fetches `net.java.dev.jna:jna`
from Maven Central (requires network access).

```bash
cd java
mvn package
java -jar target/acr-libs-sample-jar-with-dependencies.jar
```

Python / C# / VB.NET have been verified on real hardware. The Java
code is identical to the other platforms' samples, but it hasn't been
run here because Maven isn't installed on this machine.

## License and activation

Running without activation adds a watermark to the output PDF. If you
have a license key, activate before running. Check whether the
watermark is present by opening `output.pdf`.

```bash
# cmd.exe
set ACR_LICENSE_EMAIL=you@example.com
set ACR_LICENSE_KEY=ACR-xxxx-xxxx-xxxxxxxx-xxxxxxxx
python render_sample.py
```

```powershell
# PowerShell
$env:ACR_LICENSE_EMAIL = "you@example.com"
$env:ACR_LICENSE_KEY = "ACR-xxxx-xxxx-xxxxxxxx-xxxxxxxx"
python render_sample.py
```
