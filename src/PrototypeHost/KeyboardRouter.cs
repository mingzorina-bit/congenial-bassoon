namespace BilingualInput;
internal enum InputAction { Pass, Type, CoreKey, Select, SelectShadow, Reserve, FlushPrimary, FlushRaw }
internal readonly record struct InputDecision(InputAction Action, int Value=0);
internal static class KeyboardRouter {
 public static InputDecision Decide(int code,bool shift,bool caps,bool composing,bool shortcut) {
  if(shortcut) return new(composing?InputAction.FlushRaw:InputAction.Pass);
  if(code>=65 && code<=90) return new(InputAction.Type,(shift^caps)?code:code+32);
  if(code>=49 && code<=57 && !shift && composing) return new(InputAction.Select,code-49);
  if(shift && code>=49 && code<=53 && composing) return new(InputAction.SelectShadow,code-49);
  int key=code switch {13=>0,32=>1,8=>2,37=>3,39=>4,38=>5,40=>6,27=>7,_=>-1};
  if(key>=0)return new(InputAction.CoreKey,key);
  if(code==9 && composing)return new(InputAction.Reserve);
  if(code==222 && !shift && composing)return new(InputAction.Type,39);
  bool textKey=(code>=48 && code<=57)||(code>=96 && code<=111)||
    (code>=186 && code<=192)||(code>=219 && code<=222)||code==226;
  if(composing && textKey)return new(InputAction.FlushPrimary);
  if(composing && (code==33||code==34||code==35||code==36||code==46))return new(InputAction.FlushRaw);
  return new(InputAction.Pass);
 }
}


