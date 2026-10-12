#pragma once
#include <windows.h>
#include <msctf.h>
#include <inputscope.h>

namespace bilingual::tsf {
bool IsSensitiveInputScope(InputScope scope);
bool IsSensitiveWindow(HWND window);
bool HasSensitiveWindowStyle(ITfContext* context);
bool HasSensitiveInputScope(ITfContext* context,TfEditCookie cookie);
}
