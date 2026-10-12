#include <windows.h>
#include <msctf.h>
#include <iostream>
#include <string>

int wmain(int argc,wchar_t** argv) {
  if(argc!=2) return 2;
  HMODULE module=LoadLibraryW(argv[1]);
  if(!module) { std::cerr<<"TSF module missing\n";return 1; }
  using GetClassObject=HRESULT (STDAPICALLTYPE*)(REFCLSID,REFIID,void**);
  using CanUnloadNow=HRESULT (STDAPICALLTYPE*)();
  auto getClass=reinterpret_cast<GetClassObject>(GetProcAddress(module,"DllGetClassObject"));
  auto canUnload=reinterpret_cast<CanUnloadNow>(GetProcAddress(module,"DllCanUnloadNow"));
  if(!getClass||!canUnload) { FreeLibrary(module);return 1; }
  const CLSID clsid={0x8a02e78f,0xa832,0x44af,{0x9a,0x7f,0x12,0x0b,0xd4,0x85,0x14,0x79}};
  IClassFactory* factory=nullptr;
  if(FAILED(getClass(clsid,IID_IClassFactory,reinterpret_cast<void**>(&factory)))||!factory) { FreeLibrary(module);return 1; }
  ITfTextInputProcessor* processor=nullptr;
  HRESULT hr=factory->CreateInstance(nullptr,IID_ITfTextInputProcessor,reinterpret_cast<void**>(&processor));
  factory->Release();
  if(FAILED(hr)||!processor) { FreeLibrary(module);return 1; }
  ITfKeyEventSink* keys=nullptr;
  ITfThreadMgrEventSink* focus=nullptr;
  hr=processor->QueryInterface(IID_ITfKeyEventSink,reinterpret_cast<void**>(&keys));
  if(SUCCEEDED(hr)) hr=processor->QueryInterface(IID_ITfThreadMgrEventSink,reinterpret_cast<void**>(&focus));
  if(keys) keys->Release();
  if(focus) focus->Release();
  processor->Release();
  const bool ok=SUCCEEDED(hr)&&canUnload()==S_OK;
  FreeLibrary(module);
  if(!ok) { std::cerr<<"TSF class contract failed\n";return 1; }
  std::cout<<"PASS TSF COM exports and interfaces\n";
  return 0;
}

