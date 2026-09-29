namespace BilingualInput;

internal enum LearningPrivacy { Normal, Private, Secure }
internal sealed class PrivacyGate {
 public static LearningPrivacy Effective(bool privateMode,bool secureMode)=>secureMode?LearningPrivacy.Secure:privateMode?LearningPrivacy.Private:LearningPrivacy.Normal;
 public LearningPrivacy Mode {get;private set;}
 public long Revision {get;private set;}
 public bool LearningAllowed=>Mode==LearningPrivacy.Normal;
 public bool CloudAllowed=>Mode==LearningPrivacy.Normal;
 public bool ContentLoggingAllowed=>false;
 public bool PersistentTextCacheAllowed=>false;
 public long Ticket()=>Revision;
 public bool Accept(long ticket)=>LearningAllowed&&ticket==Revision;
 public void Set(LearningPrivacy mode){if(Mode!=mode){Mode=mode;Revision++;}}
}

internal sealed record LearningItem(string Key,string Lemma,string Language,string Meaning,string MeaningSource,
 string Ipa,string IpaSource,string PartOfSpeech,string PronunciationReference,DateTimeOffset SavedAt,int EncounterCount);

// One natural reappearance per saved entry per composition, never per redraw.
internal sealed class EncounterTracker {
 private string composition="";
 private readonly HashSet<string> seen=new(StringComparer.Ordinal);
 public bool Observe(string raw,string key,bool alreadySaved){
  if(raw.Length==0){composition="";seen.Clear();return false;}
  if(composition.Length==0)composition=raw;
  if(!alreadySaved)return false;
  return seen.Add(key);
 }
 public void SavedInCurrentComposition(string key)=>seen.Add(key);
 public void Reset(){composition="";seen.Clear();}
}
