#include "RimeAdapter.h"
#include <memory>
#include <mutex>
#include <filesystem>
using namespace bilingual;
namespace {
struct EmptyEngine: PrimaryEngine { std::vector<std::string> query(const std::string&) override { return {}; } };
struct Handle {
 std::unique_ptr<PrimaryEngine> engine;
 std::unique_ptr<InputSession> session;
 std::unique_ptr<LexicalStore> lexical;
 std::vector<ShadowCandidate> shadows;
 std::string shadowValue;
 std::string commit;
 bool ready = false;
};
std::mutex lifecycle;
bool active = false;
}
#define BI extern "C" __declspec(dllexport)
BI void* bi_create(const char* shared, const char* user) noexcept {
 try {
  std::lock_guard lock(lifecycle);
  if(active) return nullptr;
  auto h=std::make_unique<Handle>();
  try { h->engine=std::make_unique<RimeAdapter>(shared?shared:"", user?user:""); h->ready=true; }
  catch(...) { h->engine=std::make_unique<EmptyEngine>(); }
  h->session=std::make_unique<InputSession>(*h->engine);
  try {
   auto path=std::filesystem::u8path(shared?shared:"").parent_path()/"lexical"/"own-v0.0.2.tsv";
   auto utf8=path.u8string();
   h->lexical=std::make_unique<LexicalStore>(std::string(utf8.begin(),utf8.end()));
   h->session->setLexical(h->lexical.get());
  }catch(...){} // A lexical failure must never prevent Primary input.
  active=true; return h.release();
 } catch(...) { return nullptr; }
}
BI void bi_destroy(void* ptr) noexcept { try { std::lock_guard lock(lifecycle); if(ptr) { delete static_cast<Handle*>(ptr); active=false; } } catch(...) {} }
BI int bi_ready(void* ptr) noexcept { return ptr && static_cast<Handle*>(ptr)->ready ? 1:0; }
BI int bi_type(void* ptr, int ch) noexcept { try { if(!ptr) return 0; static_cast<Handle*>(ptr)->session->type(static_cast<char>(ch)); return 1; } catch(...) { return 0; } }
BI int bi_key(void* ptr, int key, int shift) noexcept {
 try { if(!ptr || key<0 || key>7) return 0; return static_cast<Handle*>(ptr)->session->press(static_cast<Key>(key),shift!=0)?1:0; } catch(...) { return 0; }
}
BI int bi_select(void* ptr, int index) noexcept { try { return ptr && static_cast<Handle*>(ptr)->session->select(index)?1:0; } catch(...) { return 0; } }
BI int bi_select_shadow(void* ptr,int index) noexcept {try{return ptr&&static_cast<Handle*>(ptr)->session->selectShadow(index)?1:0;}catch(...){return 0;}}
BI int bi_shadow_count(void* ptr) noexcept {try{if(!ptr)return 0;auto h=static_cast<Handle*>(ptr);h->shadows=h->session->shadows();return static_cast<int>(h->shadows.size());}catch(...){return 0;}}
BI const char* bi_shadow_field(void* ptr,int index,int field) noexcept {
 try{if(!ptr)return "";auto h=static_cast<Handle*>(ptr);if(index<0||index>=static_cast<int>(h->shadows.size()))return "";
 const auto& s=h->shadows[index];return field==0?s.text.c_str():field==1?s.language.c_str():field==2?s.sourceId.c_str():s.entryId.c_str();}catch(...){return "";}
}
BI void bi_highlight(void* ptr, int index) noexcept { try { if(ptr) static_cast<Handle*>(ptr)->session->highlight(index); } catch(...) {} }
BI const char* bi_raw(void* ptr) noexcept { return ptr?static_cast<Handle*>(ptr)->session->raw().c_str():""; }
BI int bi_count(void* ptr) noexcept { return ptr?static_cast<int>(static_cast<Handle*>(ptr)->session->candidates().size()):0; }
BI int bi_highlighted(void* ptr) noexcept { return ptr?static_cast<Handle*>(ptr)->session->highlighted():0; }
BI const char* bi_candidate(void* ptr, int index) noexcept {
 if(!ptr) return ""; const auto& c=static_cast<Handle*>(ptr)->session->candidates();
 return index>=0 && index<static_cast<int>(c.size())?c[index].c_str():"";
}
BI const char* bi_take_commit(void* ptr) noexcept {
 try { if(!ptr) return ""; auto h=static_cast<Handle*>(ptr); h->commit=h->session->takeCommit(); return h->commit.c_str(); } catch(...) { return ""; }
}
