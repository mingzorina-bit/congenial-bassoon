#include "TsfSensitivity.h"
#include <stdexcept>
#include <iostream>
using namespace bilingual::tsf;
void check(bool condition) { if(!condition) throw std::runtime_error("sensitivity assertion"); }
int main() {
  check(IsSensitiveInputScope(IS_PASSWORD));
  check(IsSensitiveInputScope(static_cast<InputScope>(63)));
  check(!IsSensitiveInputScope(IS_DEFAULT));
  check(!IsSensitiveInputScope(IS_EMAIL_SMTPEMAILADDRESS));
  HINSTANCE instance=GetModuleHandleW(nullptr);
  HWND password=CreateWindowExW(0,L"EDIT",L"",WS_POPUP|ES_PASSWORD,
    0,0,10,10,nullptr,nullptr,instance,nullptr);
  HWND normal=CreateWindowExW(0,L"EDIT",L"",WS_POPUP,
    0,0,10,10,nullptr,nullptr,instance,nullptr);
  check(password && normal);
  check(IsSensitiveWindow(password));
  check(!IsSensitiveWindow(normal));
  check(!IsSensitiveWindow(nullptr));
  DestroyWindow(password);DestroyWindow(normal);
  std::cout<<"PASS TSF sensitive scope and native password style\n";
}
