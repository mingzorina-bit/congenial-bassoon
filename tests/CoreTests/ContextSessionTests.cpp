#include "InputSession.h"
#include <functional>
#include <iostream>
#include <stdexcept>
using namespace bilingual;
struct Engine:PrimaryEngine {std::string queried;std::vector<std::string> query(const std::string& s)override{queried=s;if(s=="youhua")return {"优化","油画"};if(s=="he")return {"和"};if(s=="computer")return {"错误拼音候选"};return {};}};
void check(bool b){if(!b)throw std::runtime_error("assertion");}
void type(InputSession& s,std::string text){for(char c:text)s.type(c);}
int main(int argc,char**argv){if(argc!=2)return 2;LexicalStore lex(argv[1]);int count=0,failed=0;auto test=[&](const char* n,std::function<void()> f){++count;try{f();std::cout<<"PASS "<<n<<"\n";}catch(...){++failed;std::cout<<"FAIL "<<n<<"\n";}};
test("EnglishPrimaryNotPinyin",[]{Engine e;InputSession s(e);s.setContext("这个 ");type(s,"design");check(s.candidates()==std::vector<std::string>{"design"});s.press(Key::Space);check(s.takeCommit()=="design");});
test("AmbiguousEnglishContext",[]{Engine e;InputSession s(e);s.setContext("I think ","ZH");type(s,"he");check(s.candidates().front()=="he");});
test("ChineseContextBeatsEnglishPrior",[]{Engine e;InputSession s(e);s.setContext("这个方案","EN");type(s,"he");check(s.candidates().front()=="和");});
test("CorrectionFeedsPrimaryThenShadow",[&]{Engine e;InputSession s(e);s.setLexical(&lex);s.setContext("这个方案");type(s,"youhha");check(e.queried=="youhua"&&s.candidates().front()=="优化"&&s.shadows().front().text=="optimize");s.press(Key::Enter);check(s.takeCommit()=="youhha");});
test("UnknownLatinRetained",[]{Engine e;InputSession s(e);s.setContext("I think ");type(s,"quux");check(s.candidates()==std::vector<std::string>{"quux"});});
test("UnknownLatinNeverSentToPinyin",[]{Engine e;InputSession s(e);s.setContext("这个");type(s,"computer");check(s.candidates()==std::vector<std::string>{"computer"}&&e.queried!="computer");});
test("SecureNoContext",[]{Engine e;InputSession s(e);s.setPrivacy(PrivacyMode::Secure);s.setContext("I think 方案");type(s,"design");check(s.context().segments.empty()&&s.candidates().empty());});
std::cout<<count<<" tests, "<<failed<<" failures\n";return failed?1:0;}
