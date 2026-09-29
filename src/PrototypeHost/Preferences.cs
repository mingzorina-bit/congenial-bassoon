using System.Text.Json;
namespace BilingualInput;
internal sealed class UserPreferences {
 public string PrimaryLanguage {get;set;}="ZH";
 public string InputPreference {get;set;}="Smart";
 public string UiLanguage {get;set;}="ZH";
 public string[] Goals {get;set;}=[];
 public int Theme {get;set;}
 public bool OnboardingComplete {get;set;}
 public bool QuickPeek {get;set;}=true;
 public int HoverDelayMs {get;set;}=400;
 public string PronunciationLocale {get;set;}="en-US";
 public bool NaturalExpression {get;set;}=true;
 public bool PhraseBreakdown {get;set;}=true;
 public bool Vocabulary {get;set;}=true;
 public bool Examples {get;set;}
 public bool Ipa {get;set;}=true;
 public bool Pronunciation {get;set;}=true;
 public bool Meaning {get;set;}=true;
 public bool PartOfSpeech {get;set;}=true;
}
internal sealed class PreferenceStore(string path) {
 private static UserPreferences Normalize(UserPreferences p) {
  p.PrimaryLanguage=p.PrimaryLanguage=="EN"?"EN":"ZH";
  p.UiLanguage=p.UiLanguage=="EN"?"EN":"ZH";
  p.InputPreference=p.InputPreference=="Pinyin"?"Pinyin":"Smart";
  p.Theme=p.Theme is >=0 and <=2?p.Theme:0;
  p.PronunciationLocale=p.PronunciationLocale=="en-GB"?"en-GB":"en-US";
  p.HoverDelayMs=p.HoverDelayMs is >=200 and <=1500?p.HoverDelayMs:400;
  p.Goals=(p.Goals??[]).Where(g=>g is "Expression" or "Learning" or "Input").Distinct().ToArray();
  return p;
 }
 public UserPreferences Load(){try{return Normalize(JsonSerializer.Deserialize<UserPreferences>(File.ReadAllText(path))??new());}catch{return new();}}
 public bool Save(UserPreferences value){
  try{
   Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
   File.WriteAllText(path+".tmp",JsonSerializer.Serialize(Normalize(value)));
   File.Move(path+".tmp",path,true);return true;
  }catch{return false;}
 }
}
internal sealed class OnboardingProgress {
 public int Step {get;private set;}
 public bool PrimarySuccess {get;private set;}
 public bool ShadowSuccess {get;private set;}
 public bool CanFinish=>PrimarySuccess&&ShadowSuccess;
 public void Next(){if(Step<4)Step++;}
 public void Back(){if(Step>0)Step--;}
 public void Restart(){Step=0;PrimarySuccess=false;ShadowSuccess=false;}
 public void Observe(string raw,string commit,bool shadow){
  if(Step!=4||raw!="youhua")return;
  if(!shadow&&commit=="优化")PrimarySuccess=true;
  if(shadow&&commit=="optimize")ShadowSuccess=true;
 }
}
