#include "InputSession.h"
#include <functional>
#include <iostream>
#include <stdexcept>
using namespace bilingual;
struct Engine:PrimaryEngine { std::vector<std::string> query(const std::string& r) override {
 if(r=="youhua")return {"优化","油画","有话"};
 if(r=="youhuafangan")return {"优化方案"};
 if(r=="optimize")return {"optimize"};
 return {};
}};
void check(bool value){if(!value)throw std::runtime_error("assertion");}
void type(InputSession& s,const std::string& r){for(char c:r)s.type(c);}
int main(int argc,char**argv){if(argc!=2)return 2; int failed=0,count=0; LexicalStore lex(argv[1]);
 auto test=[&](const char*n,std::function<void()>f){++count;try{f();std::cout<<"PASS "<<n<<"\n";}catch(...){++failed;std::cout<<"FAIL "<<n<<"\n";}};
 test("WordThreeWithProvenance",[&]{auto v=lex.lookup("优化");check(v.size()==3&&v[0].text=="optimize"&&v[2].text=="refine"&&v[0].sourceId=="own-curated-v002"&&!v[0].entryId.empty());});
 test("HighlightReplacesShadow",[&]{Engine e;InputSession s(e);s.setLexical(&lex);type(s,"youhua");s.press(Key::Right);auto v=s.shadows();check(v.size()==1&&v[0].text=="oil painting");s.highlight(2);check(s.shadows().empty());});
 test("ShiftEnterCommitsShadow",[&]{Engine e;InputSession s(e);s.setLexical(&lex);type(s,"youhua");s.press(Key::Enter,true);check(s.takeCommit()=="optimize"&&s.raw().empty()&&s.shadows().empty());});
 test("ShiftIndexCommitsExactChoice",[&]{Engine e;InputSession s(e);s.setLexical(&lex);type(s,"youhua");check(!s.selectShadow(4)&&s.raw()=="youhua");check(s.selectShadow(2)&&s.takeCommit()=="refine");});
 test("PhraseUsesWholeEntry",[&]{Engine e;InputSession s(e);s.setLexical(&lex);type(s,"youhuafangan");s.press(Key::Enter,true);check(s.takeCommit()=="optimization plan");});
 test("NoMechanicalPhraseAssembly",[&]{check(lex.lookup("优化工作").empty());});
 test("MissingSourceKeepsPrimary",[&]{LexicalStore missing("missing.tsv");Engine e;InputSession s(e);s.setLexical(&missing);type(s,"youhua");s.press(Key::Enter,true);check(s.raw()=="youhua"&&s.takeCommit().empty());s.press(Key::Space);check(s.takeCommit()=="优化");});
 test("EnterStillRawWithShadow",[&]{Engine e;InputSession s(e);s.setLexical(&lex);type(s,"youhua");s.press(Key::Enter);check(s.takeCommit()=="youhua");});
 test("SecureSuppressesShadow",[&]{Engine e;InputSession s(e);s.setLexical(&lex);type(s,"youhua");s.setPrivacy(PrivacyMode::Secure);check(s.shadows().empty()&&!s.selectShadow(0));});
 test("PrivateKeepsLocalShadow",[&]{Engine e;InputSession s(e);s.setLexical(&lex);s.setPrivacy(PrivacyMode::Private);type(s,"youhua");check(!s.shadows().empty());});
 test("TargetLanguageIsNotHardcodedEnglish",[&]{Engine e;InputSession s(e);s.setLexical(&lex);type(s,"optimize");auto v=s.shadows();check(v.size()==1&&v[0].language=="ZH");s.press(Key::Enter,true);check(s.takeCommit()=="优化");});
 test("CancelClearsShadow",[&]{Engine e;InputSession s(e);s.setLexical(&lex);type(s,"youhua");s.press(Key::Escape);check(s.shadows().empty());});
 std::cout<<count<<" tests, "<<failed<<" failures\n";return failed?1:0;
}
