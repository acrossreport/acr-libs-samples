[English](README.md) | [日本語](README.ja.md) | [Français](README.fr.md)

# Embedding Samples

`axr_runtime` is a C-ABI native shared library built for the Avalonia
designer. While `@acrossreport/acr-node` (a napi-rs Node.js addon) is a
sample that embeds ACR from Node.js, this repository contains minimal
samples that **call the same C FFI directly from four languages:
C# / VB.NET / Python / Java.**

**No build required.** For each platform we've verified, the library and
the full sample set are bundled as a self-contained distribution. **Use
only the folder that matches your own OS and CPU.**

For C / C++ (and for cgo, when calling the library from Go), use
[`acr.h`](acr.h) at the root of this repository — it declares the same C
ABI these samples call into, and is the header to `#include`/`cgo` against.

```
acr-libs-samples/
├── acr.h            # C ABI header (C/C++, cgo, ...)
├── windows-x64/     # Windows (x64)
│   ├── native/axr_runtime.dll
│   ├── native/axr_runtime.lib  # import lib, for linking from MSVC C/C++ or cgo
│   ├── common/      # template JSON / data JSON
│   ├── python/
│   ├── csharp/
│   ├── vb/
│   └── java/
├── windows-arm64/   # Windows (ARM64)
│   ├── native/axr_runtime.dll
│   ├── native/axr_runtime.lib  # import lib, for linking from MSVC C/C++ or cgo
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

Each folder is completely self-contained (it does not depend on any
other platform's folder). Usage is the same across platforms — see the
README inside each `<platform>/<language>/` folder.

## Supported platforms

| Folder | Status |
|---|---|
| `windows-x64/` | ✅ Verified (all 4 languages) |
| `windows-arm64/` | ✅ Verified (all 4 languages) |
| `linux-arm64/` | ✅ Verified (all 4 languages) |
| `linux-x64/` | ✅ Verified (all 4 languages) |
| `macos-arm64/` | ✅ Verified (all 4 languages) |
| `macos-x64/` | ✅ Verified (all 4 languages) |

If you don't have that exact platform at hand, you can point the
`ACR_NATIVE_LIB` environment variable at a library you've obtained some
other way and reuse the same sample code (for Python/C#/VB.NET this is
the full path to the library file; for Java it's the path to the
directory containing the library file).

## Exposed API

This sample only uses the following minimal set of functions. See
[`acr.h`](acr.h) for the full C declarations (including `acr_render_zip`).

| Function | Description |
|---|---|
| `acr_license_activate(email, key)` | Contacts the license server to activate (requires network access) |
| `acr_license_last_message()` | Returns the message from the most recent activation attempt |
| `acr_render_pdf(templateJson, dataJson, &outLen)` | Generates a PDF byte buffer |
| `acr_free_buffer(ptr, len)` | Frees the buffer returned by the functions above (the caller must always call this) |
| `acr_last_error()` | Returns the reason the most recent render call failed (empty string on success) |

If you want a ZIP (containing one PNG per page, in ACR-PNG-PACKAGE
format) instead of a PDF, just swap in `acr_render_zip(templateJson,
dataJson, &outLen)`, which takes the same arguments (this sample does
not call it).

If `acr_render_pdf` / `acr_render_zip` returns NULL, call `acr_last_error()`
to get a human-readable reason (e.g. invalid template/data JSON).

## Linking from C / C++ (and cgo)

`acr.h` at the repository root declares the C ABI shared by every
platform. Link against:

- Windows: `<platform>/native/axr_runtime.lib` (import library) +
  `axr_runtime.dll` alongside your executable at runtime.
- Linux: link against `<platform>/native/libaxr_runtime.so` directly
  (no separate import library is needed on Linux).
- macOS: link against `<platform>/native/libaxr_runtime.dylib` directly.

For cgo, point `CGO_LDFLAGS` at the platform's `native/` folder (e.g.
`-L${SRCDIR}/windows-x64/native -laxr_runtime` on Windows, using the
`.lib`; `-laxr_runtime` against the `.so`/`.dylib` directly on Linux/macOS)
and `#include "acr.h"` from your cgo preamble.

## License and watermark

**When not activated (the default), the output PDF includes a
watermark.** The watermark only disappears once you've activated with
an ENGINE license.

You can obtain a license key for free at
[licensvr.acrossreport.com](https://licensvr.acrossreport.com) (sign up
with Google, GitHub, or an email address, using the registration form
for ACR Engine). Set the key you receive as `ACR_LICENSE_EMAIL` /
`ACR_LICENSE_KEY`.

Each sample only calls `acr_license_activate()` when both the
`ACR_LICENSE_EMAIL` and `ACR_LICENSE_KEY` environment variables are set
(if they aren't, it prints "activation: skipped" and proceeds
unactivated). **Check whether the watermark is present by actually
opening the generated `output.pdf`.**

## Notes for embedding

- The buffer returned by `acr_render_pdf` (and `acr_render_zip`) is
  memory allocated by the library. Once you're done with it, you must
  call `acr_free_buffer(ptr, len)`. Failing to do so leaks memory (the
  OS reclaims it when the process exits, but calling it is mandatory
  when embedding in a long-running server process).
- At minimum, the template JSON's array of `ACR.Report.Sections.Section`
  must contain **exactly one section that normalizes to the name
  "Detail".** `common/template.json` is the minimal form of this.
