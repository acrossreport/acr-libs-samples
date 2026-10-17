"""
axr_runtime（ACRのネイティブ共有ライブラリ）を Python から呼び出す
最小サンプル。

ビルド済みのネイティブライブラリ (axr_runtime.dll / libaxr_runtime.so /
libaxr_runtime.dylib) を ctypes でロードし、テンプレートJSON + データJSON
から PDF を1本生成するだけの例です。

事前準備:
    cd ../Acr   (Acr ワークスペースのルート。既定ではこのサンプルの
                 一つ上の階層にある "Acr" フォルダを想定しています)
    cargo build --release -p acr_dll

実行:
    python render_sample.py

Acr ワークスペースが既定の場所(このサンプルの隣の "Acr" フォルダ)に無い
場合は、環境変数 ACR_WORKSPACE_DIR で Acr ワークスペースのルートを指定
してください。
"""

import ctypes
import os
import pathlib
import platform
import sys

SAMPLE_DIR = pathlib.Path(__file__).resolve().parent
SAMPLES_ROOT = SAMPLE_DIR.parent
COMMON_DIR = SAMPLES_ROOT / "common"
# acr_dll/Cargo.toml の [lib] name = "axr_runtime" が生成物のベース名。
# 既定では acr-libs-samples の隣にある "Acr" ワークスペースを見る。
# target-dir は .cargo/config.toml で変わっていなければ target/release。
ACR_WORKSPACE_ROOT = pathlib.Path(
    os.environ.get("ACR_WORKSPACE_DIR", str(SAMPLES_ROOT.parent / "Acr"))
)
TARGET_RELEASE = ACR_WORKSPACE_ROOT / "target" / "release"


def native_lib_path() -> pathlib.Path:
    system = platform.system()
    if system == "Windows":
        name = "axr_runtime.dll"
    elif system == "Darwin":
        name = "libaxr_runtime.dylib"
    else:
        name = "libaxr_runtime.so"
    path = TARGET_RELEASE / name
    if not path.exists():
        raise FileNotFoundError(
            f"native library not found: {path}\n"
            "先に `cargo build --release -p acr_dll` を実行してください。"
        )
    return path


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
