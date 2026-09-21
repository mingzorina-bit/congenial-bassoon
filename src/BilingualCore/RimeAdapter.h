#pragma once
#include "InputSession.h"
#include <rime_api.h>
namespace bilingual {
// One active adapter per process. All methods are called by one serial session owner.
class RimeAdapter final: public PrimaryEngine {
 public:
  RimeAdapter(const std::string& shared, const std::string& user);
  ~RimeAdapter() override;
  std::vector<std::string> query(const std::string& raw) override;
 private:
  RimeApi* api_ = nullptr;
  RimeSessionId session_ = 0;
};
}
