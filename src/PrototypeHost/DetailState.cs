namespace BilingualInput;
// UI expansion owns no input session and cannot commit or persist user text.
internal sealed class DetailState {
 private long revision,enteredAt;
 private string pending="";
 public string Visible {get;private set;}="";
 public string Snapshot {get;private set;}="";
 public bool DeepOpen {get;private set;}
 public long AudioTicket=>revision;
 public bool CanPlay(long ticket)=>ticket==revision;
 public long Enter(string text,long now){Leave();pending=text;enteredAt=now;return revision;}
 public bool TryShow(long ticket,long now,bool enabled,int delay){
  if(ticket!=revision||!enabled||pending.Length==0||now-enteredAt<delay)return false;
  Visible=pending;return true;
 }
 public void Leave(){revision++;pending="";Visible="";}
 public void Context(string snapshot){if(snapshot==Snapshot)return;Snapshot=snapshot;Close();}
 public void Open(){if(Snapshot.Length>0){Leave();DeepOpen=true;}}
 public void Close(){DeepOpen=false;Leave();}
 public static string[] Modules(UserPreferences p)=>new[]{p.NaturalExpression?"Sentence":"",p.PhraseBreakdown?"Phrase":"",p.Vocabulary?"Vocabulary":""}.Where(s=>s.Length>0).ToArray();
 public static string VoiceLocale(string language,string preference)=>language=="ZH"?"zh-CN":preference=="en-GB"?"en-GB":"en-US";
 public static int ChooseVoice(string[] locales,string target)=>Array.FindIndex(locales,s=>string.Equals(s,target,StringComparison.OrdinalIgnoreCase));
}
internal readonly record struct DetailPlacement(double X,double Y){
 public static DetailPlacement Peek(double x,double y,double targetHeight,double width,double height,double viewportWidth,double viewportHeight){
  double below=y+targetHeight;
  return new(Math.Clamp(x,12,Math.Max(12,viewportWidth-width-12)),Math.Clamp(below+height<=viewportHeight-12?below:y-height,12,Math.Max(12,viewportHeight-height-12)));
 }
}
