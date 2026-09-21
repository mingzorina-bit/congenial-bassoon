#include "RimeAdapter.h"
#include <algorithm>
#include <filesystem>
#include <fstream>
#include <iostream>
using namespace bilingual;
int main(int argc,char** argv) {
 if(argc!=3) return 2;
 try {
  std::string sentinel="privacyuniquesentinelxfqk";
  {
  RimeAdapter engine(argv[1],argv[2]);
  for (const auto& pair:std::vector<std::pair<std::string,std::string>>{
       {"youhua","优化"},{"xuexi","学习"},{"gongzuo","工作"},{"zhongguo","中国"}}) {
   auto words=engine.query(pair.first);
   if(std::find(words.begin(),words.end(),pair.second)==words.end()) {
     std::cerr<<"FAIL real candidate mapping\n"; return 1;
   }
  }
  auto words=engine.query("youhua");
  for(const auto& word:engine.query("nihaozzz")) {
    if(word.find("zzz")==std::string::npos) {
      std::cerr<<"FAIL partial candidate would discard raw suffix\n"; return 1;
    }
  }
  for(auto word:{"油画","有话"})
    if(std::find(words.begin(),words.end(),word)==words.end()) return 1;
  InputSession session(engine);
  for(char c:std::string("youhua")) session.type(c);
  session.highlight(1);
  auto expected=session.candidates().at(1);
  session.press(Key::Space);
  if(session.takeCommit()!=expected) return 1;
  for(char c:std::string("asdfg")) session.type(c);
  session.press(Key::Enter);
  if(session.takeCommit()!="asdfg") return 1;
  engine.query(sentinel);
  } // Audit after session destruction and Rime finalization flush any writes.
  for(const auto& entry:std::filesystem::recursive_directory_iterator(std::filesystem::u8path(argv[2]))) {
    if(!entry.is_regular_file()) continue;
    std::ifstream f(entry.path(),std::ios::binary);
    std::string bytes((std::istreambuf_iterator<char>(f)),{});
    if(bytes.find(sentinel)!=std::string::npos) { std::cerr<<"FAIL persistent input\n"; return 1; }
  }
  std::cout<<"PASS real librime candidates, selection, raw Enter and no persistent input\n";
  return 0;
 } catch(const std::exception&) { std::cerr<<"FAIL Rime initialization\n"; return 1; }
}
