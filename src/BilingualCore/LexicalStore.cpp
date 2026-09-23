#include "LexicalStore.h"
#include <filesystem>
#include <fstream>
#include <sstream>
#include <algorithm>
namespace bilingual {
LexicalStore::LexicalStore(const std::string& path) {
 std::ifstream input(std::filesystem::u8path(path));
 std::string line;
 while(std::getline(input,line)) {
  if(!line.empty()&&line.back()=='\r')line.pop_back();
  if(line.empty()||line[0]=='#')continue;
  std::istringstream row(line);std::vector<std::string> fields;std::string value;
  while(std::getline(row,value,'\t'))fields.push_back(value);
  if(fields.size()!=5||fields[0].empty()||fields[2].empty()||fields[4].empty())continue;
  // Only this audited source can enter this store. Future licensed sources need separate boundaries.
  if(fields[3]!="own-curated-v002"||(fields[1]!="EN"&&fields[1]!="ZH"))continue;
  entries_.push_back({fields[0],{fields[2],fields[1],fields[3],fields[4]}});
 }
}
std::vector<ShadowCandidate> LexicalStore::lookup(const std::string& expression) const {
 std::vector<ShadowCandidate> result;
 for(const auto& e:entries_) {
  if(e.primary!=expression)continue;
  if(std::any_of(result.begin(),result.end(),[&](const auto& v){return v.text==e.shadow.text;}))continue;
  result.push_back(e.shadow);if(result.size()==3)break;
 }
 return result;
}
}
