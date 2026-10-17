"""
axr_runtime（ACRのネイティブ共有ライブラリ）を Python から呼び出す
最小サンプル。

ビルド済みのネイティブライブラリ (axr_runtime.dll / libaxr_runtime.so /
libaxr_runtime.dylib) を ctypes でロードし、テンプレートJSON + データJSON
から PDF を1本生成するだけの例です。ビルドは不要です。対応プラット
フォーム向けのライブラリは `native/<rid>/` に同梱されています。

実行:
    python render_sample.py

同梱されていないプラットフォーム（`native/` 配下を参照）の場合は、
環境変数 ACR_NATIVE_LIB にネイティブライブラリのフルパスを指定して
ください。
"""

import ctypes
import os
import pathlib
import platform
import sys

SAMPLE_DIR = pathlib.Path(__file__).resolve().parent
SAMPLES_ROOT = SAMPLE_DIR.parent
COMMON_DIR = SAMPLES_ROOT / "common"
NATIVE_ROOT = SAMPLES_ROOT / "native"


def native_lib_filename() -> str:
    system = platform.system()
    if system == "Windows":
        return "axr_runtime.dll"
    elif system == "Darwin":
        return "libaxr_runtime.dylib"
    else:
        return "libaxr_runtime.so"


def runtime_identifier() -> str:
    system = platform.system()
    machine = platform.machine().lower()
    is_arm64 = machine in ("arm64", "aarch64")
    if system == "Windows":
        return "win-arm64" if is_arm64 else "win-x64"
    elif system == "Darwin":
        return "osx-arm64" if is_arm64 else "osx-x64"
    else:
        return "linux-arm64" if is_arm64 else "linux-x64"


def native_lib_path() -> pathlib.Path:
    override = os.environ.get("ACR_NATIVE_LIB")
    if override:
        path = pathlib.Path(override)
        if not path.exists():
            raise FileNotFoundError(f"ACR_NATIVE_LIB で指定されたファイルが見つかりません: {path}")
        return path

    rid = runtime_identifier()
    name = native_lib_filename()
    bundled = NATIVE_ROOT / rid / name
    if bundled.exists():
        return bundled

    raise FileNotFoundError(
        f"native library not found for this platform ({rid}): {bundled}\n"
        f"同梱されているプラットフォームは native/ ディレクトリを確認してください。\n"
        "別の場所にあるライブラリを使う場合は環境変数 ACR_NATIVE_LIB で"
        "フルパスを指定してください。"
    )


def load_library() -> ctypes.CDLL:
    lib = ctypes.CDLL(str(native_lib_path()))

    lib.acr_version.restype = ctypes.c_char_p

    lib.acr_license_check.restype = ctypes.c_int

    lib.acr_render_pdf.argtypes = [
        ctypes.c_char_p,  # template_json
        ctypes.c_char_p,  # data_json
        ctypes.POINTER(ctypes.c_int),  # out_len
    ]
    lib.acr_render_pdf.restype = ctypes.POINTER(ctypes.c_ubyte)

    lib.acr_get_page_count.argtypes = [ctypes.c_char_p, ctypes.c_char_p]
    lib.acr_get_page_count.restype = ctypes.c_int

    lib.acr_free_buffer.argtypes = [ctypes.POINTER(ctypes.c_ubyte), ctypes.c_int]
    lib.acr_free_buffer.restype = None

    return lib


def main() -> int:
    lib = load_library()

    version = lib.acr_version().decode("ascii")
    tier = lib.acr_license_check()  # 2=DESIGNERPRO / 1=DESIGNER / 0=未認証(ウォーターマーク付き)
    print(f"axr_runtime version: {version}")
    print(f"license tier: {tier} (0=unlicensed/watermarked, 1=DESIGNER, 2=DESIGNERPRO)")

    template_json = (COMMON_DIR / "template.json").read_text(encoding="utf-8")
    data_json = (COMMON_DIR / "data.json").read_text(encoding="utf-8")

    template_bytes = template_json.encode("utf-8")
    data_bytes = data_json.encode("utf-8")

    page_count = lib.acr_get_page_count(template_bytes, data_bytes)
    print(f"page count: {page_count}")
    if page_count < 0:
        print("ERROR: failed to parse template/data JSON", file=sys.stderr)
        return 1

    out_len = ctypes.c_int(0)
    buf_ptr = lib.acr_render_pdf(template_bytes, data_bytes, ctypes.byref(out_len))
    if not buf_ptr:
        print("ERROR: acr_render_pdf returned null", file=sys.stderr)
        return 1

    try:
        pdf_bytes = ctypes.string_at(buf_ptr, out_len.value)
        out_path = SAMPLE_DIR / "output.pdf"
        out_path.write_bytes(pdf_bytes)
        print(f"wrote {len(pdf_bytes)} bytes -> {out_path}")
    finally:
        lib.acr_free_buffer(buf_ptr, out_len)

    return 0


if __name__ == "__main__":
    sys.exit(main())
