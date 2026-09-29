namespace BilingualInput;
internal readonly record struct ExpressionRange(int Start,int Length,string Text) {
 public static ExpressionRange At(string text,int caret,int selectionLength){
  caret=Math.Clamp(caret,0,text.Length);selectionLength=Math.Clamp(selectionLength,0,text.Length-caret);
  if(selectionLength>0)return new(caret,selectionLength,text.Substring(caret,selectionLength));
  bool End(char c)=>c is '.' or '!' or '?' or '。' or '！' or '？' or '\n' or '\r';
  int probe=caret;if(probe>0&&End(text[probe-1])&&text[probe-1]!='\n'&&text[probe-1]!='\r')probe--;
  int start=probe;while(start>0&&!End(text[start-1]))start--;
  int end=probe;while(end<text.Length&&!End(text[end]))end++;
  if(end<text.Length&&text[end]!='\n'&&text[end]!='\r')end++;
  return new(start,end-start,text.Substring(start,end-start));
 }
 public bool Matches(string text)=>Start>=0&&Length>=0&&Start<=text.Length-Length&&text.Substring(Start,Length)==Text;
}
