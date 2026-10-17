# axr_runtime 組み込みサンプル — macOS (Apple Silicon / arm64)

ビルドは不要です。`native/libaxr_runtime.dylib` を同梱しています。

## 1. Python (ctypes)

追加の依存ライブラリは不要です（標準ライブラリの `ctypes` のみ）。

```bash
cd python
python3 render_sample.py
```

## 2. C# (.NET, P/Invoke)

.NET 10 SDK が必要です。`brew install --cask dotnet-sdk` で入ります。

```bash
cd csharp
dotnet run
```

## 3. VB.NET (.NET, P/Invoke)

.NET 10 SDK が必要です。

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

Homebrew なら `brew install openjdk maven` でJDKとMavenがまとめて
入ります。

4言語とも実機（Apple Silicon Mac, macOS 26, arm64）で動作確認済みです。

## ライセンス・アクティベーション

未アクティベートで実行すると出力PDFにウォーターマークが入ります。
ライセンスキーがある場合は、アクティベートしてから実行してください。
ウォーターマークの有無は `output.pdf` を開いて確認してください。

```bash
ACR_LICENSE_EMAIL=you@example.com ACR_LICENSE_KEY=ACR-xxxx-xxxx-xxxxxxxx-xxxxxxxx \
    python3 render_sample.py
```
