#include "TsfKeyRouter.h"

namespace bilingual::tsf {
namespace {
constexpr unsigned vkBack = 0x08, vkTab = 0x09, vkReturn = 0x0d;
constexpr unsigned vkEscape = 0x1b, vkSpace = 0x20;
constexpr unsigned vkLeft = 0x25, vkUp = 0x26, vkRight = 0x27, vkDown = 0x28;
}
TsfKeyResult TsfKeyRouter::snapshot() const {
  TsfKeyResult result;
  result.raw = session_.raw();
  result.candidates = session_.candidates();
  result.highlighted = session_.highlighted();
  return result;
}
void TsfKeyRouter::reset() {
  if(!session_.raw().empty()) session_.press(Key::Escape);
  session_.takeCommit();
}
TsfKeyResult TsfKeyRouter::route(const TsfKeyEvent& event,bool sensitive) {
  if(sensitive) { reset(); return snapshot(); }
  TsfKeyResult result;
  if(event.ctrl || event.alt || event.virtualKey == vkTab || (event.shift && event.virtualKey == vkReturn)) return snapshot();
  if((event.character >= 'a' && event.character <= 'z') ||
     (event.character >= 'A' && event.character <= 'Z') || event.character == '\'') {
    session_.type(event.character);
    result.handled = true;
  } else if(!session_.raw().empty()) {
    switch(event.virtualKey) {
      case vkReturn: result.handled = session_.press(Key::Enter); break;
      case vkSpace: result.handled = session_.press(Key::Space); break;
      case vkBack: result.handled = session_.press(Key::Backspace); break;
      case vkLeft: result.handled = session_.press(Key::Left); break;
      case vkRight: result.handled = session_.press(Key::Right); break;
      case vkUp: result.handled = session_.press(Key::Up); break;
      case vkDown: result.handled = session_.press(Key::Down); break;
      case vkEscape: result.handled = session_.press(Key::Escape); result.cancelled = result.handled; break;
      default:
        if(event.virtualKey >= '1' && event.virtualKey <= '5')
          result.handled = session_.select(static_cast<int>(event.virtualKey - '1'));
        break;
    }
  }
  result.raw = session_.raw();
  result.candidates = session_.candidates();
  result.highlighted = session_.highlighted();
  result.commit = session_.takeCommit();
  return result;
}
}

