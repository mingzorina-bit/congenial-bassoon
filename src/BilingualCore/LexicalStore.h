#pragma once
#include <string>
#include <vector>
namespace bilingual {
struct ShadowCandidate { std::string text, language, sourceId, entryId; };
class LexicalStore {
 public:
  explicit LexicalStore(const std::string& path);
  std::vector<ShadowCandidate> lookup(const std::string& expression) const;
 private:
  struct Entry { std::string primary; ShadowCandidate shadow; };
  std::vector<Entry> entries_;
};
}
