using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.System;
using Windows.UI.Core;
namespace BilingualInput;
public sealed partial class MainWindow : Window {
 private NativeSession? session;
 private bool closed,initialized,showGuide;
 private bool updating;
 private ExpressionRange expressionRange;
 private string expressionShadow="";
 private readonly Guid expressionSessionId=Guid.NewGuid();
 private long expressionRevision;
 private string expressionContextKey="";
 private string aiSentenceShadow="";
 private string naturalExpression="";
 private readonly HttpClient expressionHttp=new();
 private readonly ExpressionGateway expressionGateway;
 private readonly ExpressionGateway naturalGateway;
 private readonly SelectableExpressionProvider expressionProvider;
 private readonly PreferenceStore store=new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"BilingualInput","preferences.json"));
 private UserPreferences prefs=new();
 private readonly OnboardingProgress guide=new();
 private readonly PrivacyGate privacy=new();
 private readonly EncounterTracker encounters=new();
 private readonly LearningStore learning;
 private string T(string zh,string en)=>prefs.UiLanguage=="EN"?en:zh;
 public MainWindow(){
  InitializeComponent();prefs=store.Load();showGuide=!prefs.OnboardingComplete;
  var liveExpressionProvider=new OpenAiExpressionProvider(expressionHttp,()=>Environment.GetEnvironmentVariable("OPENAI_API_KEY"));
  expressionProvider=new(liveExpressionProvider,new MockExpressionProvider(),()=>prefs.DemoAi,()=>liveExpressionProvider.Available);
  expressionGateway=new(expressionProvider,privacy,OnExpressionResult,diagnostic:code=>System.Diagnostics.Debug.WriteLine(code));
  naturalGateway=new(expressionProvider,privacy,OnExpressionResult,diagnostic:code=>System.Diagnostics.Debug.WriteLine(code));
  learning=new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"BilingualInput","learning-v005.db"),privacy);
  privacy.Set(prefs.PrivateMode?LearningPrivacy.Private:LearningPrivacy.Normal);
  Title="Bilingual Input — v0.0.6 AI Expression";
  AppWindow.Resize(new Windows.Graphics.SizeInt32(980,900));
  ThemeChoice.SelectedIndex=prefs.Theme;ApplyTheme();initialized=true;
  Editor.InputScope=new InputScope{Names={new InputScopeName(InputScopeNameValue.AlphanumericHalfWidth)}};
  Editor.TextChanged+=(_,_)=>{if(!updating)UpdateContext();};
  Editor.SelectionChanged+=(_,_)=>{if(!updating)UpdateContext();};
  Surface.SizeChanged+=(_,_)=>{if(peek?.IsOpen==true){HidePeek();StopAudio();}};
  PageScroll.ViewChanged+=(_,_)=>{if(peek?.IsOpen==true){HidePeek();StopAudio();}};
  Closed+=(_,_)=>{closed=true;expressionGateway.Dispose();naturalGateway.Dispose();expressionHttp.Dispose();CloseDetails();speech.Dispose();session?.Dispose();};
  RenderGuide();
  Root.Loaded+=async(_,_)=>{
   try{var created=await Task.Run(()=>new NativeSession());if(closed){created.Dispose();return;}session=created;created.Privacy(privacy.Mode);
    Status.Text=created.Ready?T("本地输入已就绪 · 无需网络或 API Key","Local input ready · No network or API key required"):T("候选词表不可用，仍可用 Enter 提交原文","Primary data unavailable; Enter still commits raw input");
   }catch{Status.Text=T("本地候选未启动，编辑区仍可直接输入","Local engine unavailable; editor still accepts text");}
   if(!closed){Editor.IsEnabled=true;UpdateContext();Refresh();if(!showGuide||guide.Step==4)Editor.Focus(FocusState.Programmatic);}
  };
 }
 private void Save(){SaveStatus.Text=store.Save(prefs)?"":T("设置暂时无法保存；输入仍可使用，下次可能再次显示引导。","Preferences could not be saved. Input still works; onboarding may appear next time.");}
 private TextBlock Paragraph(string text)=>new(){Text=text,TextWrapping=TextWrapping.Wrap};
 private void RenderGuide(){
  Subtitle.Text=T("同样的输入，连接另一种表达。","The same input, another way to express it.");
  Hint.Text=T("试试 design、youhua 或 I think this 方案 is better。系统输入法切到英文输入拼音；可粘贴混合句体验补全。","Try design or youhua. You can paste a mixed sentence to complete its Chinese gaps. Use the system keyboard in English for pinyin.");
  Editor.PlaceholderText=T("在这里连续输入…","Keep typing here…");
  GuideCard.Visibility=showGuide?Visibility.Visible:Visibility.Collapsed;
  InputPanel.Visibility=!showGuide||guide.Step==4?Visibility.Visible:Visibility.Collapsed;
  ReplayButton.Visibility=showGuide?Visibility.Collapsed:Visibility.Visible;
  if(!showGuide)return;
  StepLabel.Text=$"{guide.Step+1} / 5 · v0.0.6";
  BackButton.Content=T("上一步","Back");BackButton.Visibility=guide.Step==0?Visibility.Collapsed:Visibility.Visible;
  NextButton.Content=guide.Step==4?T("开始使用","Start using"):T("继续","Continue");
  NextButton.IsEnabled=guide.Step!=4||guide.CanFinish;
  GuideBody.Children.Clear();
  switch(guide.Step){
   case 0:
    GuideTitle.Text=T("欢迎使用 Bilingual Input","Welcome to Bilingual Input");
    GuideBody.Children.Add(Paragraph(T("用你熟悉的语言，自然连接另一种表达。","Use your familiar language to connect with another expression.")));
    GuideBody.Children.Add(Paragraph(T("自然输入 · 双语表达 · 在真实使用中学习","Natural input · Bilingual expression · Learn through use")));
    GuideBody.Children.Add(Paragraph(T("本次试用：可在设置中开启云端辅助，为完整句子取得英文表达；本地输入始终可用。","This trial can add cloud assistance for complete sentences. Local input remains available.")));break;
   case 1:
    GuideTitle.Text=T("你更熟悉哪种语言？","Which language feels more familiar?");
    var primary=new ComboBox{Header=T("Primary Language（不代表英语水平）","Primary language (not a proficiency rating)"),Width=300};
    primary.Items.Add("中文");primary.Items.Add("English");primary.SelectedIndex=prefs.PrimaryLanguage=="EN"?1:0;
    primary.SelectionChanged+=(_,_)=>{prefs.PrimaryLanguage=primary.SelectedIndex==1?"EN":"ZH";prefs.UiLanguage=prefs.PrimaryLanguage;RenderGuide();};GuideBody.Children.Add(primary);
    var ui=new ComboBox{Header="界面语言 / UI language",Width=300};ui.Items.Add("中文");ui.Items.Add("English");ui.SelectedIndex=prefs.UiLanguage=="EN"?1:0;
    ui.SelectionChanged+=(_,_)=>{prefs.UiLanguage=ui.SelectedIndex==1?"EN":"ZH";RenderGuide();};GuideBody.Children.Add(ui);break;
   case 2:
    GuideTitle.Text=T("输入偏好","Input preference");
    var preference=new ComboBox{Width=320};preference.Items.Add(T("智能识别（推荐）","Smart recognition (recommended)"));preference.Items.Add(T("中文拼音优先","Chinese pinyin first"));preference.SelectedIndex=prefs.InputPreference=="Pinyin"?1:0;
    preference.SelectionChanged+=(_,_)=>prefs.InputPreference=preference.SelectedIndex==1?"Pinyin":"Smart";GuideBody.Children.Add(preference);
    GuideBody.Children.Add(Paragraph(T("偏好会保存。当前表达优先于语言偏好；本版使用有限本地规则，未知词保留原文。","Current expression takes priority over your saved preference. Local rules are limited; unknown words keep their original form.")));break;
   case 3:
    GuideTitle.Text=T("你希望用它做什么？","What would you like to do?");
    foreach(var item in new[]{("Expression",T("更自然地使用另一种语言表达","Express yourself naturally in another language")),("Learning",T("在输入过程中学习另一种语言","Learn another language while typing")),("Input",T("快速进行中英双语输入","Type in Chinese and English quickly"))}){
     var box=new CheckBox{Content=item.Item2,IsChecked=prefs.Goals.Contains(item.Item1)};
     box.Checked+=(_,_)=>prefs.Goals=prefs.Goals.Append(item.Item1).Distinct().ToArray();box.Unchecked+=(_,_)=>prefs.Goals=prefs.Goals.Where(g=>g!=item.Item1).ToArray();GuideBody.Children.Add(box);
    }
    GuideBody.Children.Add(Paragraph(T("可多选。目标将保存，尚未实现的能力不会提前开启。","Choose any that apply. Goals are saved; later features are not enabled yet.")));break;
   case 4:
    GuideTitle.Text=T("准备好了，试着输入一句。","Ready. Try typing.");
    GuideBody.Children.Add(Paragraph((guide.PrimarySuccess?"✓ ":"1. ")+T("输入 youhua，按 Space → 优化","Type youhua, press Space → 优化")));
    GuideBody.Children.Add(Paragraph((guide.ShadowSuccess?"✓ ":"2. ")+T("再次输入 youhua，按 Shift+Enter → optimize","Type youhua again, press Shift+Enter → optimize")));
    if(guide.CanFinish)GuideBody.Children.Add(Paragraph(T("就是这样。像平时一样输入，需要时使用另一种表达。","That's it. Type as usual; use another expression when needed.")));
    break;
  }
 }
 private void NextClick(object sender,RoutedEventArgs e){
  if(guide.Step==4){if(!guide.CanFinish)return;prefs.OnboardingComplete=true;showGuide=false;Save();}
  else guide.Next();RenderGuide();if(!showGuide||guide.Step==4)Editor.Focus(FocusState.Programmatic);
 }
 private void BackClick(object sender,RoutedEventArgs e){guide.Back();RenderGuide();}
 private void ReplayClick(object sender,RoutedEventArgs e){guide.Restart();showGuide=true;RenderGuide();}
 private static bool Down(VirtualKey k)=>(Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(k)&CoreVirtualKeyStates.Down)!=0;
 private void UpdateContext(){
  if(session==null||updating)return;
  expressionRange=ExpressionRange.At(Editor.Text,Editor.SelectionStart,Editor.SelectionLength);
  int relative=Math.Clamp(Editor.SelectionStart-expressionRange.Start,0,expressionRange.Length);
  string surroundings=expressionRange.Text;
  string current=surroundings.Insert(relative,session.Raw);
  session.Context(surroundings,current,prefs.InputPreference=="Pinyin"?"ZH":prefs.PrimaryLanguage);
  expressionShadow=session.Raw.Length==0?session.ExpressionShadow:"";
  UpdateExpressionRequest();
  ContextLabel.Text=session.ContextInfo.StartsWith("EN |")?T("当前表达：英文","Current expression: English"):session.ContextInfo.StartsWith("ZH |")?T("当前表达：中文","Current expression: Chinese"):T("当前表达：待判断，保留原文","Current expression: uncertain; original text preserved");
  RenderCandidates();
  RenderShadows();
 }
 private void CommitExpression(){
  string replacement=expressionShadow.Length>0?expressionShadow:aiSentenceShadow;
  if(session==null||session.Raw.Length!=0||replacement.Length==0||!expressionRange.Matches(Editor.Text))return;
  var current=ExpressionRange.At(Editor.Text,Editor.SelectionStart,Editor.SelectionLength);
  if(current!=expressionRange){UpdateContext();return;}
  updating=true;
  try{Editor.Select(expressionRange.Start,expressionRange.Length);Editor.SelectedText=replacement;Editor.Select(expressionRange.Start+replacement.Length,0);}
  finally{updating=false;}
  UpdateContext();Editor.Focus(FocusState.Programmatic);
 }
 private void EditorKeyDown(object sender,KeyRoutedEventArgs e){
  if(session==null)return;UpdateContext();bool shift=Down(VirtualKey.Shift);string raw=session.Raw;
  if(e.Key==VirtualKey.Tab&&!shift&&!Down(VirtualKey.Control)&&!Down(VirtualKey.Menu)&&DetailText().Length>0){OpenDetails();e.Handled=true;return;}
  if(raw.Length==0&&shift&&e.Key==VirtualKey.Enter&&(expressionShadow.Length>0||aiSentenceShadow.Length>0)){CommitExpression();e.Handled=true;return;}
  bool caps=(Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.CapitalLock)&CoreVirtualKeyStates.Locked)!=0;
  var d=KeyboardRouter.Decide((int)e.Key,shift,caps,raw.Length>0,Down(VirtualKey.Control)||Down(VirtualKey.Menu));
  bool shadow=d.Action==InputAction.SelectShadow||(d.Action==InputAction.CoreKey&&d.Value==0&&shift);
  // After a Latin Primary is committed by Space, let the editor insert its normal separator.
  bool wordSpace=d.Action==InputAction.CoreKey&&d.Value==1&&session.Candidates.Length>0&&session.Candidates[Math.Clamp(session.Highlighted,0,session.Candidates.Length-1)].All(c=>c<128);
  switch(d.Action){
   case InputAction.Type:session.Type((char)d.Value);e.Handled=true;break;
   case InputAction.CoreKey:e.Handled=session.Key(d.Value,shift);break;
   case InputAction.Select:session.Select(d.Value);e.Handled=true;break;
   case InputAction.SelectShadow:session.SelectShadow(d.Value);e.Handled=true;break;
   case InputAction.Reserve:e.Handled=true;break;
   case InputAction.FlushPrimary:session.Key(1);break;
   case InputAction.FlushRaw:session.Key(0);break;
  }
  Refresh(raw,shadow);
  if(wordSpace)e.Handled=false;
 }
 private void Refresh(string raw="",bool shadow=false){
  if(session==null)return;string commit=session.TakeCommit();
  if(commit.Length>0){int start=Editor.SelectionStart;updating=true;try{Editor.SelectedText=commit;Editor.Select(start+commit.Length,0);}finally{updating=false;}
   if(showGuide){guide.Observe(raw,commit,shadow);RenderGuide();}
  }
  Composition.Text=session.Raw.Length==0?T("准备输入","Ready to type"):session.Raw;
  UpdateContext();
 }
 private void RenderCandidates(){
  if(session==null)return;CandidateRow.Children.Clear();var candidates=session.Candidates;
  for(int i=0;i<candidates.Length;i++){
   int index=i;var button=new Button{Content=$"{i+1}  {candidates[i]}",FontSize=18,IsTabStop=false};
   if(i==session.Highlighted)button.Style=(Style)Application.Current.Resources["AccentButtonStyle"];
   var snapshot=new CandidateSnapshot(session.Raw,index,candidates[i]);
   button.Click+=(_,_)=>{if(!snapshot.Matches(session.Raw,session.Candidates)){Refresh();return;}string before=session.Raw;session.Select(index);Refresh(before);Editor.Focus(FocusState.Programmatic);};
   button.PointerEntered+=(_,_)=>{session.Highlight(index);UpdateHighlights();RenderShadows();};CandidateRow.Children.Add(button);
  }
 }
 private void RenderShadows(){
  if(session==null)return;SyncDetailContext();ShadowRow.Children.Clear();var items=session.Shadows;
  if(session.Raw.Length==0)encounters.Reset();
  else if(privacy.LearningAllowed){bool changed=false;foreach(var item in items){var key=LearningKey(item.Text);if(learning.Contains(key)&&encounters.Observe(session.Raw,key,true))changed|=learning.Encounter(key);}if(changed&&LibraryCard.Visibility==Visibility.Visible)RenderLibraryBody();}
  string wholeExpression=expressionShadow.Length>0?expressionShadow:aiSentenceShadow;
  if(session.Raw.Length==0&&wholeExpression.Length>0){
   ShadowLanguage.Text=expressionShadow.Length>0?"EN":ExpressionDirection.ShadowTarget(expressionRange.Text);
   ShadowHint.Text=expressionShadow.Length>0?T("补全当前表达中的中文缺口 · Shift+Enter 应用","Complete Chinese gaps in this expression · Shift+Enter to apply"):prefs.DemoAi?T("AI 演示结果（Mock，不联网） · Shift+Enter 应用","AI demo result (Mock, offline) · Shift+Enter to apply"):T("完整句子的云端辅助表达 · Shift+Enter 应用","Cloud-assisted sentence · Shift+Enter to apply");
   var button=new Button{Content=new TextBlock{Text=wholeExpression,TextWrapping=TextWrapping.Wrap},FontSize=14,IsTabStop=false,MaxWidth=720};
   button.Click+=(_,_)=>CommitExpression();ShadowRow.Children.Add(button);return;
  }
  ShadowLanguage.Text=items.Length>0?items[0].Language:"";
  ShadowHint.Text=items.Length>0?T("Shift+Enter 使用首选表达 · Shift+数字选择","Shift+Enter uses the first expression · Shift+number selects"):session.Raw.Length>0?T("暂无本地辅助表达；Primary 与 Enter 原文仍可用。","No local Shadow available; Primary and raw Enter still work."):"";
  for(int i=0;i<items.Length;i++){
   int index=i;var item=items[i];var button=new Button{Content=item.Text,FontSize=14,IsTabStop=false};
   var snapshot=new CandidateSnapshot(session.Raw,index,item.Text);
   button.Click+=(_,_)=>{if(!snapshot.Matches(session.Raw,session.Shadows.Select(s=>s.Text).ToArray())){Refresh();return;}string before=session.Raw;session.SelectShadow(index);Refresh(before,true);Editor.Focus(FocusState.Programmatic);};AttachPeek(button,item.Text,item.Language);ShadowRow.Children.Add(button);
  }
 }
 private void UpdateHighlights(){if(session==null)return;for(int i=0;i<CandidateRow.Children.Count;i++)((Button)CandidateRow.Children[i]).Style=i==session.Highlighted?(Style)Application.Current.Resources["AccentButtonStyle"]:null;}
 private void ApplyTheme(){Root.RequestedTheme=prefs.Theme switch{1=>ElementTheme.Light,2=>ElementTheme.Dark,_=>ElementTheme.Default};}
 private void ThemeChanged(object sender,SelectionChangedEventArgs e){if(!initialized)return;prefs.Theme=ThemeChoice.SelectedIndex;ApplyTheme();Save();}
}
