#include "RimeAdapter.h"
#include <filesystem>
#include <stdexcept>
namespace bilingual {
RimeAdapter::RimeAdapter(const std::string& shared, const std::string& user) {
  if (!std::filesystem::exists(std::filesystem::u8path(shared) / "bilingual.schema.yaml"))
    throw std::runtime_error("Candidate data unavailable");
  std::filesystem::create_directories(std::filesystem::u8path(user));
  api_ = rime_get_api();
  RIME_STRUCT(RimeTraits, traits);
  traits.shared_data_dir = shared.c_str();
  traits.user_data_dir = user.c_str();
  traits.distribution_name = "BilingualInput";
  traits.distribution_code_name = "bilingual";
  traits.distribution_version = "0.0.1";
  traits.app_name = "rime.bilingual";
  traits.min_log_level = 3;
  traits.log_dir = "";
  api_->setup(&traits);
  api_->initialize(&traits);
  try {
    if (api_->start_maintenance(True)) api_->join_maintenance_thread();
    session_ = api_->create_session();
    if (!session_ || !api_->select_schema(session_, "bilingual"))
      throw std::runtime_error("Candidate data unavailable");
    api_->set_option(session_, "ascii_mode", False);
    api_->set_option(session_, "incognito_mode", True);
  } catch (...) { if(session_) api_->destroy_session(session_); api_->finalize(); throw; }
}
RimeAdapter::~RimeAdapter() {
  if (session_) api_->destroy_session(session_);
  if (api_) api_->finalize();
}
std::vector<std::string> RimeAdapter::query(const std::string& raw) {
  api_->clear_composition(session_);
  for (unsigned char ch:raw) api_->process_key(session_, ch, 0);
  RIME_STRUCT(RimeContext, context);
  std::vector<std::string> result;
  if (api_->get_context(session_, &context)) {
    for (int i=0;i<context.menu.num_candidates;++i)
      result.emplace_back(context.menu.candidates[i].text);
    api_->free_context(&context);
  }
  // Never commit into the Rime user dictionary; Core owns commit intent.
  api_->clear_composition(session_);
  RIME_STRUCT(RimeCommit, unused);
  if (api_->get_commit(session_, &unused)) api_->free_commit(&unused);
  return result;
}
}
