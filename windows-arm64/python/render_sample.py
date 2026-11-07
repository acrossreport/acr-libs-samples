"""
axr_runtime（ACRのネイティブ共有ライブラリ、Windows ARM64版）を Python
から呼び出す最小サンプル。ビルドは不要です。

実行:
    python render_sample.py

"""

import ctypes
import os
import pathlib
import sys

SAMPLE_DIR = pathlib.Path(__file__).resolve().parent
PLATFORM_ROOT = SAMPLE_DIR.parent
COMMON_DIR = PLATFORM_ROOT / "common"
NATIVE_LIB = PLATFORM_ROOT / "native" / "axr_runtime.dll"


def load_library() -> ctypes.CDLL:
    lib_path = pathlib.Path(os.environ.get("ACR_NATIVE_LIB", str(NATIVE_LIB)))
    if not lib_path.exists():
        raise FileNotFoundError(f"native library not found: {lib_path}")
    lib = ctypes.CDLL(str(lib_path))

    lib.acr_render_pdf.argtypes = [
        ctypes.c_char_p,  # template_json
        ctypes.c_char_p,  # data_json
        ctypes.POINTER(ctypes.c_int),  # out_len
    ]
    lib.acr_render_pdf.restype = ctypes.POINTER(ctypes.c_ubyte)

    lib.acr_free_buffer.argtypes = [ctypes.POINTER(ctypes.c_ubyte), ctypes.c_int]
    lib.acr_free_buffer.restype = None

    return lib


def main() -> int:
    lib = load_library()


    template_json = (COMMON_DIR / "template.json").read_text(encoding="utf-8")
    data_json = (COMMON_DIR / "data.json").read_text(encoding="utf-8")

    template_bytes = template_json.encode("utf-8")
    data_bytes = data_json.encode("utf-8")

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
