# axr_runtime 組み込みサンプル — Linux (ARM64)

ビルドは不要です。`native/libaxr_runtime.so` を同梱しています。

## 1. Python (ctypes)

追加の依存ライブラリは不要です（標準ライブラリの `ctypes` のみ）。

```bash
cd python
python3 render_sample.py
```

## 2. C# (.NET, P/Invoke)

.NET 8 SDK が必要です。

```bash
cd csharp
dotnet run
```

## 3. VB.NET (.NET, P/Invoke)

.NET 8 SDK が必要です。

```bash
cd vb
dotnet run
```

## 4. Java (JNA)

JDK 11+ と Maven が必要です。`pom.xml` が `net.java.dev.jna:jna` を
Maven Central から取得します（ネットワーク接続が必要）。

```bash
cd java
mvn package
java -jar target/acr-libs-sample-jar-with-dependencies.jar
```

Ubuntu/Debian なら `sudo apt install openjdk-21-jdk maven` でJDKと
Mavenがまとめて入ります。

Python / Java は実機（Ubuntu 24.04, aarch64）で動作確認済みです。
C#/VB.NET はロジック上は Windows 版と同一ですが、この環境に .NET SDK
が無く未実機確認です（`dotnet` が通常どおり動く環境であれば、そのまま
動作するはずです）。

## ライセンス・アクティベーション

未アクティベートで実行すると出力PDFにウォーターマークが入ります。
ライセンスキーがある場合は、アクティベートしてから実行してください。

```bash
ACR_LICENSE_EMAIL=you@example.com ACR_LICENSE_KEY=ACR-xxxx-xxxx-xxxxxxxx-xxxxxxxx \
    python3 render_sample.py
```
