#include "LanguageContext.h"
#include <functional>
#include <iostream>
#include <stdexcept>
using namespace bilingual;
void check(bool b){if(!b)throw std::runtime_error("assertion");}
int main(int argc,char** argv){if(argc!=2)return 2;LexicalStore lex(argv[1]);int count=0,failed=0;
auto test=[&](const char* n,std::function<void()> f){++count;try{f();std::cout<<"PASS "<<n<<"\n";}catch(...){++failed;std::cout<<"FAIL "<<n<<"\n";}};
test("ChineseMixedKeepsEnglish",[]{auto c=LanguageContextResolver::analyze("zhege design hai keyi","EN");check(c.dominant=="ZH"&&c.segments.size()==4&&c.segments[1].text=="design"&&c.segments[1].language=="EN");});
test("EnglishGapOverridesChineseProfile",[&]{std::string s="I think this 方案 is better";auto c=LanguageContextResolver::analyze(s,"ZH");check(c.dominant=="EN"&&c.confidence>0.5);auto v=LanguageContextResolver::completeGaps(s,c,lex);check(v.size()==1&&v[0].text=="I think this approach is better");});
test("VerbGapPreservesPunctuation",[&]{std::string s="We probably need to 优化 this part.";auto c=LanguageContextResolver::analyze(s);auto v=LanguageContextResolver::completeGaps(s,c,lex);check(c.dominant=="EN"&&v.size()==1&&v[0].text=="We probably need to optimize this part.");});
test("ExplicitEnglishSurvivesChinese",[]{check(LanguageContextResolver::tokenLanguage("design",LanguageContextResolver::analyze("这个"))=="EN");});
test("AmbiguityUsesContext",[]{for(auto s:{"shi","he","can","an","in","me"}){auto zh=LanguageContextResolver::analyze("这个方案");auto en=LanguageContextResolver::analyze("I think this");check(LanguageContextResolver::tokenLanguage(s,zh,"EN")=="ZH");check(LanguageContextResolver::tokenLanguage(s,en,"ZH")=="EN");auto c=LanguageContextResolver::analyze(s);check(!c.reason.empty()&&c.confidence<0.8);}});
test("TypoOnlyInChineseContext",[]{check(LanguageContextResolver::correctedPinyin("youhha",LanguageContextResolver::analyze("这个方案"))=="youhua");check(LanguageContextResolver::correctedPinyin("youhha",LanguageContextResolver::analyze("I think"))=="youhha");});
test("EmojiAndUnknownPreserved",[&]{auto c=LanguageContextResolver::analyze("😀 quux");check(c.dominant=="Unknown");check(LanguageContextResolver::completeGaps("I think 神秘词 works 😀",LanguageContextResolver::analyze("I think 神秘词 works 😀"),lex).empty());});
test("WhitespaceAndMultipleGaps",[&]{std::string s="I think  方案 needs 优化!";auto v=LanguageContextResolver::completeGaps(s,LanguageContextResolver::analyze(s),lex);check(v.size()==1&&v[0].text=="I think  approach needs optimize!");});
test("MissingLexicalKeepsExpression",[]{LexicalStore missing("absent.tsv");std::string s="I think 方案";check(LanguageContextResolver::completeGaps(s,LanguageContextResolver::analyze(s),missing).empty());});
std::cout<<count<<" tests, "<<failed<<" failures\n";return failed?1:0;}
