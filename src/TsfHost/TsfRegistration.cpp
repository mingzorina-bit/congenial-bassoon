#include "TsfIdentity.h"
#include <msctf.h>
#include <filesystem>
#include <cwchar>
#include <iostream>
#include <string>

namespace {
constexpr wchar_t kComKey[]=L"Software\\Classes\\CLSID\\{8A02E78F-A832-44AF-9A7F-120BD4851479}";
constexpr LANGID kLanguage=MAKELANGID(LANG_CHINESE,SUBLANG_CHINESE_SIMPLIFIED);

bool setString(HKEY key,const wchar_t* name,const std::wstring& value) {
  return RegSetValueExW(key,name,0,REG_SZ,
    reinterpret_cast<const BYTE*>(value.c_str()),
    static_cast<DWORD>((value.size()+1)*sizeof(wchar_t)))==ERROR_SUCCESS;
}
bool addComServer(const std::wstring& dll,bool& created) {
  created=false;
  HKEY classKey=nullptr;
  DWORD disposition=0;
  if(RegCreateKeyExW(HKEY_CURRENT_USER,kComKey,0,nullptr,0,KEY_READ|KEY_WRITE,
      nullptr,&classKey,&disposition)!=ERROR_SUCCESS) return false;
  if(disposition!=REG_CREATED_NEW_KEY) { RegCloseKey(classKey); return false; }
  created=true;
  if(!setString(classKey,L"BilingualInputTrialOwner",L"Phase1A")) {
    RegCloseKey(classKey);return false;
  }
  HKEY serverKey=nullptr;
  const LONG opened=RegCreateKeyExW(classKey,L"InprocServer32",0,nullptr,0,
    KEY_READ|KEY_WRITE,nullptr,&serverKey,nullptr);
  bool okay=false;
  if(opened==ERROR_SUCCESS) {
    okay=setString(serverKey,nullptr,dll) &&
      setString(serverKey,L"ThreadingModel",L"Apartment");
    RegCloseKey(serverKey);
  }
  RegCloseKey(classKey);
  return okay;
}
bool ownsComServer() {
  HKEY key=nullptr;
  if(RegOpenKeyExW(HKEY_CURRENT_USER,kComKey,0,KEY_READ,&key)!=ERROR_SUCCESS) return false;
  wchar_t owner[64]{};
  DWORD type=0,length=sizeof(owner);
  const LONG read=RegQueryValueExW(key,L"BilingualInputTrialOwner",nullptr,&type,
    reinterpret_cast<BYTE*>(owner),&length);
  RegCloseKey(key);
  return read==ERROR_SUCCESS && type==REG_SZ && std::wstring(owner)==L"Phase1A";
}
bool removeComServer() {
  return ownsComServer() && RegDeleteTreeW(HKEY_CURRENT_USER,kComKey)==ERROR_SUCCESS;
}

HRESULT profiles(ITfInputProcessorProfiles** output) {
  return CoCreateInstance(CLSID_TF_InputProcessorProfiles,nullptr,CLSCTX_INPROC_SERVER,
    IID_ITfInputProcessorProfiles,reinterpret_cast<void**>(output));
}
HRESULT categories(ITfCategoryMgr** output) {
  return CoCreateInstance(CLSID_TF_CategoryMgr,nullptr,CLSCTX_INPROC_SERVER,
    IID_ITfCategoryMgr,reinterpret_cast<void**>(output));
}
bool includesGuid(IEnumGUID* items,REFGUID target,bool& present) {
  if(!items) return false;
  present=false;
  GUID value{};
  ULONG fetched=0;
  HRESULT hr=S_OK;
  while((hr=items->Next(1,&value,&fetched))==S_OK) {
    if(fetched!=1) return false;
    if(IsEqualGUID(value,target)) { present=true; return true; }
  }
  return hr==S_FALSE;
}
bool categoryPresent(ITfCategoryMgr* manager,bool& present) {
  IEnumGUID* items=nullptr;
  if(FAILED(manager->EnumItemsInCategory(GUID_TFCAT_TIP_KEYBOARD,&items)) || !items)
    return false;
  const bool known=includesGuid(items,bilingual::tsf::kServiceClsid,present);
  items->Release();
  return known;
}
bool profilePresent(ITfInputProcessorProfiles* manager,bool& present) {
  IEnumGUID* items=nullptr;
  if(FAILED(manager->EnumInputProcessorInfo(&items)) || !items) return false;
  const bool known=includesGuid(items,bilingual::tsf::kServiceClsid,present);
  items->Release();
  return known;
}
bool removeTsf() {
  bool okay=true;
  ITfCategoryMgr* category=nullptr;
  if(SUCCEEDED(categories(&category)) && category) {
    category->UnregisterCategory(bilingual::tsf::kServiceClsid,
      GUID_TFCAT_TIP_KEYBOARD,bilingual::tsf::kServiceClsid);
    bool present=true;
    if(!categoryPresent(category,present) || present) okay=false;
    category->Release();
  } else okay=false;
  ITfInputProcessorProfiles* profile=nullptr;
  if(SUCCEEDED(profiles(&profile)) && profile) {
    profile->Unregister(bilingual::tsf::kServiceClsid);
    bool present=true;
    if(!profilePresent(profile,present) || present) okay=false;
    profile->Release();
  } else okay=false;
  return okay;
}
bool addTsf() {
  ITfInputProcessorProfiles* profile=nullptr;
  if(FAILED(profiles(&profile)) || !profile) return false;
  HRESULT hr=profile->Register(bilingual::tsf::kServiceClsid);
  constexpr wchar_t description[]=L"Bilingual Input TSF Trial";
  if(SUCCEEDED(hr)) hr=profile->AddLanguageProfile(
    bilingual::tsf::kServiceClsid,kLanguage,bilingual::tsf::kProfileGuid,
    description,static_cast<ULONG>(wcslen(description)),nullptr,0,0);
  if(SUCCEEDED(hr)) hr=profile->EnableLanguageProfile(
    bilingual::tsf::kServiceClsid,kLanguage,bilingual::tsf::kProfileGuid,TRUE);
  profile->Release();
  if(FAILED(hr)) return false;
  ITfCategoryMgr* category=nullptr;
  if(FAILED(categories(&category)) || !category) return false;
  hr=category->RegisterCategory(bilingual::tsf::kServiceClsid,
    GUID_TFCAT_TIP_KEYBOARD,bilingual::tsf::kServiceClsid);
  category->Release();
  return SUCCEEDED(hr);
}
}

int wmain(int argc,wchar_t** argv) {
  if(argc==2 && std::wstring(argv[1])==L"plan") {
    std::wcout<<L"CLSID {8A02E78F-A832-44AF-9A7F-120BD4851479}\n"
      <<L"Profile {40E7CBF8-40C3-44F7-9031-1D0CC66138A9}\n"
      <<L"COM registry HKCU\\Software\\Classes\\CLSID\n"
      <<L"TSF zh-CN keyboard profile; no automatic activation\n";
    return 0;
  }
  if(argc<2) return 2;
  const bool adding=std::wstring(argv[1])==L"register";
  const bool removing=std::wstring(argv[1])==L"unregister";
  if((adding && argc!=3) || (removing && argc!=2) || (!adding && !removing)) return 2;
  std::wstring dll;
  if(adding) {
    try {
      const auto path=std::filesystem::canonical(argv[2]);
      if(path.extension()!=L".dll") return 2;
      dll=path.wstring();
    } catch(...) { return 2; }
  }
  const HRESULT initialized=CoInitializeEx(nullptr,COINIT_APARTMENTTHREADED);
  if(FAILED(initialized)) return 3;
  int status=0;
  if(adding) {
    bool created=false;
    if(!addComServer(dll,created)) {
      if(created) RegDeleteTreeW(HKEY_CURRENT_USER,kComKey);
      status=4;
    } else if(!addTsf()) {
      if(removeTsf()) removeComServer();
      status=4;
    }
  } else {
    if(!ownsComServer()) status=5;
    else {
      if(!removeTsf() || !removeComServer()) status=5;
    }
  }
  CoUninitialize();
  return status;
}
