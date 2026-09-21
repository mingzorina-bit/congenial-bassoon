#include "RimeAdapter.h"
#include <memory>
#include <mutex>
using namespace bilingual;
namespace {
struct EmptyEngine: PrimaryEngine { std::vector<std::string> query(const std::string&) override { return {}; } };
struct Handle {
 std::unique_ptr<PrimaryEngine> engine;
 std::unique_ptr<InputSession> session;
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
