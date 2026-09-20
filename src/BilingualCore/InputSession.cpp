#include "InputSession.h"
namespace bilingual {
// Initial test seam only. The first CI run must fail behavior tests.
void InputSession::type(char) {}
bool InputSession::press(Key, bool) { return false; }
bool InputSession::select(int) { return false; }
void InputSession::highlight(int) {}
void InputSession::setPrivacy(PrivacyMode) {}
std::string InputSession::takeCommit() { return {}; }
void InputSession::refresh() {}
void InputSession::commit(const std::string&) {}
}
