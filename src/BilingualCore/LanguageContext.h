#pragma once
#include <string>
#include <vector>
#include "LexicalStore.h"
namespace bilingual {
struct LanguageSegment { size_t start=0,length=0; std::string text,language; bool gap=false; };
struct LanguageContext { std::string dominant="Unknown",reason="insufficient evidence"; double confidence=0; std::vector<LanguageSegment> segments; };
class LanguageContextResolver {
 public:
 static LanguageContext analyze(const std::string& expression,const std::string& prior="ZH");
 static std::string tokenLanguage(const std::string& raw,const LanguageContext& context,const std::string& prior="ZH");
 static std::string correctedPinyin(const std::string& raw,const LanguageContext& context);
 static std::vector<ShadowCandidate> completeGaps(const std::string& expression,const LanguageContext& context,const LexicalStore& lexical);
};
}
