[English](README.md) | **日本語** | [Français](README.fr.md)

# 組み込みサンプル

`axr_runtime` は、Avalonia デザイナー向けに作られた C ABI のネイティブ
共有ライブラリです。`@acrossreport/acr-node`（napi-rs 製 Node.js アドオン）
が Node.js から ACR を組み込むサンプルであるのに対し、ここでは**同じ
C FFI を、C# / VB.NET / Python / Java の4言語から直接呼び出す**最小
サンプルを置いています。

**ビルドは不要です。** 動作確認を行ったプラットフォームごとに、ライブラリ
とサンプル一式を丸ごと配布物として分けています。**自分のOS・CPUの
フォルダだけ**を使ってください。

C / C++ から（また、Go から cgo 経由で呼ぶ場合も）、このリポジトリ
ルートにある [`acr.h`](acr.h) を使ってください。各サンプルが呼んでいる
のと同じ C ABI を宣言したヘッダーです。

```
acr-libs-samples/
├── acr.h            # C ABIヘッダー(C/C++、cgoなど向け)
├── windows-x64/     # Windows (x64)
│   ├── native/axr_runtime.dll
│   ├── native/axr_runtime.lib  # インポートライブラリ(MSVC製C/C++やcgoからのリンク用)
│   ├── common/      # テンプレートJSON / データJSON
│   ├── python/
│   ├── csharp/
│   ├── vb/
│   └── java/
├── windows-arm64/   # Windows (ARM64)
│   ├── native/axr_runtime.dll
│   ├── native/axr_runtime.lib  # インポートライブラリ(MSVC製C/C++やcgoからのリンク用)
│   ├── common/
│   ├── python/
│   ├── csharp/
│   ├── vb/
│   └── java/
├── linux-x64/       # Linux (x64)
│   ├── native/libaxr_runtime.so
│   ├── common/
│   ├── python/
│   ├── csharp/
│   ├── vb/
│   └── java/
├── linux-arm64/     # Linux (ARM64)
│   ├── native/libaxr_runtime.so
│   ├── common/
│   ├── python/
│   ├── csharp/
│   ├── vb/
│   └── java/
├── macos-arm64/     # macOS (Apple Silicon / arm64)
│   ├── native/libaxr_runtime.dylib
│   ├── common/
│   ├── python/
│   ├── csharp/
│   ├── vb/
│   └── java/
└── macos-x64/       # macOS (Intel / x64)
    ├── native/libaxr_runtime.dylib
    ├── common/
    ├── python/
    ├── csharp/
    ├── vb/
    └── java/
```

各フォルダは完全に独立しています（他のプラットフォーム用フォルダに一切
依存しません）。使い方は共通で、それぞれの `<platform>/<language>/` の
中の README を見てください。

## 対応プラットフォーム

| フォルダ | 状態 |
|---|---|
| `windows-x64/` | ✅ 動作確認済み（4言語とも） |
| `windows-arm64/` | ✅ 動作確認済み（4言語とも） |
| `linux-arm64/` | ✅ 動作確認済み（4言語とも） |
| `linux-x64/` | ✅ 動作確認済み（4言語とも） |
| `macos-arm64/` | ✅ 動作確認済み（4言語とも） |
| `macos-x64/` | ✅ 動作確認済み（4言語とも） |

手元にそのプラットフォームが無い場合は、環境変数 `ACR_NATIVE_LIB` で
別途用意したライブラリの場所を指定すれば、同じサンプルコードを流用
できます（Python/C#/VB.NET はライブラリファイルへのフルパス、Java は
ライブラリファイルを含むディレクトリのパス）。

## 公開しているAPI

このサンプルが使うのは次の最小限だけです。C宣言の全体（`acr_render_zip`
を含む）は [`acr.h`](acr.h) を参照してください。

| 関数 | 説明 |
|---|---|
| `acr_license_activate(email, key)` | ライセンスサーバーに問い合わせて認証（ネットワーク必須） |
| `acr_license_last_message()` | 直前のアクティベーション結果メッセージを返す |
| `acr_render_pdf(templateJson, dataJson, &outLen)` | PDFバイト列を生成 |
| `acr_free_buffer(ptr, len)` | 上記が返したバッファを解放（呼び出し側は必ず呼ぶこと） |
| `acr_last_error()` | 直近の描画呼び出しが失敗した理由を返す（成功時は空文字列） |

PDFの代わりにZIP（全ページPNG入り、ACR-PNG-PACKAGE形式）が欲しい場合は、
同じ引数の `acr_render_zip(templateJson, dataJson, &outLen)` に差し替える
だけです（このサンプルでは呼んでいません）。

`acr_render_pdf` / `acr_render_zip` が NULL を返した場合は、
`acr_last_error()` を呼べば失敗理由（テンプレート・データJSONの不備など）
を人間が読める形で取得できます。

## C / C++（および cgo）からのリンク

リポジトリルートの `acr.h` が、全プラットフォーム共通のC ABIを宣言して
います。リンク方法:

- Windows: `<platform>/native/axr_runtime.lib`（インポートライブラリ）を
  リンクし、実行時は `axr_runtime.dll` を実行ファイルと同じ場所に置く。
- Linux: `<platform>/native/libaxr_runtime.so` を直接リンク（Linuxでは
  別途インポートライブラリは不要）。
- macOS: `<platform>/native/libaxr_runtime.dylib` を直接リンク。

cgo の場合は `CGO_LDFLAGS` でそのプラットフォームの `native/` フォルダ
を指定し（例: Windowsなら `-L${SRCDIR}/windows-x64/native -laxr_runtime`
で `.lib` を、Linux/macOSなら `.so`/`.dylib` を直接指定）、cgoの
プリアンブルで `#include "acr.h"` してください。

## ライセンスとウォーターマーク

**アクティベートしていない状態（既定）では、出力PDFにウォーターマークが
入ります。** ENGINEライセンスでアクティベートした場合のみ
ウォーターマークが消えます。

ライセンスキーは [licensvr.acrossreport.com](https://licensvr.acrossreport.com)
で無償登録すると取得できます（Google・GitHub・メールアドレスのいずれかで
登録可能、ACR Engine向けの登録フォームからどうぞ）。取得したキーを
`ACR_LICENSE_EMAIL` / `ACR_LICENSE_KEY` に設定してください。

各サンプルは、環境変数 `ACR_LICENSE_EMAIL` / `ACR_LICENSE_KEY` の両方が
設定されている場合のみ `acr_license_activate()` を呼び出します（未設定
なら "activation: skipped" と表示してそのまま未認証状態で進みます）。
**ウォーターマークの有無は、生成された `output.pdf` を実際に開いて
確認してください。**

## 注意点（組み込み時に意識すること）

- `acr_render_pdf`（および `acr_render_zip`）が返すバッファはライブラリ
  側が確保したメモリです。使い終わったら必ず `acr_free_buffer(ptr, len)`
  を呼んでください。呼ばないとメモリリークします（プロセス終了時にOSが
  回収しますが、長時間稼働するサーバープロセスに組み込む場合は必須です）。
- テンプレートJSONは最低限、`ACR.Report.Sections.Section` の配列に
  **「Detail」という名前に正規化されるセクションを1つだけ**含む必要が
  あります。`common/template.json` はその最小形です。
