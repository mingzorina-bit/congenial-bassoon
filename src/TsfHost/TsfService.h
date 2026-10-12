#pragma once
#include "TsfComposition.h"
#include "TsfEditGate.h"
#include "TsfIdentity.h"
#include "TsfCandidateWindow.h"
#include <msctf.h>
#include <atomic>

namespace bilingual::tsf {
class ServiceEngine final : public PrimaryEngine {
 public:
  std::vector<std::string> query(const std::string& raw) override;
};

class TsfService final : public ITfTextInputProcessorEx,
                         public ITfKeyEventSink,
                         public ITfThreadMgrEventSink {
 public:
  TsfService();
  HRESULT STDMETHODCALLTYPE QueryInterface(REFIID iid,void** value) override;
  ULONG STDMETHODCALLTYPE AddRef() override;
  ULONG STDMETHODCALLTYPE Release() override;
  HRESULT STDMETHODCALLTYPE Activate(ITfThreadMgr* manager,TfClientId clientId) override;
  HRESULT STDMETHODCALLTYPE Deactivate() override;
  HRESULT STDMETHODCALLTYPE ActivateEx(ITfThreadMgr* manager,TfClientId clientId,DWORD flags) override;
  HRESULT STDMETHODCALLTYPE OnSetFocus(BOOL foreground) override;
  HRESULT STDMETHODCALLTYPE OnTestKeyDown(ITfContext* context,WPARAM key,LPARAM flags,BOOL* eaten) override;
  HRESULT STDMETHODCALLTYPE OnTestKeyUp(ITfContext* context,WPARAM key,LPARAM flags,BOOL* eaten) override;
  HRESULT STDMETHODCALLTYPE OnKeyDown(ITfContext* context,WPARAM key,LPARAM flags,BOOL* eaten) override;
  HRESULT STDMETHODCALLTYPE OnKeyUp(ITfContext* context,WPARAM key,LPARAM flags,BOOL* eaten) override;
  HRESULT STDMETHODCALLTYPE OnPreservedKey(ITfContext* context,REFGUID guid,BOOL* eaten) override;
  HRESULT STDMETHODCALLTYPE OnInitDocumentMgr(ITfDocumentMgr* manager) override;
  HRESULT STDMETHODCALLTYPE OnUninitDocumentMgr(ITfDocumentMgr* manager) override;
  HRESULT STDMETHODCALLTYPE OnSetFocus(ITfDocumentMgr* current,ITfDocumentMgr* previous) override;
  HRESULT STDMETHODCALLTYPE OnPushContext(ITfContext* context) override;
  HRESULT STDMETHODCALLTYPE OnPopContext(ITfContext* context) override;

 private:
  ~TsfService();
  bool sensitive(ITfContext* context) const;
  TsfKeyEvent event(WPARAM key) const;
  void updateCandidates(ITfContext* context,TfEditCookie cookie,const TsfKeyResult& result);
  void clearFocus();
  std::atomic<ULONG> references_{1};
  ITfThreadMgr* manager_=nullptr;
  ITfKeystrokeMgr* keys_=nullptr;
  ITfSource* source_=nullptr;
  ITfContext* focus_=nullptr;
  DWORD sinkCookie_=TF_INVALID_COOKIE;
  TfClientId clientId_=TF_CLIENTID_NULL;
  bool keysAdvised_=false;
  bool secureActivation_=false;
  bool ready_=false;
  bool recoveryPending_=false;
  bool cleanupQueued_=false;
  bool destroying_=false;
  ServiceEngine engine_;
  InputSession session_{engine_};
  TsfKeyRouter router_{session_};
  TsfEditGate gate_{router_};
  TsfComposition composition_;
  TsfCandidateWindow candidates_;
};
}
