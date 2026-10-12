#pragma once
#include "InputSession.h"
#include <string>
#include <vector>

namespace bilingual::tsf {
struct TsfKeyEvent {
  unsigned virtualKey;
  char character;
  bool shift;
  bool ctrl;
  bool alt;
};
struct TsfKeyResult {
  bool handled = false;
  bool cancelled = false;
  std::string raw;
  std::vector<std::string> candidates;
  int highlighted = 0;
  std::string commit;
};
class TsfKeyRouter {
 public:
  explicit TsfKeyRouter(InputSession& session): session_(session) {}
  TsfKeyResult route(const TsfKeyEvent& event,bool sensitive);
  TsfKeyResult snapshot() const;
  void reset();
 private:
  InputSession& session_;
};
}

