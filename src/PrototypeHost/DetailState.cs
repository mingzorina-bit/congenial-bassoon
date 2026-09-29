namespace BilingualInput;
// UI expansion owns no input session and cannot commit or persist user text.
internal sealed class DetailState {
 private long revision,enteredAt;
 private string pending="";
 public string Visible {get;private set;}="";
 public string Snapshot {get;private set;}="";
 public bool DeepOpen {get;private set;}
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
