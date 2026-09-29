using BilingualInput;
int count=0,failed=0;
void Test(string name,Action action){count++;try{action();Console.WriteLine("PASS "+name);}catch(Exception e){failed++;Console.WriteLine("FAIL "+name+": "+e.Message);}}
void Check(bool v){if(!v)throw new Exception("assertion failed");}
Test("DelayAndNoAutoplay",()=>{var s=new DetailState();var ticket=s.Enter("refine",100);Check(!s.TryShow(ticket,499,true,400));Check(s.TryShow(ticket,500,true,400));Check(s.Visible=="refine");});
Test("LeaveCancelsDelayedPeek",()=>{var s=new DetailState();var ticket=s.Enter("refine",100);s.Leave();Check(!s.TryShow(ticket,1000,true,400));Check(s.Visible=="");});
Test("NewHoverInvalidatesPrevious",()=>{var s=new DetailState();var old=s.Enter("refine",0);var current=s.Enter("improve",1);Check(!s.TryShow(old,1000,true,400));Check(s.TryShow(current,1000,true,400));Check(s.Visible=="improve");});
Test("DisabledPeek",()=>{var s=new DetailState();var ticket=s.Enter("refine",0);Check(!s.TryShow(ticket,1000,false,400));Check(s.Visible=="");});
Test("TabEscKeepsCompositionSnapshot",()=>{var s=new DetailState();s.Context("youhua|优化");s.Open();s.Open();Check(s.DeepOpen);s.Close();s.Close();Check(!s.DeepOpen);Check(s.Snapshot=="youhua|优化");});
Test("ChangedContextClosesStaleDetails",()=>{var s=new DetailState();s.Context("youhua");s.Open();var t=s.Enter("refine",0);s.Context("youhuaa");Check(!s.DeepOpen);Check(!s.TryShow(t,1000,true,400));});
Test("SameContextKeepsExpansion",()=>{var s=new DetailState();s.Context("youhua");s.Open();s.Context("youhua");Check(s.DeepOpen);});
Test("DefaultsAndModuleOrder",()=>{var p=new UserPreferences();Check(p.QuickPeek&&p.HoverDelayMs==400&&p.PronunciationLocale=="en-US"&&!p.Examples);Check(DetailState.Modules(p).SequenceEqual(new[]{"Sentence","Phrase","Vocabulary"}));p.PhraseBreakdown=false;Check(DetailState.Modules(p).SequenceEqual(new[]{"Sentence","Vocabulary"}));});
Test("OldPreferencesGetDetailDefaults",()=>{var path=Path.GetTempFileName();try{File.WriteAllText(path,"{\"PrimaryLanguage\":\"EN\",\"OnboardingComplete\":true}");var p=new PreferenceStore(path).Load();Check(p.OnboardingComplete&&p.QuickPeek&&p.Ipa&&p.Pronunciation&&p.Meaning&&p.PartOfSpeech&&!p.Examples);}finally{File.Delete(path);}});
Test("SettingBoundsAndPersistence",()=>{var path=Path.GetTempFileName();try{var store=new PreferenceStore(path);Check(store.Save(new(){PronunciationLocale="bad",HoverDelayMs=-1,QuickPeek=false}));var p=store.Load();Check(p.PronunciationLocale=="en-US"&&p.HoverDelayMs==400&&!p.QuickPeek);Check(store.Save(new(){PronunciationLocale="en-GB",HoverDelayMs=800}));Check(store.Load().PronunciationLocale=="en-GB"&&store.Load().HoverDelayMs==800);}finally{File.Delete(path);}});
Test("ExactVoiceLocaleOnly",()=>{Check(DetailState.VoiceLocale("EN","en-GB")=="en-GB");Check(DetailState.VoiceLocale("ZH","en-US")=="zh-CN");Check(DetailState.ChooseVoice(new[]{"en-US","zh-CN"},"en-GB")==-1);Check(DetailState.ChooseVoice(new[]{"en-US","en-GB"},"en-GB")==1);});
Test("CmuConversionHasStressAndSchwa",()=>{Check(PhoneticDataProvider.ToIpa("R IH0 F AY1 N")=="/rɪˈfaɪn/");Check(PhoneticDataProvider.ToIpa("F ER1 DH ER0")=="/ˈfɝðɚ/");Check(PhoneticDataProvider.ToIpa("W ER1 K")=="/wɝk/");Check(PhoneticDataProvider.ToIpa("UNKNOWN")==null);});
Test("PronunciationProvenanceAndNoLocaleFallback",()=>{var path=Path.GetTempFileName();try{File.WriteAllText(path,"refine\tR IH0 F AY1 N\ninvalid\tNOPE\n");var p=new PhoneticDataProvider(path);var entry=p.Find("refine","en-US");Check(entry!=null&&entry.Word=="refine"&&entry.Locale=="en-US"&&entry.SourceId=="cmudict-7479086"&&entry.Notation=="IPA");Check(p.Find("refine","en-GB")==null);Check(p.Find("unknown","en-US")==null);Check(p.Find("invalid","en-US")==null);}finally{File.Delete(path);}});
Test("MissingPhoneticsDoNotBlockInput",()=>Check(new PhoneticDataProvider("nonexistent-phonetics.tsv").Find("refine","en-US")==null));
CatalogChecks.Run(Test,Check);
Test("ReplacementInvalidatesPendingAudio",()=>{var s=new DetailState();s.Context("youhua");s.Enter("refine",0);long a=s.AudioTicket;s.Enter("improve",1);Check(!s.CanPlay(a));s.Open();long b=s.AudioTicket;s.Open();Check(!s.CanPlay(b));});
Test("DelayedAudioCannotPlayAfterTab",()=>{var s=new DetailState();s.Context("youhua");s.Enter("refine",0);long ticket=s.AudioTicket;var synth=new TaskCompletionSource<bool>();var continuation=synth.Task.ContinueWith(_=>s.CanPlay(ticket));s.Open();synth.SetResult(true);Check(!continuation.GetAwaiter().GetResult());});
Test("PeekFitsBottomAndRightEdges",()=>{var p=DetailPlacement.Peek(1000,650,32,360,240,1280,720);Check(p.X+360<=1268&&p.Y+240<=650&&p.X>=12&&p.Y>=12);});
Test("PeekBelowWhenSpaceAvailable",()=>{var p=DetailPlacement.Peek(20,100,32,360,240,1280,720);Check(p.Y==132);});
Console.WriteLine($"{count} tests, {failed} failures");return failed==0?0:1;


