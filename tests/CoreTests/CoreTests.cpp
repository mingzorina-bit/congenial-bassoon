#include "InputSession.h"
#include <functional>
#include <iostream>
#include <stdexcept>
using namespace bilingual;
struct FixtureEngine: PrimaryEngine {
 bool broken = false;
 int calls = 0;
 std::vector<std::string> query(const std::string& raw) override {
  ++calls;
  if (broken) throw std::runtime_error("simulated failure");
  if (raw == "youhua") return {"优化", "油画", "有话"};
  if (raw == "xuexi") return {"学习"};
  return {};
 }
};
void require(bool condition) { if(!condition) throw std::runtime_error("assertion failed"); }
void type(InputSession& session, const std::string& text) { for(char c:text) session.type(c); }
int main() {
 int failures=0, count=0;
 auto test=[&](const char* name, std::function<void()> run) {
  ++count; try { run(); std::cout<<"PASS "<<name<<"\n"; }
  catch(...) { ++failures; std::cout<<"FAIL "<<name<<"\n"; }
 };
 test("EnterPreservesPinyin", [] { FixtureEngine e; InputSession s(e); type(s,"youhua"); s.press(Key::Enter); require(s.takeCommit()=="youhua" && s.raw().empty()); });
 test("EnterPreservesUnknown", [] { FixtureEngine e; InputSession s(e); type(s,"asdfg"); s.press(Key::Enter); require(s.takeCommit()=="asdfg"); });
 test("SpaceUsesHighlight", [] { FixtureEngine e; InputSession s(e); type(s,"youhua"); s.press(Key::Right); s.press(Key::Space); require(s.takeCommit()=="油画"); });
 test("SelectionExactlyOnce", [] { FixtureEngine e; InputSession s(e); type(s,"youhua"); require(s.select(2)); require(s.takeCommit()=="有话"); require(s.takeCommit().empty()); });
 test("MouseHighlightMatchesKeyboard", [] { FixtureEngine e; InputSession s(e); type(s,"youhua"); s.highlight(1); s.press(Key::Space); require(s.takeCommit()=="油画"); });
 test("InvalidIndexDoesNotCommit", [] { FixtureEngine e; InputSession s(e); type(s,"youhua"); require(!s.select(-1) && !s.select(12)); require(s.raw()=="youhua" && s.takeCommit().empty()); });
 test("EmptySessionPassesKeysToEditor", [] { FixtureEngine e; InputSession s(e); require(!s.press(Key::Enter) && !s.press(Key::Space) && !s.press(Key::Backspace)); });
 test("NewCompositionHasNoOldState", [] { FixtureEngine e; InputSession s(e); type(s,"youhua"); s.select(1); require(s.takeCommit()=="油画"); type(s,"xuexi"); require(s.highlighted()==0 && s.candidates().size()==1); s.press(Key::Space); require(s.takeCommit()=="学习"); });
 test("FailureKeepsRawInput", [] { FixtureEngine e; e.broken=true; InputSession s(e); type(s,"youhua"); s.press(Key::Enter); require(s.takeCommit()=="youhua"); });
 test("BackspaceRefreshesCandidates", [] { FixtureEngine e; InputSession s(e); type(s,"youhuax"); s.press(Key::Backspace); require(s.raw()=="youhua" && s.candidates().size()==3); });
 test("ReservedShiftDoesNotCommitPrimary", [] { FixtureEngine e; InputSession s(e); type(s,"youhua"); require(s.press(Key::Enter,true)); require(s.raw()=="youhua" && s.takeCommit().empty()); });
 test("SecureClearsSuggestionsAndAvoidsEngine", [] { FixtureEngine e; InputSession s(e); type(s,"youhua"); s.setPrivacy(PrivacyMode::Secure); int before=e.calls; type(s,"x"); require(s.candidates().empty() && e.calls==before); s.press(Key::Enter); require(s.takeCommit()=="youhuax"); });
 test("PrivateStillAllowsLocalCandidates", [] { FixtureEngine e; InputSession s(e); s.setPrivacy(PrivacyMode::Private); type(s,"youhua"); require(!s.candidates().empty()); });
 test("PrivacyPolicyBlocksSideEffects", [] { for(auto m:{PrivacyMode::Secure,PrivacyMode::Private}) { require(!PrivacyPolicy::cloud(m) && !PrivacyPolicy::learning(m) && !PrivacyPolicy::contentLogging(m) && !PrivacyPolicy::persistentTextCache(m)); } });
 std::cout<<count<<" tests, "<<failures<<" failures\n";
 return failures ? 1:0;
}
