# axr_runtime 組み込みサンプル — Windows (x64)

ビルドは不要です。`native/axr_runtime.dll` を同梱しています。

## 1. Python (ctypes)

追加の依存ライブラリは不要です（標準ライブラリの `ctypes` のみ）。

```bash
cd python
python render_sample.py
```

## 2. C# (.NET, P/Invoke)

.NET 10 SDK が必要です。

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

Python / C# / VB.NET は実機で動作確認済みです。Java はコードは他プラット
フォームと同一のサンプルですが、この端末には Maven が無いため未実行です。

## ライセンス・アクティベーション

未アクティベートで実行すると出力PDFにウォーターマークが入ります。
ライセンスキーがある場合は、アクティベートしてから実行してください。
ウォーターマークの有無は `output.pdf` を開いて確認してください。

```bash
# cmd.exe の場合
set ACR_LICENSE_EMAIL=you@example.com
set ACR_LICENSE_KEY=ACR-xxxx-xxxx-xxxxxxxx-xxxxxxxx
python render_sample.py
```

```powershell
# PowerShell の場合
$env:ACR_LICENSE_EMAIL = "you@example.com"
$env:ACR_LICENSE_KEY = "ACR-xxxx-xxxx-xxxxxxxx-xxxxxxxx"
python render_sample.py
```
