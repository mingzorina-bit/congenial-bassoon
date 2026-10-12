#include "TsfCandidateState.h"
#include <stdexcept>
#include <iostream>
using namespace bilingual::tsf;

void check(bool value) { if(!value) throw std::runtime_error("candidate assertion"); }
int main() {
  TsfCandidateState state;
  TsfKeyResult result;
  result.handled=true;
  result.raw="beijing";
  result.candidates={"北京","背景","北境","倍镜","背静","悲景"};
  result.highlighted=4;
  state.update(result,true,true);
  check(state.visible());
  check(state.entries().size()==5);
  check(state.entries()[0]=="北京" && state.entries()[4]=="背静");
  check(state.highlighted()==4);
  result.highlighted=5;
  state.update(result,true,true);
  check(state.highlighted()==4);
  result.commit="北京";result.raw.clear();
  state.update(result,true,true);
  check(!state.visible() && state.entries().empty());
  result.commit.clear();result.raw="nihao";result.candidates={"你好"};result.highlighted=0;
  state.update(result,true,true);
  check(state.visible());
  state.update(result,false,true);
  check(!state.visible());
  state.update(result,true,false);
  check(!state.visible());
  state.update(result,true,true);
  result.cancelled=true;result.raw.clear();
  state.update(result,true,true);
  check(!state.visible());
  std::cout<<"PASS TSF candidate lifecycle\n";
}
