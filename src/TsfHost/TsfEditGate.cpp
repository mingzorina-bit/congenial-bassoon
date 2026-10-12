#include "TsfEditGate.h"

namespace bilingual::tsf {
bool TsfEditGate::interested(const TsfKeyEvent& event,bool sensitive) const {
  if(sensitive || event.ctrl || event.alt || event.virtualKey==0x09 ||
     (event.shift && event.virtualKey==0x0d)) return false;
  if((event.character>='a'&&event.character<='z') ||
     (event.character>='A'&&event.character<='Z') || event.character=='\'') return true;
  if(router_.snapshot().raw.empty()) return false;
  switch(event.virtualKey) {
    case 0x08: case 0x0d: case 0x1b: case 0x20:
    case 0x25: case 0x26: case 0x27: case 0x28: return true;
    default: return event.virtualKey>='1'&&event.virtualKey<='5';
  }
}
TsfKeyResult TsfEditGate::dispatch(const TsfKeyEvent& event,bool sensitive,
  const std::function<bool(const std::function<void()>&)>& requestEdit) {
  if(!interested(event,sensitive)) {
    if(sensitive) router_.reset();
    return router_.snapshot();
  }
  TsfKeyResult result;
  bool invoked=false;
  const bool granted=requestEdit([&] {
    invoked=true;
    result=router_.route(event,false);
  });
  if(!granted || !invoked) return router_.snapshot();
  return result;
}
}

