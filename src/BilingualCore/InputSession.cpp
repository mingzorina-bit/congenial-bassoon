#include "InputSession.h"
namespace bilingual {
void InputSession::type(char ch) {
  if ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || ch == '\'') {
    raw_ += ch;
    refresh();
  }
}
bool InputSession::press(Key key, bool shift) {
  if (raw_.empty()) return false;
  if (shift && key == Key::Enter) { selectShadow(0); return true; }
  switch (key) {
    case Key::Enter: commit(raw_); break;
    case Key::Space:
      if (candidates_.empty()) commit(raw_); else select(highlighted_);
      break;
    case Key::Backspace: raw_.pop_back(); refresh(); break;
    case Key::Left: case Key::Up:
      if (highlighted_ > 0) --highlighted_;
      break;
    case Key::Right: case Key::Down:
      if (highlighted_ + 1 < static_cast<int>(candidates_.size())) ++highlighted_;
      break;
    case Key::Escape: raw_.clear(); refresh(); break;
  }
  return true;
}
bool InputSession::select(int index) {
  if (index < 0 || index >= static_cast<int>(candidates_.size())) return false;
  commit(candidates_[index]);
  return true;
}
void InputSession::highlight(int index) {
  if (index >= 0 && index < static_cast<int>(candidates_.size())) highlighted_ = index;
}
void InputSession::setPrivacy(PrivacyMode mode) { mode_ = mode; refresh(); }
std::vector<ShadowCandidate> InputSession::shadows() const {
 if(!lexical_||mode_==PrivacyMode::Secure||highlighted_<0||highlighted_>=static_cast<int>(candidates_.size()))return {};
 try{return lexical_->lookup(candidates_[highlighted_]);}catch(...){return {};}
}
bool InputSession::selectShadow(int index) {
 auto items=shadows();if(index<0||index>=static_cast<int>(items.size()))return false;
 commit(items[index].text);return true;
}
std::string InputSession::takeCommit() { auto text = pending_; pending_.clear(); return text; }
void InputSession::refresh() {
  candidates_.clear(); highlighted_ = 0;
  if (raw_.empty() || mode_ == PrivacyMode::Secure) return;
  try { candidates_ = engine_.query(raw_); } catch (...) { candidates_.clear(); }
}
void InputSession::commit(const std::string& text) {
  pending_ += text; // Copy before clearing raw/candidates: text may reference either.
  raw_.clear(); candidates_.clear(); highlighted_ = 0;
}
}
