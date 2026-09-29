#include "LanguageContext.h"
namespace bilingual {
LanguageContext LanguageContextResolver::analyze(const std::string&,const std::string&){return {};}
std::string LanguageContextResolver::tokenLanguage(const std::string&,const LanguageContext&,const std::string&){return "Unknown";}
std::string LanguageContextResolver::correctedPinyin(const std::string& raw,const LanguageContext&){return raw;}
std::vector<ShadowCandidate> LanguageContextResolver::completeGaps(const std::string&,const LanguageContext&,const LexicalStore&){return {};}
}
