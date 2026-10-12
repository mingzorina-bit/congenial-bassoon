#include "TsfKeyRouter.h"
#include "TsfEditGate.h"
#include <functional>
#include <iostream>
#include <stdexcept>
using namespace bilingual;
using namespace bilingual::tsf;

struct Engine final: PrimaryEngine {
  std::vector<std::string> query(const std::string& raw) override {
    if(raw=="nihao") return {"你好","拟好"};
    if(raw=="beijing") return {"北京","背景"};
    return {};
  }
};
void check(bool result) { if(!result) throw std::runtime_error("assertion"); }
TsfKeyEvent letter(char c) { return {static_cast<unsigned>(c), c, false, false, false}; }
void type(TsfKeyRouter& router,const char* word) { for(;*word;++word) router.route(letter(*word),false); }
int main() {
  int total=0,failures=0;
  auto test=[&](const char* name,std::function<void()> run) {
    ++total;try { run();std::cout<<"PASS "<<name<<"\n"; }
    catch(...) { ++failures;std::cout<<"FAIL "<<name<<"\n"; }
  };
  test("SpaceCommitsChinese",[]{Engine engine;InputSession session(engine);session.setContext("","ZH");TsfKeyRouter router(session);type(router,"nihao");auto result=router.route({0x20,' ',false,false,false},false);check(result.handled&&result.commit=="你好"&&result.raw.empty());});
  test("EnterCommitsRaw",[]{Engine engine;InputSession session(engine);session.setContext("","ZH");TsfKeyRouter router(session);type(router,"beijing");auto result=router.route({0x0D,0,false,false,false},false);check(result.handled&&result.commit=="beijing");});
  test("ArrowHighlightsSecondCandidate",[]{Engine engine;InputSession session(engine);session.setContext("","ZH");TsfKeyRouter router(session);type(router,"nihao");auto result=router.route({0x27,0,false,false,false},false);check(result.handled&&result.highlighted==1&&result.candidates[1]=="拟好");result=router.route({0x20,' ',false,false,false},false);check(result.commit=="拟好");});
  test("EscapeCancelsWithoutCommit",[]{Engine engine;InputSession session(engine);session.setContext("","ZH");TsfKeyRouter router(session);type(router,"nihao");auto result=router.route({0x1B,0,false,false,false},false);check(result.handled&&result.cancelled&&result.commit.empty()&&result.raw.empty());});
  test("EnglishWordsCanCommitRaw",[]{Engine engine;InputSession session(engine);session.setContext("","ZH");TsfKeyRouter router(session);type(router,"computer");auto result=router.route({0x20,' ',false,false,false},false);check(result.handled&&result.commit=="computer");});
  test("TabAndModifiedKeysPassThrough",[]{Engine engine;InputSession session(engine);session.setContext("","ZH");TsfKeyRouter router(session);check(!router.route({0x09,0,false,false,false},false).handled);check(!router.route({'A','a',false,true,false},false).handled);check(!router.route({0x0D,0,true,false,false},false).handled);});
  test("SensitiveContextNeverQueriesOrRetains",[]{Engine engine;InputSession session(engine);session.setContext("","ZH");TsfKeyRouter router(session);type(router,"nihao");auto result=router.route(letter('a'),true);check(!result.handled&&result.raw.empty()&&result.candidates.empty());});
  test("FocusResetCancelsComposition",[]{Engine engine;InputSession session(engine);session.setContext("","ZH");TsfKeyRouter router(session);type(router,"nihao");router.reset();auto result=router.snapshot();check(result.raw.empty()&&result.candidates.empty()&&result.commit.empty());});
  test("DeniedEditDoesNotConsumeOrMutate",[]{Engine engine;InputSession session(engine);session.setContext("","ZH");TsfKeyRouter router(session);TsfEditGate gate(router);auto result=gate.dispatch(letter('n'),false,[](const std::function<void()>&){return false;});check(!result.handled&&router.snapshot().raw.empty());});
  test("GrantedEditRoutesInsideCallback",[]{Engine engine;InputSession session(engine);session.setContext("","ZH");TsfKeyRouter router(session);TsfEditGate gate(router);bool inside=false;auto result=gate.dispatch(letter('n'),false,[&](const std::function<void()>& apply){inside=true;apply();inside=false;return true;});check(result.handled&&result.raw=="n"&&!inside);});
  std::cout<<total<<" tests, "<<failures<<" failures\n";
  return failures?1:0;
}

