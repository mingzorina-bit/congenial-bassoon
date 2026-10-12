#pragma once
#include "TsfKeyRouter.h"
#include <functional>

namespace bilingual::tsf {
class TsfEditGate {
 public:
  explicit TsfEditGate(TsfKeyRouter& router): router_(router) {}
  bool interested(const TsfKeyEvent& event,bool sensitive) const;
  TsfKeyResult dispatch(const TsfKeyEvent& event,bool sensitive,
    const std::function<bool(const std::function<void()>&)>& requestEdit);
 private:
  TsfKeyRouter& router_;
};
}

