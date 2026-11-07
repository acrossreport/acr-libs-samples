/*
 * acr.h - C ABI for axr_runtime (the ACR native rendering engine)
 *
 * This header declares the minimal C FFI surface documented in this
 * repository's README: PDF/ZIP(PNG) rendering.
 * It matches the native library bundled under each platform's
 * native/ folder (axr_runtime.dll / libaxr_runtime.so / libaxr_runtime.dylib).
 *
 * Usage (C/C++):
 *     #include "acr.h"
 *     // link against / dlopen the axr_runtime library for your platform
 *
 * This same C ABI is also what other language bindings call through
 * (ctypes in Python, P/Invoke in C#/VB.NET, JNA/JNI in Java). Future
 * bindings (e.g. Go via cgo) can target this header directly.
 */

#ifndef ACR_H
#define ACR_H

#ifdef __cplusplus
extern "C" {
#endif

/*
 * Render a report to PDF.
 *
 * template_json, data_json: NUL-terminated UTF-8 strings holding the ACR
 * template and data definitions.
 * out_len: receives the length, in bytes, of the returned buffer.
 *
 * Returns a pointer to the PDF bytes, or NULL on failure. The caller must
 * release the returned buffer with acr_free_buffer(ptr, *out_len).
 */
unsigned char* acr_render_pdf(const char* template_json, const char* data_json, int* out_len);

/*
 * Same as acr_render_pdf, but returns a ZIP archive containing one PNG per
 * page (ACR-PNG-PACKAGE format) instead of a PDF.
 */
unsigned char* acr_render_zip(const char* template_json, const char* data_json, int* out_len);

/*
 * Returns a pointer to a NUL-terminated UTF-8 string describing the error
 * from the most recent acr_render_pdf() / acr_render_zip() call.
 *
 * Returns an empty string ("") if that call succeeded. Call this right
 * after a render call returns NULL to get a human-readable reason.
 *
 * The returned pointer is owned by the library (do not free it); it
 * remains valid until the next render call on this process.
 */
const char* acr_last_error(void);

/*
 * Free a buffer previously returned by acr_render_pdf() or acr_render_zip().
 *
 * ptr: the buffer pointer.
 * len: the length that was written to out_len by the call that returned ptr.
 *
 * Always call this once you are done with the buffer; failing to do so
 * leaks memory (the OS reclaims it when the process exits, but calling this
 * is mandatory when embedding in a long-running process).
 */
void acr_free_buffer(unsigned char* ptr, int len);

#ifdef __cplusplus
}
#endif

#endif /* ACR_H */
