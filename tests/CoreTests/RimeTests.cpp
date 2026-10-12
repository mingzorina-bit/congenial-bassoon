#include "RimeAdapter.h"
#include <algorithm>
#include <chrono>
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
  if(words.empty() || words.front()!="优化") {
    std::cerr<<"FAIL authored candidate ranking\n"; return 1;
  }
  // Imported Luna entries are stored in traditional script. The UI profile
  // expects simplified candidates without adding an unreviewed OpenCC table.
  auto imported=engine.query("beijingdiqu");
  if(std::find(imported.begin(),imported.end(),"北京地区")==imported.end()) {
    std::cerr<<"FAIL imported simplified phrase\n"; return 1;
  }
  auto started=std::chrono::steady_clock::now();
  for(const auto& pair:std::vector<std::pair<std::string,std::string>>{
      {"beijing","北京"},{"shanghai","上海"},{"diannao","电脑"},
      {"nihao","你好"},{"tianqi","天气"},{"beijingdiqu","北京地区"}}) {
    auto candidates=engine.query(pair.first);
    if(std::find(candidates.begin(),candidates.end(),pair.second)==candidates.end()) {
      std::cerr<<"FAIL common-word coverage: "<<pair.first<<"\n"; return 1;
    }
  }
  auto elapsed=std::chrono::duration_cast<std::chrono::milliseconds>(std::chrono::steady_clock::now()-started).count();
  if(elapsed>6000) { std::cerr<<"FAIL common-word latency: "<<elapsed<<"ms\n"; return 1; }
  std::cout<<"Common-word fixture: 6/6, total "<<elapsed<<"ms\n";
  for(const auto& word:engine.query("nihaozzz")) {
    if(word.find("zzz")==std::string::npos) {
      std::cerr<<"FAIL partial candidate would discard raw suffix\n"; return 1;
    }
  }
  for(auto word:{"油画","有话"})
    if(std::find(words.begin(),words.end(),word)==words.end()) { std::cerr<<"FAIL authored alternate: "<<word<<"\n"; return 1; }
  InputSession session(engine);
  auto lexicalPath=std::filesystem::u8path(argv[1]).parent_path()/"lexical"/"own-v0.0.2.tsv";
  auto lexicalUtf8=lexicalPath.u8string();
  LexicalStore lexical(std::string(lexicalUtf8.begin(),lexicalUtf8.end()));
  session.setLexical(&lexical);
  for(char c:std::string("youhua"))session.type(c);
  session.press(Key::Enter,true);
  if(session.takeCommit()!="optimize") { std::cerr<<"FAIL Shadow commit\n"; return 1; }
  for(char c:std::string("youhuafangan"))session.type(c);
  session.press(Key::Enter,true);
  if(session.takeCommit()!="optimization plan") { std::cerr<<"FAIL phrase Shadow commit\n"; return 1; }
  for(char c:std::string("youhua")) session.type(c);
  session.highlight(1);
  auto expected=session.candidates().at(1);
  session.press(Key::Space);
  if(session.takeCommit()!=expected) { std::cerr<<"FAIL highlighted candidate commit\n"; return 1; }
  for(char c:std::string("asdfg")) session.type(c);
  session.press(Key::Enter);
  if(session.takeCommit()!="asdfg") { std::cerr<<"FAIL raw Enter commit\n"; return 1; }
  session.setContext("这个方案","EN");
  for(char c:std::string("youhha"))session.type(c);
  if(session.candidates().empty()||session.candidates()[0]!="优化"||session.shadows().empty()||session.shadows()[0].text!="optimize") { std::cerr<<"FAIL corrected candidate or Shadow\n"; return 1; }
  session.press(Key::Enter);if(session.takeCommit()!="youhha") { std::cerr<<"FAIL corrected raw Enter\n"; return 1; }
  session.setContext("这个 ");for(char c:std::string("design"))session.type(c);
  session.press(Key::Space);if(session.takeCommit()!="design") { std::cerr<<"FAIL English passthrough\n"; return 1; }
  session.setContext("这个 design ");for(char c:std::string("haikeyi"))session.type(c);
  session.press(Key::Space);if(session.takeCommit()!="还可以") { std::cerr<<"FAIL mixed Chinese commit\n"; return 1; }
  session.setContext("I think this ");for(char c:std::string("fangan"))session.type(c);
  session.press(Key::Space);if(session.takeCommit()!="方案") { std::cerr<<"FAIL language-gap commit\n"; return 1; }
  auto gap=LanguageContextResolver::completeGaps("I think this 方案 is better",LanguageContextResolver::analyze("I think this 方案 is better"),lexical);
  if(gap.size()!=1||gap[0].text!="I think this approach is better") { std::cerr<<"FAIL gap completion\n"; return 1; }
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
