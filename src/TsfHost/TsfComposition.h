#pragma once
#include "TsfKeyRouter.h"
#include <msctf.h>

namespace bilingual::tsf {
class TsfComposition {
 public:
  ~TsfComposition();
  HRESULT apply(ITfContext* context, TfEditCookie cookie, const TsfKeyResult& result);
  HRESULT cancel(ITfContext* context, TfEditCookie cookie);
  void abandon();
  ITfContext* context() const { return context_; }
  HRESULT currentRange(ITfRange** range) const;
 private:
  HRESULT begin(ITfContext* context, TfEditCookie cookie);
  HRESULT replace(TfEditCookie cookie, const std::wstring& text);
  HRESULT finish(TfEditCookie cookie, const std::wstring& text);
  ITfContext* context_ = nullptr;
  ITfComposition* composition_ = nullptr;
};
}
