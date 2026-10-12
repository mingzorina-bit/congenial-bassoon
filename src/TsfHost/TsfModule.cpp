#include "TsfIdentity.h"
#include "TsfService.h"
#include <atomic>
#include <new>

namespace bilingual::tsf {
namespace {
std::atomic<long> liveObjects{0};
std::atomic<long> serverLocks{0};
HMODULE moduleHandle=nullptr;

class Factory final : public IClassFactory {
 public:
  Factory() { ModuleObjectCreated(); }
  HRESULT STDMETHODCALLTYPE QueryInterface(REFIID iid,void** value) override {
    if(!value) return E_POINTER;
    *value=nullptr;
    if(IsEqualIID(iid,IID_IUnknown) || IsEqualIID(iid,IID_IClassFactory)) {
      *value=static_cast<IClassFactory*>(this); AddRef(); return S_OK;
    }
    return E_NOINTERFACE;
  }
  ULONG STDMETHODCALLTYPE AddRef() override { return ++references_; }
  ULONG STDMETHODCALLTYPE Release() override {
    const ULONG count=--references_;
    if(!count) delete this;
    return count;
  }
  HRESULT STDMETHODCALLTYPE CreateInstance(IUnknown* outer,REFIID iid,void** value) override {
    if(!value) return E_POINTER;
    *value=nullptr;
    if(outer) return CLASS_E_NOAGGREGATION;
    auto* service=new(std::nothrow) TsfService();
    if(!service) return E_OUTOFMEMORY;
    const HRESULT hr=service->QueryInterface(iid,value);
    service->Release();
    return hr;
  }
  HRESULT STDMETHODCALLTYPE LockServer(BOOL lock) override {
    if(lock) ++serverLocks; else --serverLocks;
    return S_OK;
  }
 private:
  ~Factory() { ModuleObjectDestroyed(); }
  std::atomic<ULONG> references_{1};
};
}
void ModuleObjectCreated() { ++liveObjects; }
void ModuleObjectDestroyed() { --liveObjects; }
HMODULE ModuleHandle() { return moduleHandle; }
void SetModuleHandle(HMODULE handle) { moduleHandle=handle; }
bool CanUnload() { return liveObjects.load()==0 && serverLocks.load()==0; }
IClassFactory* NewFactory() { return new(std::nothrow) Factory(); }
}

extern "C" BOOL WINAPI DllMain(HMODULE module,DWORD reason,LPVOID) {
  if(reason==DLL_PROCESS_ATTACH) {
    bilingual::tsf::SetModuleHandle(module);
    DisableThreadLibraryCalls(module);
  }
  return TRUE;
}
extern "C" __declspec(dllexport) HRESULT STDAPICALLTYPE DllGetClassObject(
    REFCLSID clsid,REFIID iid,void** value) {
  if(!value) return E_POINTER;
  *value=nullptr;
  if(!IsEqualCLSID(clsid,bilingual::tsf::kServiceClsid)) return CLASS_E_CLASSNOTAVAILABLE;
  IClassFactory* factory=bilingual::tsf::NewFactory();
  if(!factory) return E_OUTOFMEMORY;
  const HRESULT hr=factory->QueryInterface(iid,value);
  factory->Release();
  return hr;
}
extern "C" __declspec(dllexport) HRESULT STDAPICALLTYPE DllCanUnloadNow() {
  return bilingual::tsf::CanUnload()?S_OK:S_FALSE;
}
