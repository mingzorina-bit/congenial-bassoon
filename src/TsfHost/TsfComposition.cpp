#include "TsfComposition.h"
#include <string>

namespace bilingual::tsf {
namespace {
std::wstring toWide(const std::string& text) {
  if(text.empty()) return {};
  const int length=MultiByteToWideChar(CP_UTF8,MB_ERR_INVALID_CHARS,text.data(),
    static_cast<int>(text.size()),nullptr,0);
  if(length<=0) return {};
  std::wstring wide(static_cast<size_t>(length),L'\0');
  if(!MultiByteToWideChar(CP_UTF8,MB_ERR_INVALID_CHARS,text.data(),
      static_cast<int>(text.size()),wide.data(),length)) return {};
  return wide;
}
}
TsfComposition::~TsfComposition() { abandon(); }
HRESULT TsfComposition::currentRange(ITfRange** range) const {
  if(!range) return E_POINTER;
  *range=nullptr;
  return composition_ ? composition_->GetRange(range) : E_UNEXPECTED;
}
void TsfComposition::abandon() {
  if(composition_) { composition_->Release(); composition_=nullptr; }
  if(context_) { context_->Release(); context_=nullptr; }
}
HRESULT TsfComposition::begin(ITfContext* context,TfEditCookie cookie) {
  if(!context) return E_POINTER;
  if(composition_) return context_==context ? S_OK : E_UNEXPECTED;
  ITfInsertAtSelection* insert=nullptr;
  HRESULT hr=context->QueryInterface(IID_ITfInsertAtSelection,
    reinterpret_cast<void**>(&insert));
  if(FAILED(hr)) return hr;
  ITfRange* range=nullptr;
  hr=insert->InsertTextAtSelection(cookie,TF_IAS_QUERYONLY,L"",0,&range);
  insert->Release();
  if(FAILED(hr) || !range) return FAILED(hr)?hr:E_FAIL;
  ITfContextComposition* manager=nullptr;
  hr=context->QueryInterface(IID_ITfContextComposition,
    reinterpret_cast<void**>(&manager));
  if(SUCCEEDED(hr)) {
    hr=manager->StartComposition(cookie,range,nullptr,&composition_);
    manager->Release();
  }
  range->Release();
  if(FAILED(hr) || !composition_) return FAILED(hr)?hr:E_FAIL;
  context_=context;
  context_->AddRef();
  return S_OK;
}
HRESULT TsfComposition::replace(TfEditCookie cookie,const std::wstring& text) {
  if(!composition_) return E_UNEXPECTED;
  ITfRange* range=nullptr;
  HRESULT hr=composition_->GetRange(&range);
  if(FAILED(hr) || !range) return FAILED(hr)?hr:E_FAIL;
  hr=range->SetText(cookie,0,text.c_str(),static_cast<LONG>(text.size()));
  range->Release();
  return hr;
}
HRESULT TsfComposition::finish(TfEditCookie cookie,const std::wstring& text) {
  if(!composition_) return E_UNEXPECTED;
  const HRESULT hr=replace(cookie,text);
  if(FAILED(hr)) return hr;
  const HRESULT end=composition_->EndComposition(cookie);
  if(SUCCEEDED(end)) abandon();
  return end;
}
HRESULT TsfComposition::cancel(ITfContext* context,TfEditCookie cookie) {
  if(!composition_) return S_OK;
  if(context!=context_) return E_UNEXPECTED;
  return finish(cookie,L"");
}
HRESULT TsfComposition::apply(ITfContext* context,TfEditCookie cookie,
    const TsfKeyResult& result) {
  if(!result.handled) return S_OK;
  if(!result.commit.empty()) {
    const auto text=toWide(result.commit);
    if(text.empty()) return E_INVALIDARG;
    if(composition_) {
      if(context!=context_) return E_UNEXPECTED;
      return finish(cookie,text);
    }
    ITfInsertAtSelection* insert=nullptr;
    HRESULT hr=context->QueryInterface(IID_ITfInsertAtSelection,
      reinterpret_cast<void**>(&insert));
    if(FAILED(hr)) return hr;
    hr=insert->InsertTextAtSelection(cookie,TF_IAS_NOQUERY,text.c_str(),
      static_cast<LONG>(text.size()),nullptr);
    insert->Release();
    return hr;
  }
  if(result.raw.empty()) return cancel(context,cookie);
  const auto text=toWide(result.raw);
  if(text.empty()) return E_INVALIDARG;
  HRESULT hr=begin(context,cookie);
  if(FAILED(hr)) return hr;
  return replace(cookie,text);
}
}
