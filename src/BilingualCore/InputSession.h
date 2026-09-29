#pragma once
#include <string>
#include <vector>
#include "LexicalStore.h"
#include "LanguageContext.h"
namespace bilingual {
enum class PrivacyMode { Normal, Private, Secure };
struct PrivacyPolicy {
  static bool cloud(PrivacyMode m) { return m == PrivacyMode::Normal; }
  static bool learning(PrivacyMode m) { return m == PrivacyMode::Normal; }
  static bool contentLogging(PrivacyMode) { return false; }
  static bool persistentTextCache(PrivacyMode) { return false; }
};
struct PrimaryEngine {
  virtual ~PrimaryEngine() = default;
  virtual std::vector<std::string> query(const std::string& raw) = 0;
};
enum class Key { Enter, Space, Backspace, Left, Right, Up, Down, Escape };
class InputSession {
 public:
  explicit InputSession(PrimaryEngine& engine): engine_(engine) {}
  void type(char ch);
  bool press(Key key, bool shift = false);
  bool select(int index);
  void highlight(int index);
  void setPrivacy(PrivacyMode mode);
  PrivacyMode privacy() const { return mode_; }
  void setLexical(const LexicalStore* store) { lexical_ = store; }
  void setContext(const std::string& expression,const std::string& prior = "ZH");
  const LanguageContext& context() const { return context_; }
  std::vector<ShadowCandidate> shadows() const;
  bool selectShadow(int index);
  const std::string& raw() const { return raw_; }
  const std::vector<std::string>& candidates() const { return candidates_; }
  int highlighted() const { return highlighted_; }
  std::string takeCommit();
 private:
  void refresh();
  void commit(const std::string& text);
  PrimaryEngine& engine_;
  const LexicalStore* lexical_ = nullptr;
  std::string raw_, pending_;
  std::vector<std::string> candidates_;
  int highlighted_ = 0;
  PrivacyMode mode_ = PrivacyMode::Normal;
  LanguageContext context_;
  std::string surroundings_,prior_="ZH";
  bool contextEnabled_=false;
};
}
