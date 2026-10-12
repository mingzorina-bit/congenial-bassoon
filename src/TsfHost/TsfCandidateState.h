#pragma once
#include "TsfKeyRouter.h"
#include <vector>

namespace bilingual::tsf {
class TsfCandidateState {
 public:
  void update(const TsfKeyResult& result,bool hostVisible,bool geometryAvailable);
  void hide();
  bool visible() const { return visible_; }
  const std::vector<std::string>& entries() const { return entries_; }
  int highlighted() const { return highlighted_; }
 private:
  bool visible_=false;
  std::vector<std::string> entries_;
  int highlighted_=0;
};
}
