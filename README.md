# axr_runtime 組み込みサンプル (C# / VB.NET / Python / Java)

`axr_runtime`（Windowsでは `axr_runtime.dll`、macOSでは
`libaxr_runtime.dylib`、Linuxでは `libaxr_runtime.so` というファイル名に
なります）は、Avalonia デザイナー向けに作られた C ABI のネイティブ共有
ライブラリです（Rust側のクレート名は `acr_dll`）。`@acrossreport/acr-node`
（napi-rs 製 Node.js アドオン）が Node.js から ACR を組み込むサンプルで
あるのに対し、ここでは**同じ C FFI を、C# / VB.NET / Python / Java の
4言語から直接呼び出す**最小サンプルを置いています。

Windows / macOS / Linux のどれでも、`axr_runtime` をそのプラットフォーム
用にビルドすれば同じサンプルコードで動きます（ライブラリの探索ロジックが
OS ごとのファイル名差を吸収します）。

このフォルダは `Acr`（本体のソース管理リポジトリ）とは別管理です
（`acr_node_native` が `@acrossreport/acr-node` と別リポジトリに分かれて
いるのと同じ理由）。既定では `Acr` の**隣**に置くことを想定しています：

```
ACR_DEV/
├── Acr/                 # 本体（axr_runtime のビルド元。target/release/ ができる）
└── acr-libs-samples/     # このフォルダ
    ├── common/          # 共通のテンプレートJSON / データJSON
    ├── python/          # ctypes 版
    ├── csharp/          # .NET (C#) 版、P/Invoke
    ├── vb/              # .NET (VB.NET) 版、P/Invoke
    └── java/            # JNA 版
```

`Acr` を別の場所に置いている場合は、各サンプル実行時に環境変数
`ACR_WORKSPACE_DIR` で `Acr` ワークスペースのルートを指定してください。

## 公開している API（Rust側: `acr_dll/src/lib.rs`, `license_ffi.rs`）

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

## 0. 事前準備: axr_runtime をビルドする

`Acr` ワークスペースのルートで：

```bash
cargo build --release -p acr_dll
```

成功すると `Acr/target/release/` に以下が生成されます（OSごとに名前が
変わります。拡張子はそのプラットフォームの共有ライブラリ形式です）：

| OS | ファイル名 |
|---|---|
| Windows | `axr_runtime.dll` |
| macOS | `libaxr_runtime.dylib` |
| Linux | `libaxr_runtime.so` |

各言語サンプルは、このビルド済みファイルを `../Acr/target/release/`
（既定値。`ACR_WORKSPACE_DIR` で変更可）から直接ロードします（事前に
コピーする必要はありません）。

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

Python / C# / VB.NET / Java すべて、実際に `output.pdf` の生成まで確認
済みです。

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

Ubuntu 24.04 (WSL2, aarch64) で `sudo apt install openjdk-21-jdk maven` →
Python / Java の両サンプルを実機で動作確認済みです（`libaxr_runtime.so`
は `cargo build --release -p acr_dll` でそのマシン上でビルドしたもの。
ARM64 Linux用に別途クロスビルドする必要はありません）。

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
