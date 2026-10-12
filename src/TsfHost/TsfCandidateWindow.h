#pragma once
#include "TsfCandidateState.h"
#include <windows.h>

namespace bilingual::tsf {
class TsfCandidateWindow {
 public:
  ~TsfCandidateWindow();
  void update(const TsfKeyResult& result,const RECT& caret,HWND host,bool geometryAvailable);
  void hide();
 private:
  static LRESULT CALLBACK WindowProc(HWND window,UINT message,WPARAM wparam,LPARAM lparam);
  LRESULT paint(HWND window);
  bool create(HWND host);
  HWND window_=nullptr;
  TsfCandidateState state_;
};
}
