#include <initguid.h>
#include "TsfSensitivity.h"

namespace bilingual::tsf {
bool IsSensitiveInputScope(InputScope scope) {
  return scope==IS_PASSWORD || scope==IS_PRIVATE ||
    scope==IS_NUMERIC_PASSWORD || scope==IS_NUMERIC_PIN ||
    scope==IS_ALPHANUMERIC_PIN || scope==IS_ALPHANUMERIC_PIN_SET;
}
bool IsSensitiveWindow(HWND window) {
  return window && (GetWindowLongPtrW(window,GWL_STYLE)&ES_PASSWORD)!=0;
}
bool HasSensitiveWindowStyle(ITfContext* context) {
  if(!context) return true;
  ITfContextView* view=nullptr;
  if(FAILED(context->GetActiveView(&view)) || !view) return false;
  HWND host=nullptr;
  view->GetWnd(&host);
  view->Release();
  if(!host) return false;
  HWND focus=GetFocus();
  const HWND target=(focus && (focus==host || IsChild(host,focus)))?focus:host;
  return IsSensitiveWindow(target);
}
bool HasSensitiveInputScope(ITfContext* context,TfEditCookie cookie) {
  if(!context) return true;
  TF_SELECTION selection{};
  ULONG fetched=0;
  if(FAILED(context->GetSelection(cookie,TF_DEFAULT_SELECTION,1,&selection,&fetched)) ||
     fetched!=1 || !selection.range) return true;
  ITfReadOnlyProperty* property=nullptr;
  const HRESULT found=context->GetAppProperty(GUID_PROP_INPUTSCOPE,&property);
  if(FAILED(found) || !property) { selection.range->Release(); return false; }
  VARIANT value; VariantInit(&value);
  const HRESULT read=property->GetValue(cookie,selection.range,&value);
  selection.range->Release();
  property->Release();
  if(FAILED(read) || value.vt!=VT_UNKNOWN || !value.punkVal) {
    VariantClear(&value);
    return FAILED(read);
  }
  ITfInputScope* inputScope=nullptr;
  const HRESULT queried=value.punkVal->QueryInterface(IID_ITfInputScope,
    reinterpret_cast<void**>(&inputScope));
  VariantClear(&value);
  if(FAILED(queried) || !inputScope) return true;
  InputScope* scopes=nullptr;
  UINT count=0;
  const HRESULT listed=inputScope->GetInputScopes(&scopes,&count);
  inputScope->Release();
  if(FAILED(listed)) return true;
  bool sensitive=false;
  for(UINT i=0;i<count;++i) if(IsSensitiveInputScope(scopes[i])) sensitive=true;
  CoTaskMemFree(scopes);
  return sensitive;
}
}
