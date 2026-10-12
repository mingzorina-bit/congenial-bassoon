#include "TsfCandidateWindow.h"
#include "TsfIdentity.h"
#include <algorithm>
#include <string>

namespace bilingual::tsf {
namespace {
constexpr wchar_t kWindowClass[]=L"BilingualInputTsfTrialCandidates";
std::wstring wide(const std::string& text) {
  if(text.empty()) return {};
  const int count=MultiByteToWideChar(CP_UTF8,MB_ERR_INVALID_CHARS,text.data(),
    static_cast<int>(text.size()),nullptr,0);
  if(count<=0) return {};
  std::wstring value(static_cast<size_t>(count),L'\0');
  if(!MultiByteToWideChar(CP_UTF8,MB_ERR_INVALID_CHARS,text.data(),
      static_cast<int>(text.size()),value.data(),count)) return {};
  return value;
}
}
TsfCandidateWindow::~TsfCandidateWindow() { hide(); }
bool TsfCandidateWindow::create(HWND host) {
  if(window_ && IsWindow(window_)) return true;
  WNDCLASSEXW type{};
  type.cbSize=sizeof(type);
  type.lpfnWndProc=WindowProc;
  type.hInstance=ModuleHandle();
  type.hCursor=LoadCursorW(nullptr,IDC_ARROW);
  type.lpszClassName=kWindowClass;
  if(!RegisterClassExW(&type) && GetLastError()!=ERROR_CLASS_ALREADY_EXISTS) return false;
  window_=CreateWindowExW(WS_EX_NOACTIVATE|WS_EX_TOOLWINDOW|WS_EX_TOPMOST,
    kWindowClass,L"",WS_POPUP,0,0,1,1,host,nullptr,ModuleHandle(),this);
  return window_!=nullptr;
}
void TsfCandidateWindow::hide() {
  state_.hide();
  if(window_ && IsWindow(window_)) DestroyWindow(window_);
  window_=nullptr;
}
void TsfCandidateWindow::update(const TsfKeyResult& result,const RECT& caret,
    HWND host,bool geometryAvailable) {
  const bool visible=host && IsWindowVisible(host);
  state_.update(result,visible,geometryAvailable);
  if(!state_.visible()) { hide(); return; }
  if(!create(host)) { hide(); return; }
  const int rowHeight=28;
  const int width=420;
  const int height=static_cast<int>(state_.entries().size())*rowHeight+8;
  RECT monitorRect{};
  MONITORINFO monitor{};monitor.cbSize=sizeof(monitor);
  if(GetMonitorInfoW(MonitorFromRect(&caret,MONITOR_DEFAULTTONEAREST),&monitor))
    monitorRect=monitor.rcWork;
  else { monitorRect.left=0;monitorRect.top=0;monitorRect.right=GetSystemMetrics(SM_CXSCREEN);monitorRect.bottom=GetSystemMetrics(SM_CYSCREEN); }
  const int x=std::clamp(caret.left,monitorRect.left,std::max(monitorRect.left,monitorRect.right-width));
  int y=caret.bottom+2;
  if(y+height>monitorRect.bottom) y=caret.top-height-2;
  y=std::clamp(y,monitorRect.top,std::max(monitorRect.top,monitorRect.bottom-height));
  SetWindowPos(window_,HWND_TOPMOST,x,y,width,height,
    SWP_NOACTIVATE|SWP_SHOWWINDOW);
  InvalidateRect(window_,nullptr,TRUE);
}
LRESULT TsfCandidateWindow::paint(HWND window) {
  PAINTSTRUCT paint{};
  HDC dc=BeginPaint(window,&paint);
  RECT bounds{};GetClientRect(window,&bounds);
  HBRUSH background=CreateSolidBrush(RGB(31,31,31));
  FillRect(dc,&bounds,background);
  DeleteObject(background);
  SetBkMode(dc,TRANSPARENT);
  SelectObject(dc,GetStockObject(DEFAULT_GUI_FONT));
  for(size_t i=0;i<state_.entries().size();++i) {
    RECT row{4,4+static_cast<LONG>(i*28),bounds.right-4,4+static_cast<LONG>((i+1)*28)};
    if(static_cast<int>(i)==state_.highlighted()) {
      HBRUSH selected=CreateSolidBrush(RGB(102,208,217));
      FillRect(dc,&row,selected);
      DeleteObject(selected);
      SetTextColor(dc,RGB(16,26,29));
    } else SetTextColor(dc,RGB(245,245,245));
    std::wstring label=std::to_wstring(i+1)+L"  "+wide(state_.entries()[i]);
    row.left+=12;
    DrawTextW(dc,label.c_str(),-1,&row,DT_LEFT|DT_VCENTER|DT_SINGLELINE|DT_END_ELLIPSIS);
  }
  EndPaint(window,&paint);
  return 0;
}
LRESULT CALLBACK TsfCandidateWindow::WindowProc(HWND window,UINT message,
    WPARAM wparam,LPARAM lparam) {
  if(message==WM_NCCREATE) {
    auto* created=reinterpret_cast<CREATESTRUCTW*>(lparam);
    SetWindowLongPtrW(window,GWLP_USERDATA,
      reinterpret_cast<LONG_PTR>(created->lpCreateParams));
  }
  auto* self=reinterpret_cast<TsfCandidateWindow*>(GetWindowLongPtrW(window,GWLP_USERDATA));
  if(message==WM_MOUSEACTIVATE) return MA_NOACTIVATE;
  if(message==WM_NCHITTEST) return HTTRANSPARENT;
  if(message==WM_PAINT && self) return self->paint(window);
  if(message==WM_NCDESTROY && self) self->window_=nullptr;
  return DefWindowProcW(window,message,wparam,lparam);
}
}
