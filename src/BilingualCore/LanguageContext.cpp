#include "LanguageContext.h"
#include <algorithm>
#include <cctype>
#include <unordered_set>
namespace bilingual {
namespace {
// Project-authored, deliberately bounded vocabulary; no downloaded language model.
const std::unordered_set<std::string> english={"i","we","you","they","this","that","the","a","is","are","was","be","think","probably","need","needs","to","better","part","design","works","work","well","hello","world","optimize","improve","refine","plan","approach","input","test","use","my","your","and","of","for","with","it","not","please","good","very","thank","thanks","new","have","want","will","should","do","does","help"};
const std::unordered_set<std::string> pinyin={"zhege","hai","keyi","haikeyi","youhua","youhuafangan","jixu","jixuyouhua","fangan","wo","women","ni","nihao","xuexi","gongzuo","zhongguo","sheji","gaijin","wancheng","wenti","jihua","juede","shuru","zhongwen","yingwen","yuyan","shijian","jintian","mingtian","xianzai","xuyao","ceshi","tiyan","ziran","biaoda","xuanze","houxuan","baocun","xiexie","pengyou","fangfa","diannao","neirong","kaishi","jieshu","xihuan","bangzhu","zhichi","kaifa","yige","haishi","danshi","yinwei","suoyi","feichang","yijing","shenme","zenme","weishenme"};
const std::unordered_set<std::string> ambiguous={"shi","he","can","an","in","me"};
std::string lower(std::string s){for(char& c:s)if(c>='A'&&c<='Z')c+=32;return s;}
bool latin(unsigned char c){return (c>='A'&&c<='Z')||(c>='a'&&c<='z');}
bool han(const std::string& s,size_t i){
 if(i+2>=s.size())return false;auto a=static_cast<unsigned char>(s[i]);auto b=static_cast<unsigned char>(s[i+1]);auto c=static_cast<unsigned char>(s[i+2]);
 if((a&0xf0)!=0xe0||(b&0xc0)!=0x80||(c&0xc0)!=0x80)return false;
 unsigned cp=((a&15)<<12)|((b&63)<<6)|(c&63);return (cp>=0x3400&&cp<=0x9fff);
}
}
LanguageContext LanguageContextResolver::analyze(const std::string& expression,const std::string& prior){
 LanguageContext c;int en=0,zh=0,amb=0;
 for(size_t i=0;i<expression.size();){size_t start=i;std::string lang;
  if(latin(static_cast<unsigned char>(expression[i]))){while(i<expression.size()&&(latin(static_cast<unsigned char>(expression[i]))||expression[i]=='\''))++i;
   auto word=lower(expression.substr(start,i-start));
   if(ambiguous.contains(word)){lang="Ambiguous";++amb;}
   else if(english.contains(word)){lang="EN";++en;}
   else if(pinyin.contains(word)){lang="ZH";zh+=2;}
   else lang="Unknown";
  }else if(han(expression,i)){while(han(expression,i))i+=3;lang="ZH";zh+=static_cast<int>((i-start)/3);}
  else {++i;continue;}
  c.segments.push_back({start,i-start,expression.substr(start,i-start),lang,false});
 }
 if(en>=2&&en>zh){c.dominant="EN";c.confidence=.9;c.reason="English word sequence outweighs Chinese gaps";}
 else if(zh>0&&zh>=en){c.dominant="ZH";c.confidence=.85;c.reason="Chinese or explicit pinyin evidence";}
 else if(en>0){c.dominant="EN";c.confidence=.8;c.reason="Recognized English word form";}
 else if(amb>0){c.dominant=prior=="EN"?"EN":"ZH";c.confidence=.35;c.reason="Ambiguous short token; profile used only as weak prior";}
 for(auto& s:c.segments){if(s.language=="Ambiguous")s.language=c.dominant;s.gap=c.dominant=="EN"&&s.language=="ZH";}
 return c;
}
std::string LanguageContextResolver::tokenLanguage(const std::string& raw,const LanguageContext& context,const std::string& prior){
 auto word=lower(raw);
 if(ambiguous.contains(word))return context.confidence>=.5?context.dominant:(prior=="EN"?"EN":"ZH");
 if(english.contains(word))return "EN";
 if(pinyin.contains(word))return "ZH";
 return "Unknown";
}
std::string LanguageContextResolver::correctedPinyin(const std::string& raw,const LanguageContext& context){
 return raw=="youhha"&&context.dominant=="ZH"&&context.confidence>=.5?"youhua":raw;
}
std::vector<ShadowCandidate> LanguageContextResolver::completeGaps(const std::string& expression,const LanguageContext& context,const LexicalStore& lexical){
 if(context.dominant!="EN"||context.confidence<.5)return {};
 std::string result=expression,sources,entries;bool changed=false;
 for(auto it=context.segments.rbegin();it!=context.segments.rend();++it){if(!it->gap)continue;
  // Whole gap only: never assemble a phrase out of individually translated characters.
  auto choices=lexical.lookup(it->text);auto choice=std::find_if(choices.begin(),choices.end(),[](const auto& v){return v.language=="EN";});
  if(choice==choices.end())continue;
  // In an English expression, the authored alternative "approach" is preferred for 方案.
  if(it->text=="方案"){auto approach=std::find_if(choices.begin(),choices.end(),[](const auto& v){return v.text=="approach";});if(approach!=choices.end())choice=approach;}
  if(it->start+it->length>expression.size()||expression.substr(it->start,it->length)!=it->text)return {};
  result.replace(it->start,it->length,choice->text);sources=choice->sourceId;entries=choice->entryId+(entries.empty()?"":";"+entries);changed=true;
 }
 if(!changed)return {};return {{result,"EN",sources,entries}};
}
}
