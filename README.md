# axr_runtime 組み込みサンプル (C# / VB.NET / Python / Java)

`axr_runtime`（Windowsでは `axr_runtime.dll`、macOSでは
`libaxr_runtime.dylib`、Linuxでは `libaxr_runtime.so` というファイル名に
なります）は、Avalonia デザイナー向けに作られた C ABI のネイティブ共有
ライブラリです。`@acrossreport/acr-node`（napi-rs 製 Node.js アドオン）が
Node.js から ACR を組み込むサンプルであるのに対し、ここでは**同じ C FFI
を、C# / VB.NET / Python / Java の4言語から直接呼び出す**最小サンプルを
置いています。

**ビルドは不要です。** 対応プラットフォーム向けのビルド済みライブラリを
`native/<rid>/` に同梱しています。利用者は `Acr` ソース一式やRustの
ビルド環境を用意する必要はありません。

```
acr-libs-samples/
├── common/          # 共通のテンプレートJSON / データJSON
├── native/          # ビルド済みネイティブライブラリ（プラットフォーム別）
│   ├── win-arm64/axr_runtime.dll
│   └── linux-arm64/libaxr_runtime.so
├── python/          # ctypes 版
├── csharp/          # .NET (C#) 版、P/Invoke
├── vb/              # .NET (VB.NET) 版、P/Invoke
└── java/            # JNA 版
```

## 対応プラットフォーム（同梱済み）

| RID | 状態 |
|---|---|
| `win-arm64` | ✅ 同梱済み・動作確認済み |
| `linux-arm64` | ✅ 同梱済み・動作確認済み |
| `win-x64` | 未同梱（ビルドでき次第追加予定） |
| `osx-arm64` / `osx-x64` | 未同梱（ビルドでき次第追加予定） |
| `linux-x64` | 未同梱（ビルドでき次第追加予定） |

`native/` に無いプラットフォームで動かす場合は、環境変数
`ACR_NATIVE_LIB` でライブラリの場所を指定してください
（Python/C#/VB.NET はライブラリファイルへのフルパス、Java は
ライブラリファイルを含むディレクトリのパス）。ライブラリ自体は
`Acr` ワークスペース側（このリポジトリの管理者）でビルドして提供します。
利用者側でRustをビルドする必要はありません。

## 公開しているAPI

| 関数 | 説明 |
|---|---|
| `acr_version()` | ライブラリのバージョン文字列を返す |
| `acr_license_check()` | ローカルキャッシュのライセンス状態を返す（0=未認証/ウォーターマーク付き, 1=DESIGNER, 2=DESIGNERPRO） |
| `acr_license_activate(email, key)` | ライセンスサーバーに問い合わせて認証（ネットワーク必須） |
| `acr_get_page_count(templateJson, dataJson)` | レンダリング前にページ数だけ取得 |
| `acr_render_pdf(templateJson, dataJson, &outLen)` | PDFバイト列を生成 |
| `acr_render_page_png(templateJson, dataJson, pageIndex, &outLen)` | 指定ページのPNGバイト列を生成 |
| `acr_render_zip(templateJson, dataJson, &outLen)` | 全ページPNG入りZIP（ACR-PNG-PACKAGE）を生成 |
| `acr_free_buffer(ptr, len)` | 上記が返したバッファを解放（呼び出し側は必ず呼ぶこと） |

各サンプルはこのうち `acr_version` / `acr_license_check` /
`acr_get_page_count` / `acr_render_pdf` / `acr_free_buffer` を使い、
`common/template.json` + `common/data.json` から `output.pdf` を1本
生成するだけの、最小構成になっています。

## 1. Python (ctypes)

追加の依存ライブラリは不要です（標準ライブラリの `ctypes` のみ）。

```bash
cd acr-libs-samples/python
python render_sample.py
```

## 2. C# (.NET, P/Invoke)

.NET 8 SDK が必要です。Windows / macOS / Linux のいずれでも動作します。

```bash
cd acr-libs-samples/csharp
dotnet run
```

## 3. VB.NET (.NET, P/Invoke)

.NET 8 SDK が必要です。

```bash
cd acr-libs-samples/vb
dotnet run
```

## 4. Java (JNA)

JDK 11+ と Maven が必要です。`pom.xml` が `net.java.dev.jna:jna` を
Maven Central から取得します（ネットワーク接続が必要）。

```bash
cd acr-libs-samples/java
mvn package
java -jar target/acr-libs-samples-jar-with-dependencies.jar
```

Python / C# / VB.NET / Java すべて、`native/` 同梱のライブラリから実際に
`output.pdf` の生成まで確認済みです（Windows ARM64, Linux ARM64(WSL2)）。

### JDK のインストールについて（プラットフォーム / CPUアーキテクチャ別の注意）

`axr_runtime` はネイティブライブラリなので、**JVM 自体の CPU
アーキテクチャと `axr_runtime` のビルドが一致していないと読み込めません**
（`UnsatisfiedLinkError` / "not a valid Win32 application" のようなエラー
になります）。`java -XshowSettings:properties -version` の `os.arch` を
確認してください。

| OS / CPU | 対応するJDK |
|---|---|
| Windows x64 | 多くのディストリビューションで提供（例: [Eclipse Temurin](https://adoptium.net/) 17/21 の x64 版） |
| **Windows ARM64** | **Eclipse Temurin は JDK 17 系に Windows ARM64 ビルドが無い。JDK 21 以降を使うこと**（[Temurin 21 の aarch64_windows 版](https://github.com/adoptium/temurin21-binaries/releases)）。各種パッケージマネージャ経由のインストールはx64版しか無いことが多く、ARM64 Windows ではエミュレーションで動いてしまい `axr_runtime.dll`（ARM64ネイティブ）を読み込めない |
| macOS (Intel / Apple Silicon) | [Eclipse Temurin](https://adoptium.net/) の `aarch64_mac` / `x64_mac` 版 |
| Linux (x64 / ARM64) | [Eclipse Temurin](https://adoptium.net/) の `x64_linux` / `aarch64_linux` 版、または各ディストリのパッケージ（Ubuntu/Debianなら `sudo apt install openjdk-21-jdk maven` でJDKとMavenがまとめて入る） |

Maven がパッケージマネージャに無い場合は、
https://maven.apache.org/download.cgi からバイナリZIPを取得して展開し、
`bin/` を PATH に追加してください（SHA-512 チェックサムでの検証を推奨）。

## 注意点（組み込み時に意識すること）

- `acr_render_pdf` / `acr_render_page_png` / `acr_render_zip` が返す
  バッファは Rust 側で `Box::into_raw` されたものです。使い終わったら
  必ず `acr_free_buffer(ptr, len)` を呼んでください。呼ばないとメモリ
  リークします（プロセス終了時にOSが回収しますが、長時間稼働する
  サーバープロセスに組み込む場合は必須です）。
- `acr_license_check()` が `0`（未認証）または `1`（DESIGNER）のときは
  出力にウォーターマークが入ります。`2`（DESIGNERPRO）のみ透かし無し
  になります。ライセンスキーを持っている場合は `acr_license_activate`
  を呼んで有効化してください。
- テンプレートJSONは最低限、`ACR.Report.Sections.Section` の配列に
  **「Detail」という名前に正規化されるセクションを1つだけ**含む必要が
  あります（`acr_core/src/layout/engine.rs` の `assert!` で強制されて
  います）。`common/template.json` はその最小形です。

## （リポジトリ管理者向け）ネイティブライブラリの追加・更新

`native/<rid>/` のライブラリは `Acr` ワークスペース側でビルドして、この
リポジトリにコピーします。利用者側はビルド不要です。

```bash
# Acr ワークスペースのルートで
cargo build --release -p acr_dll
# 生成物を native/<rid>/ にコピー（例: Windows ARM64）
cp target/release/axr_runtime.dll ../acr-libs-samples/native/win-arm64/
```

RID の命名は `.NET` の RID（`win-x64` / `win-arm64` / `osx-x64` /
`osx-arm64` / `linux-x64` / `linux-arm64`）に合わせています。
