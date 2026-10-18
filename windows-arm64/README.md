[English](README.md) | [日本語](README.ja.md) | [Français](README.fr.md)

# axr_runtime embedding sample — Windows (ARM64)

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

Regarding the JDK on Windows ARM64: Eclipse Temurin has no Windows
ARM64 build in the JDK 17 line, so use **JDK 21 or later**
([Temurin 21's aarch64_windows build](https://github.com/adoptium/temurin21-binaries/releases)).
Installs via most package managers are often x64-only, which on ARM64
Windows ends up running under emulation and fails to load
`axr_runtime.dll` (ARM64-native). Check that `os.arch` reported by
`java -XshowSettings:properties -version` is `aarch64`.

All 4 languages have been verified on real hardware.

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
