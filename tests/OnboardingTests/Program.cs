using BilingualInput;
int failed=0,count=0;
void Check(bool value){if(!value)throw new Exception("Assertion");}
void Test(string name,Action f){count++;try{f();Console.WriteLine("PASS "+name);}catch{failed++;Console.WriteLine("FAIL "+name);}}
string dir=Path.Combine(Path.GetTempPath(),"BilingualInputTests",Guid.NewGuid().ToString());Directory.CreateDirectory(dir);
Test("FiveOrderedSteps",()=>{var p=new OnboardingProgress();Check(p.Step==0);for(int i=1;i<=4;i++){p.Next();Check(p.Step==i);}p.Next();Check(p.Step==4);p.Back();Check(p.Step==3);});
Test("CannotFakeSuccess",()=>{var p=new OnboardingProgress();p.Observe("youhua","优化",false);Check(!p.CanFinish&&!p.PrimarySuccess);for(int i=0;i<4;i++)p.Next();p.Observe("x","优化",false);p.Observe("youhua","optimize",false);Check(!p.CanFinish&&!p.PrimarySuccess);});
Test("TwoRealCommitsRequired",()=>{var p=new OnboardingProgress();for(int i=0;i<4;i++)p.Next();p.Observe("youhua","优化",false);Check(p.PrimarySuccess&&!p.CanFinish);p.Observe("youhua","optimize",true);Check(p.CanFinish);p.Restart();Check(p.Step==0&&!p.CanFinish);});
Test("PreferencesSurviveRestart",()=>{var s=new PreferenceStore(Path.Combine(dir,"settings.json"));Check(s.Save(new(){PrimaryLanguage="EN",UiLanguage="EN",Goals=["Expression"],Theme=2,OnboardingComplete=true}));var p=new PreferenceStore(Path.Combine(dir,"settings.json")).Load();Check(p.PrimaryLanguage=="EN"&&p.UiLanguage=="EN"&&p.OnboardingComplete&&p.Theme==2&&p.Goals.SequenceEqual(["Expression"]));});
Test("CorruptSettingsRecover",()=>{string f=Path.Combine(dir,"broken.json");File.WriteAllText(f,"{{");Check(!new PreferenceStore(f).Load().OnboardingComplete);});
Test("InvalidValuesAreNormalized",()=>{string f=Path.Combine(dir,"invalid.json");File.WriteAllText(f,"{\"PrimaryLanguage\":\"XX\",\"Theme\":99,\"Goals\":null}");var p=new PreferenceStore(f).Load();Check(p.PrimaryLanguage=="ZH"&&p.Theme==0&&p.Goals!=null);});
Test("UnwritableSettingsDoNotThrow",()=>{Check(!new PreferenceStore(dir).Save(new()));});
Console.WriteLine($"{count} tests, {failed} failures");return failed==0?0:1;
