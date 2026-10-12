#include "TsfService.h"
#include "RimeAdapter.h"
#include "TsfSensitivity.h"
#include <filesystem>
#include <functional>
#include <memory>
#include <mutex>
#include <new>
#include <string>

namespace bilingual::tsf {
namespace {
std::mutex engineMutex;
std::unique_ptr<RimeAdapter> sharedEngine;

std::string utf8(const std::wstring& wide) {
  if(wide.empty()) return {};
  const int size=WideCharToMultiByte(CP_UTF8,0,wide.data(),
    static_cast<int>(wide.size()),nullptr,0,nullptr,nullptr);
  if(size<=0) return {};
  std::string text(static_cast<size_t>(size),'\0');
  if(!WideCharToMultiByte(CP_UTF8,0,wide.data(),static_cast<int>(wide.size()),
      text.data(),size,nullptr,nullptr)) return {};
  return text;
}
bool ensureEngine() {
  std::lock_guard lock(engineMutex);
  if(sharedEngine) return true;
  wchar_t file[MAX_PATH]{};
  if(!GetModuleFileNameW(ModuleHandle(),file,MAX_PATH)) return false;
  const auto shared=std::filesystem::path(file).parent_path()/L"data"/L"rime";
  if(!std::filesystem::exists(shared/L"bilingual.schema.yaml")) return false;
  wchar_t local[MAX_PATH]{};
  const DWORD length=GetEnvironmentVariableW(L"LOCALAPPDATA",local,MAX_PATH);
  if(!length || length>=MAX_PATH) return false;
  const auto user=std::filesystem::path(local)/L"BilingualInput"/L"tsf-rime";
  try {
    sharedEngine=std::make_unique<RimeAdapter>(utf8(shared.wstring()),utf8(user.wstring()));
    return true;
  } catch(...) { sharedEngine.reset(); return false; }
}

class EditSession final : public ITfEditSession {
 public:
  explicit EditSession(std::function<HRESULT(TfEditCookie)> callback): callback_(std::move(callback)) {}
  HRESULT STDMETHODCALLTYPE QueryInterface(REFIID iid,void** value) override {
    if(!value) return E_POINTER;
    *value=nullptr;
    if(IsEqualIID(iid,IID_IUnknown) || IsEqualIID(iid,IID_ITfEditSession)) {
      *value=static_cast<ITfEditSession*>(this); AddRef(); return S_OK;
    }
    return E_NOINTERFACE;
  }
  ULONG STDMETHODCALLTYPE AddRef() override { return ++refs_; }
  ULONG STDMETHODCALLTYPE Release() override {
    const ULONG count=--refs_;
    if(!count) delete this;
    return count;
  }
  HRESULT STDMETHODCALLTYPE DoEditSession(TfEditCookie cookie) override { return callback_(cookie); }
 private:
  std::atomic<ULONG> refs_{1};
  std::function<HRESULT(TfEditCookie)> callback_;
};
}

std::vector<std::string> ServiceEngine::query(const std::string& raw) {
  std::lock_guard lock(engineMutex);
  return sharedEngine ? sharedEngine->query(raw) : std::vector<std::string>{};
}
TsfService::TsfService() { ModuleObjectCreated(); }
TsfService::~TsfService() { Deactivate(); ModuleObjectDestroyed(); }
HRESULT STDMETHODCALLTYPE TsfService::QueryInterface(REFIID iid,void** value) {
  if(!value) return E_POINTER;
  *value=nullptr;
  if(IsEqualIID(iid,IID_IUnknown) || IsEqualIID(iid,IID_ITfTextInputProcessor))
    *value=static_cast<ITfTextInputProcessor*>(this);
  else if(IsEqualIID(iid,IID_ITfTextInputProcessorEx))
    *value=static_cast<ITfTextInputProcessorEx*>(this);
  else if(IsEqualIID(iid,IID_ITfKeyEventSink))
    *value=static_cast<ITfKeyEventSink*>(this);
  else if(IsEqualIID(iid,IID_ITfThreadMgrEventSink))
    *value=static_cast<ITfThreadMgrEventSink*>(this);
  else return E_NOINTERFACE;
  AddRef();
  return S_OK;
}
ULONG STDMETHODCALLTYPE TsfService::AddRef() { return ++references_; }
ULONG STDMETHODCALLTYPE TsfService::Release() {
  const ULONG count=--references_;
  if(!count) delete this;
  return count;
}
HRESULT STDMETHODCALLTYPE TsfService::Activate(ITfThreadMgr* manager,TfClientId clientId) {
  return ActivateEx(manager,clientId,0);
}
HRESULT STDMETHODCALLTYPE TsfService::ActivateEx(ITfThreadMgr* manager,TfClientId clientId,DWORD flags) {
  if(!manager) return E_POINTER;
  if(manager_) Deactivate();
  secureActivation_=(flags & TF_TMAE_SECUREMODE)!=0;
  if(secureActivation_ || (flags & TF_TMAE_UIELEMENTENABLEDONLY) || !ensureEngine()) return S_OK;
  manager_=manager; manager_->AddRef();
  clientId_=clientId;
  ready_=true;
  HRESULT hr=manager_->QueryInterface(IID_ITfKeystrokeMgr,
    reinterpret_cast<void**>(&keys_));
  if(SUCCEEDED(hr)) {
    hr=keys_->AdviseKeyEventSink(clientId_,this,TRUE);
    keysAdvised_=SUCCEEDED(hr);
  }
  if(SUCCEEDED(hr)) {
    hr=manager_->QueryInterface(IID_ITfSource,
      reinterpret_cast<void**>(&source_));
    if(SUCCEEDED(hr)) hr=source_->AdviseSink(IID_ITfThreadMgrEventSink,
      static_cast<ITfThreadMgrEventSink*>(this),&sinkCookie_);
  }
  if(FAILED(hr)) { Deactivate(); return hr; }
  ITfDocumentMgr* document=nullptr;
  if(SUCCEEDED(manager_->GetFocus(&document)) && document) {
    document->GetTop(&focus_);
    document->Release();
  }
  return S_OK;
}
void TsfService::clearFocus() {
  candidates_.hide();
  ITfContext* active=composition_.context();
  if(active && clientId_!=TF_CLIENTID_NULL) {
    auto* edit=new(std::nothrow) EditSession([this,active](TfEditCookie cookie) {
      return composition_.cancel(active,cookie);
    });
    if(edit) {
      HRESULT sessionHr=E_FAIL;
      const HRESULT requestHr=active->RequestEditSession(clientId_,edit,
        TF_ES_SYNC|TF_ES_READWRITE,&sessionHr);
      edit->Release();
      if(FAILED(requestHr) || FAILED(sessionHr)) recoveryPending_=true;
    }
    if(composition_.active()) {
      // TSF can deny a synchronous edit during focus loss. Retain the old
      // composition in the queued edit session so it can be removed later.
      auto pending=std::make_shared<TsfComposition>();
      pending->swap(composition_);
      auto* deferred=new(std::nothrow) EditSession([pending,active](TfEditCookie cookie) {
        return pending->cancel(active,cookie);
      });
      HRESULT sessionHr=E_FAIL;
      const HRESULT requestHr=deferred ? active->RequestEditSession(clientId_,deferred,
        TF_ES_ASYNC|TF_ES_READWRITE,&sessionHr) : E_OUTOFMEMORY;
      if(deferred) deferred->Release();
      if(FAILED(requestHr) || FAILED(sessionHr)) pending->swap(composition_);
    }
  }
  recoveryPending_=composition_.active();
  router_.reset();
  if(focus_) { focus_->Release(); focus_=nullptr; }
}
HRESULT STDMETHODCALLTYPE TsfService::Deactivate() {
  clearFocus();
  if(source_ && sinkCookie_!=TF_INVALID_COOKIE) {
    source_->UnadviseSink(sinkCookie_);
    sinkCookie_=TF_INVALID_COOKIE;
  }
  if(source_) { source_->Release(); source_=nullptr; }
  if(keys_) {
    if(keysAdvised_) keys_->UnadviseKeyEventSink(clientId_);
    keys_->Release(); keys_=nullptr;
  }
  keysAdvised_=false;
  if(manager_) { manager_->Release(); manager_=nullptr; }
  clientId_=TF_CLIENTID_NULL;
  ready_=false;
  return S_OK;
}
bool TsfService::sensitive(ITfContext* context) const {
  if(!ready_ || secureActivation_ || !context || recoveryPending_ ||
     HasSensitiveWindowStyle(context)) return true;
  TF_STATUS status{};
  if(FAILED(context->GetStatus(&status))) return true;
  if(status.dwDynamicFlags & TF_SD_READONLY) return true;
  ITfCompartmentMgr* compartments=nullptr;
  if(SUCCEEDED(context->QueryInterface(IID_ITfCompartmentMgr,
      reinterpret_cast<void**>(&compartments))) && compartments) {
    ITfCompartment* compartment=nullptr;
    if(SUCCEEDED(compartments->GetCompartment(GUID_COMPARTMENT_KEYBOARD_DISABLED,
        &compartment)) && compartment) {
      VARIANT value; VariantInit(&value);
      const HRESULT hr=compartment->GetValue(&value);
      const bool disabled=SUCCEEDED(hr) && value.vt==VT_I4 && value.lVal!=0;
      VariantClear(&value);
      compartment->Release();
      compartments->Release();
      return disabled;
    }
    compartments->Release();
  }
  return false;
}
TsfKeyEvent TsfService::event(WPARAM key) const {
  const bool shift=(GetKeyState(VK_SHIFT)&0x8000)!=0;
  const bool ctrl=(GetKeyState(VK_CONTROL)&0x8000)!=0;
  const bool alt=(GetKeyState(VK_MENU)&0x8000)!=0;
  char character=0;
  if(key>='A' && key<='Z') character=static_cast<char>(key+(shift?0:('a'-'A')));
  return {static_cast<unsigned>(key),character,shift,ctrl,alt};
}
void TsfService::updateCandidates(ITfContext* context,TfEditCookie cookie,
    const TsfKeyResult& result) {
  if(result.raw.empty() || result.candidates.empty()) { candidates_.hide(); return; }
  ITfContextView* view=nullptr;
  if(FAILED(context->GetActiveView(&view)) || !view) { candidates_.hide(); return; }
  HWND host=nullptr;
  view->GetWnd(&host);
  RECT caret{};
  bool valid=false;
  ITfRange* range=nullptr;
  if(SUCCEEDED(composition_.currentRange(&range)) && range) {
    BOOL clipped=FALSE;
    if(SUCCEEDED(view->GetTextExt(cookie,range,&caret,&clipped)) &&
       !clipped && (caret.right>caret.left || caret.bottom>caret.top)) valid=true;
    range->Release();
  }
  if(!valid && host) {
    GUITHREADINFO info{};info.cbSize=sizeof(info);
    if(GetGUIThreadInfo(0,&info) && info.hwndCaret &&
       (info.hwndCaret==host || IsChild(host,info.hwndCaret))) {
      caret=info.rcCaret;
      POINT a{caret.left,caret.top},b{caret.right,caret.bottom};
      ClientToScreen(info.hwndCaret,&a);ClientToScreen(info.hwndCaret,&b);
      caret={a.x,a.y,b.x,b.y};
      valid=true;
    }
  }
  candidates_.update(result,caret,host,valid);
  view->Release();
}
HRESULT STDMETHODCALLTYPE TsfService::OnSetFocus(BOOL foreground) {
  if(!foreground) clearFocus();
  return S_OK;
}
HRESULT STDMETHODCALLTYPE TsfService::OnTestKeyDown(ITfContext* context,WPARAM key,LPARAM,BOOL* eaten) {
  if(!eaten) return E_POINTER;
  *eaten=FALSE;
  if(sensitive(context)) { clearFocus(); return S_OK; }
  *eaten=gate_.interested(event(key),false)?TRUE:FALSE;
  return S_OK;
}
HRESULT STDMETHODCALLTYPE TsfService::OnTestKeyUp(ITfContext*,WPARAM,LPARAM,BOOL* eaten) {
  if(!eaten) return E_POINTER;
  *eaten=FALSE; return S_OK;
}
HRESULT STDMETHODCALLTYPE TsfService::OnKeyDown(ITfContext* context,WPARAM key,LPARAM,BOOL* eaten) {
  if(!eaten) return E_POINTER;
  *eaten=FALSE;
  if(sensitive(context)) { clearFocus(); return S_OK; }
  const TsfKeyEvent keyEvent=event(key);
  if(!gate_.interested(keyEvent,false)) return S_OK;
  TsfKeyResult result;
  auto* edit=new(std::nothrow) EditSession([&](TfEditCookie cookie) {
    if(HasSensitiveInputScope(context,cookie)) {
      candidates_.hide();
      router_.reset();
      const HRESULT cancelled=composition_.cancel(context,cookie);
      if(FAILED(cancelled)) recoveryPending_=true;
      return cancelled;
    }
    result=router_.route(keyEvent,false);
    if(!result.handled) return S_OK;
    const HRESULT hr=composition_.apply(context,cookie,result);
    if(FAILED(hr)) {
      result.handled=false;
      router_.reset();
      candidates_.hide();
      if(FAILED(composition_.cancel(context,cookie))) recoveryPending_=true;
    } else {
      updateCandidates(context,cookie,result);
    }
    return hr;
  });
  if(!edit) return E_OUTOFMEMORY;
  HRESULT sessionHr=E_FAIL;
  const HRESULT requestHr=context->RequestEditSession(clientId_,edit,
    TF_ES_SYNC|TF_ES_READWRITE,&sessionHr);
  edit->Release();
  if(FAILED(requestHr) || FAILED(sessionHr) || recoveryPending_) {
    clearFocus();
    return S_OK;
  }
  if(SUCCEEDED(requestHr) && SUCCEEDED(sessionHr) && result.handled) *eaten=TRUE;
  return S_OK;
}
HRESULT STDMETHODCALLTYPE TsfService::OnKeyUp(ITfContext*,WPARAM,LPARAM,BOOL* eaten) {
  if(!eaten) return E_POINTER;
  *eaten=FALSE; return S_OK;
}
HRESULT STDMETHODCALLTYPE TsfService::OnPreservedKey(ITfContext*,REFGUID,BOOL* eaten) {
  if(!eaten) return E_POINTER;
  *eaten=FALSE; return S_OK;
}
HRESULT STDMETHODCALLTYPE TsfService::OnInitDocumentMgr(ITfDocumentMgr*) { return S_OK; }
HRESULT STDMETHODCALLTYPE TsfService::OnUninitDocumentMgr(ITfDocumentMgr*) { clearFocus(); return S_OK; }
HRESULT STDMETHODCALLTYPE TsfService::OnSetFocus(ITfDocumentMgr* current,ITfDocumentMgr*) {
  clearFocus();
  if(current) current->GetTop(&focus_);
  return S_OK;
}
HRESULT STDMETHODCALLTYPE TsfService::OnPushContext(ITfContext*) { return S_OK; }
HRESULT STDMETHODCALLTYPE TsfService::OnPopContext(ITfContext* context) {
  if(context==focus_ || context==composition_.context()) clearFocus();
  return S_OK;
}
}
