[English](README.md) | [日本語](README.ja.md) | [Français](README.fr.md)

# axr_runtime embedding sample — macOS (Apple Silicon / arm64)

No build required. `native/libaxr_runtime.dylib` is included.

## 1. Python (ctypes)

No extra dependencies needed (only the standard library's `ctypes`).

```bash
cd python
python3 render_sample.py
```

## 2. C# (.NET, P/Invoke)

Requires the .NET 10 SDK. You can get it with
`brew install --cask dotnet-sdk`.

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

With Homebrew, `brew install openjdk maven` gets you both the JDK and
Maven at once.

All 4 languages have been verified on real hardware (Apple Silicon
Mac, macOS 26, arm64).

## License and activation

Running without activation adds a watermark to the output PDF. If you
have a license key, activate before running. Check whether the
watermark is present by opening `output.pdf`.

```bash
ACR_LICENSE_EMAIL=you@example.com ACR_LICENSE_KEY=ACR-xxxx-xxxx-xxxxxxxx-xxxxxxxx \
    python3 render_sample.py
```
