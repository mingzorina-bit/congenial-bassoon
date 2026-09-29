using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.System;
using Windows.UI.Core;
namespace BilingualInput;
public sealed partial class MainWindow : Window {
 private NativeSession? session;
 private bool closed,initialized,showGuide;
 private readonly PreferenceStore store=new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"BilingualInput","preferences.json"));
 private UserPreferences prefs=new();
 private readonly OnboardingProgress guide=new();
 private string T(string zh,string en)=>prefs.UiLanguage=="EN"?en:zh;
 public MainWindow(){
  InitializeComponent();prefs=store.Load();showGuide=!prefs.OnboardingComplete;
  Title="Bilingual Input — v0.0.2 Shadow";
  AppWindow.Resize(new Windows.Graphics.SizeInt32(980,900));
  ThemeChoice.SelectedIndex=prefs.Theme;ApplyTheme();initialized=true;
  Editor.InputScope=new InputScope{Names={new InputScopeName(InputScopeNameValue.AlphanumericHalfWidth)}};
  Closed+=(_,_)=>{closed=true;session?.Dispose();};
  RenderGuide();
  Root.Loaded+=async(_,_)=>{
   try{var created=await Task.Run(()=>new NativeSession());if(closed){created.Dispose();return;}session=created;
    Status.Text=created.Ready?T("本地输入已就绪 · 无需网络或 API Key","Local input ready · No network or API key required"):T("候选词表不可用，仍可用 Enter 提交原文","Primary data unavailable; Enter still commits raw input");
   }catch{Status.Text=T("本地候选未启动，编辑区仍可直接输入","Local engine unavailable; editor still accepts text");}
   if(!closed){Editor.IsEnabled=true;Refresh();if(!showGuide||guide.Step==4)Editor.Focus(FocusState.Programmatic);}
  };
 }
 private void Save(){SaveStatus.Text=store.Save(prefs)?"":T("设置暂时无法保存；输入仍可使用，下次可能再次显示引导。","Preferences could not be saved. Input still works; onboarding may appear next time.");}
 private TextBlock Paragraph(string text)=>new(){Text=text,TextWrapping=TextWrapping.Wrap};
 private void RenderGuide(){
  Subtitle.Text=T("同样的输入，连接另一种表达。","The same input, another way to express it.");
  Hint.Text=T("试试 youhua、youhuafangan、xuexi。将系统输入法切到英文，在这里输入拼音。","Try youhua, youhuafangan or xuexi. Use the system keyboard in English to enter pinyin here.");
  Editor.PlaceholderText=T("在这里连续输入…","Keep typing here…");
  GuideCard.Visibility=showGuide?Visibility.Visible:Visibility.Collapsed;
  InputPanel.Visibility=!showGuide||guide.Step==4?Visibility.Visible:Visibility.Collapsed;
  ReplayButton.Visibility=showGuide?Visibility.Collapsed:Visibility.Visible;
  if(!showGuide)return;
  StepLabel.Text=$"{guide.Step+1} / 5 · v0.0.2";
  BackButton.Content=T("上一步","Back");BackButton.Visibility=guide.Step==0?Visibility.Collapsed:Visibility.Visible;
  NextButton.Content=guide.Step==4?T("开始使用","Start using"):T("继续","Continue");
  NextButton.IsEnabled=guide.Step!=4||guide.CanFinish;
  GuideBody.Children.Clear();
  switch(guide.Step){
   case 0:
    GuideTitle.Text=T("欢迎使用 Bilingual Input","Welcome to Bilingual Input");
    GuideBody.Children.Add(Paragraph(T("用你熟悉的语言，自然连接另一种表达。","Use your familiar language to connect with another expression.")));
    GuideBody.Children.Add(Paragraph(T("自然输入 · 双语表达 · 在真实使用中学习","Natural input · Bilingual expression · Learn through use")));
    GuideBody.Children.Add(Paragraph(T("本次试用：本地词语和短语 Shadow。上下文智能、发音与学习收藏将在后续版本加入。","This trial adds local word and phrase Shadows. Context intelligence, pronunciation and learning collections come later.")));break;
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
    GuideBody.Children.Add(Paragraph(T("偏好会保存。当前版本验证拼音与本地 Shadow；完整中英智能识别将在下一里程碑实现。","Your preference is saved. This version tests pinyin and local Shadows; full bilingual recognition is planned for the next milestone.")));break;
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
 private void EditorKeyDown(object sender,KeyRoutedEventArgs e){
  if(session==null)return;bool shift=Down(VirtualKey.Shift);string raw=session.Raw;
  bool caps=(Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.CapitalLock)&CoreVirtualKeyStates.Locked)!=0;
  var d=KeyboardRouter.Decide((int)e.Key,shift,caps,raw.Length>0,Down(VirtualKey.Control)||Down(VirtualKey.Menu));
  bool shadow=d.Action==InputAction.SelectShadow||(d.Action==InputAction.CoreKey&&d.Value==0&&shift);
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
 }
 private void Refresh(string raw="",bool shadow=false){
  if(session==null)return;string commit=session.TakeCommit();
  if(commit.Length>0){int start=Editor.SelectionStart;Editor.Text=Editor.Text.Remove(start,Editor.SelectionLength).Insert(start,commit);Editor.SelectionStart=start+commit.Length;Editor.SelectionLength=0;
   if(showGuide){guide.Observe(raw,commit,shadow);RenderGuide();}
  }
  Composition.Text=session.Raw.Length==0?T("准备输入","Ready to type"):session.Raw;
  CandidateRow.Children.Clear();var candidates=session.Candidates;
  for(int i=0;i<candidates.Length;i++){
   int index=i;var button=new Button{Content=$"{i+1}  {candidates[i]}",FontSize=18,IsTabStop=false};
   if(i==session.Highlighted)button.Style=(Style)Application.Current.Resources["AccentButtonStyle"];
   button.Click+=(_,_)=>{string before=session.Raw;session.Select(index);Refresh(before);Editor.Focus(FocusState.Programmatic);};
   button.PointerEntered+=(_,_)=>{session.Highlight(index);UpdateHighlights();RenderShadows();};CandidateRow.Children.Add(button);
  }RenderShadows();
 }
 private void RenderShadows(){
  if(session==null)return;ShadowRow.Children.Clear();var items=session.Shadows;
  ShadowLanguage.Text=items.Length>0?items[0].Language:"";
  ShadowHint.Text=items.Length>0?T("Shift+Enter 使用首选表达 · Shift+数字选择","Shift+Enter uses the first expression · Shift+number selects"):session.Raw.Length>0?T("暂无本地辅助表达；Primary 与 Enter 原文仍可用。","No local Shadow available; Primary and raw Enter still work."):"";
  for(int i=0;i<items.Length;i++){
   int index=i;var item=items[i];var button=new Button{Content=item.Text,FontSize=14,IsTabStop=false};
   button.Click+=(_,_)=>{string before=session.Raw;session.SelectShadow(index);Refresh(before,true);Editor.Focus(FocusState.Programmatic);};ShadowRow.Children.Add(button);
  }
 }
 private void UpdateHighlights(){if(session==null)return;for(int i=0;i<CandidateRow.Children.Count;i++)((Button)CandidateRow.Children[i]).Style=i==session.Highlighted?(Style)Application.Current.Resources["AccentButtonStyle"]:null;}
 private void ApplyTheme(){Root.RequestedTheme=prefs.Theme switch{1=>ElementTheme.Light,2=>ElementTheme.Dark,_=>ElementTheme.Default};}
 private void ThemeChanged(object sender,SelectionChangedEventArgs e){if(!initialized)return;prefs.Theme=ThemeChoice.SelectedIndex;ApplyTheme();Save();}
}
