#include "TsfCandidateState.h"
#include <algorithm>

namespace bilingual::tsf {
void TsfCandidateState::hide() {
  visible_=false;
  entries_.clear();
  highlighted_=0;
}
void TsfCandidateState::update(const TsfKeyResult& result,bool hostVisible,bool geometryAvailable) {
  if(!hostVisible || !geometryAvailable || result.raw.empty() ||
     result.cancelled || !result.commit.empty() || result.candidates.empty()) {
    hide(); return;
  }
  const size_t count=std::min<size_t>(5,result.candidates.size());
  entries_.assign(result.candidates.begin(),result.candidates.begin()+count);
  highlighted_=std::clamp(result.highlighted,0,static_cast<int>(count)-1);
  visible_=true;
}
}
