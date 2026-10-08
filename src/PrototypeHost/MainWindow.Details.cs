using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Windows.System;
namespace BilingualInput;
public sealed partial class MainWindow {
 private readonly DetailState details=new();
 private readonly DetailCatalog catalog=new(Path.Combine(AppContext.BaseDirectory,"data","details","own-v004.tsv"));
 private readonly PhoneticDataProvider phonetics=new(Path.Combine(AppContext.BaseDirectory,"data","pronunciation","cmudict-us.tsv"));
 private readonly WindowsSpeechProvider speech=new();
 private Popup? peek;
 private long leaveRevision,audioRevision;
 private string DetailText()=>session?.Raw.Length>0?session.Candidates.ElementAtOrDefault(session.Highlighted)??session.Raw:expressionRange.Text?.Trim()??"";
 private string DetailMapping()=>session?.Raw.Length>0?session.Shadows.FirstOrDefault()?.Text??"":expressionShadow.Length>0?expressionShadow:aiSentenceShadow;
 private void SyncDetailContext(){
  string text=DetailText();
  string snapshot=text.Length==0?"":string.Join("\n",text,session?.Raw,Editor.SelectionStart,Editor.SelectionLength,prefs.PronunciationLocale);
  if(snapshot==details.Snapshot)return;
  details.Context(snapshot);HidePeek();DeepCard.Visibility=Visibility.Collapsed;StopAudio();
 }
 private void HidePeek(){leaveRevision++;details.Leave();if(peek!=null)peek.IsOpen=false;}
 private void CloseDetails(){details.Close();naturalGateway.Invalidate();HidePeek();DeepCard.Visibility=Visibility.Collapsed;StopAudio();}
 private void StopAudio(){audioRevision++;speech.Stop();}
 private void RootKeyDown(object sender,KeyRoutedEventArgs e){
  if(e.Key==VirtualKey.Escape&&(details.DeepOpen||peek?.IsOpen==true)){
   CloseDetails();e.Handled=true;Editor.Focus(FocusState.Programmatic);
  }
 }
 private void AttachPeek(Button target,string text,string language){
  target.PointerEntered+=async(_,_)=>{
   if(!prefs.QuickPeek||closed)return;
   HidePeek();StopAudio();long ticket=details.Enter(text,Environment.TickCount64);leaveRevision++;
   await Task.Delay(prefs.HoverDelayMs);
   if(closed||!details.TryShow(ticket,Environment.TickCount64,prefs.QuickPeek,prefs.HoverDelayMs))return;
   if(peek==null){peek=new Popup{IsLightDismissEnabled=false};Surface.Children.Add(peek);}
   var content=EntryCard(text,language);
   double width=Math.Min(360,Math.Max(120,Surface.ActualWidth-24));
   var border=new Border{Width=width,RequestedTheme=Root.RequestedTheme,Padding=new Thickness(16),CornerRadius=new CornerRadius(8),BorderThickness=new Thickness(1),
    BorderBrush=(Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"],
    Background=(Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["SolidBackgroundFillColorBaseBrush"],Child=new ScrollViewer{Content=content,MaxHeight=Math.Max(60,Math.Min(310,Surface.ActualHeight-60))}};
   border.PointerEntered+=(_,_)=>leaveRevision++;
   border.PointerExited+=(_,_)=>LeavePeekSoon();
   peek.Child=border;peek.XamlRoot=Root.XamlRoot;
   border.Measure(new Windows.Foundation.Size(width,double.PositiveInfinity));
   var position=target.TransformToVisual(Surface).TransformPoint(new Windows.Foundation.Point(0,0));
   var placement=DetailPlacement.Peek(position.X,position.Y,target.ActualHeight,width,border.DesiredSize.Height,Surface.ActualWidth,Surface.ActualHeight);
   peek.HorizontalOffset=placement.X;peek.VerticalOffset=placement.Y;
   peek.IsOpen=true;
  };
  target.PointerExited+=(_,_)=>LeavePeekSoon();
 }
 private async void LeavePeekSoon(){long ticket=++leaveRevision;await Task.Delay(160);if(!closed&&ticket==leaveRevision){HidePeek();StopAudio();}}
 private StackPanel EntryCard(string text,string language){
  var panel=new StackPanel{Spacing=8};panel.Children.Add(new TextBlock{Text=text,FontSize=19,FontWeight=Microsoft.UI.Text.FontWeights.SemiBold,TextWrapping=TextWrapping.Wrap});
  var entry=catalog.Find(text);
  if(prefs.Ipa){
   var pronunciation=language=="EN"?phonetics.Find(text,prefs.PronunciationLocale):null;
   panel.Children.Add(Paragraph(pronunciation?.Value??(language=="EN"?(prefs.PronunciationLocale=="en-GB"?T("暂无英式音标","British IPA unavailable"):T("暂无本地音标","Local IPA unavailable")):T("暂无本地音标","Local IPA unavailable"))));
   if(pronunciation!=null){var source=Paragraph("CMUdict · "+pronunciation.Locale+" · "+pronunciation.SourceId);source.FontSize=10;panel.Children.Add(source);ToolTipService.SetToolTip(source,"ARPAbet: "+pronunciation.Arpabet+" → IPA (broad phonemic conversion)");}
  }
  if(prefs.PartOfSpeech&&entry!=null)panel.Children.Add(Paragraph(entry.Pos));
  if(prefs.Meaning)panel.Children.Add(Paragraph(entry==null?T("暂无本地释义","Local meaning unavailable"):prefs.PrimaryLanguage=="EN"?entry.EnMeaning:entry.ZhMeaning));
  var actions=new StackPanel{Orientation=Orientation.Horizontal,Spacing=10};
  var feedback=Paragraph("");feedback.FontSize=12;
  if(prefs.Pronunciation){
   var speak=new Button{Content=T("🔊 播放","🔊 Play"),IsTabStop=false,AllowFocusOnInteraction=false};
   speak.Click+=async(_,_)=>{
    long ticket=++audioRevision,surfaceTicket=details.AudioTicket;string locale=DetailState.VoiceLocale(language,prefs.PronunciationLocale);
    bool IsCurrent()=>!closed&&ticket==audioRevision&&details.CanPlay(surfaceTicket);
    await speech.PlayAsync(text,locale,IsCurrent,state=>DispatcherQueue.TryEnqueue(()=>{
     if(!IsCurrent())return;
     feedback.Text=state switch{
      "MissingVoice"=>T("未安装对应声线（","Voice not installed (")+locale+T("），可在 Windows 语言设置中添加。", "). Add it in Windows language settings."),
      "Preparing"=>T("准备发音…","Preparing audio…"),
      "Playing"=>T("正在播放 · ","Playing · ")+locale,
      "Ended"=>T("播放结束 · ","Playback ended · ")+locale,
      _=>T("暂时无法播放；输入仍可继续。","Playback unavailable; you can keep typing.")};
    }));
   };actions.Children.Add(speak);
  }
  var save=new Button{Content=T("☆ 收藏","☆ Save"),IsEnabled=entry!=null&&privacy.LearningAllowed,IsTabStop=false};
  if(entry!=null){var saved=learning.Contains(LearningKey(entry.English));if(saved)save.Content=T("★ 已收藏","★ Saved");
   save.Click+=(_,_)=>{
    if(!privacy.LearningAllowed)return;
    var pronunciation=phonetics.Find(entry.English,prefs.PronunciationLocale);
    var item=new LearningItem(LearningKey(entry.English),entry.English,"EN",entry.ZhMeaning,"own-v004",pronunciation?.Value??"",pronunciation?.SourceId??"",entry.Pos,"WindowsSpeech:"+prefs.PronunciationLocale+":"+entry.English,DateTimeOffset.UtcNow,0);
    if(learning.Save(item)){if(session?.Raw.Length>0)encounters.SavedInCurrentComposition(item.Key);else encounters.Reset();save.Content=T("★ 已收藏","★ Saved");feedback.Text=T("已存入收藏词库","Saved to Library");}
    else feedback.Text=T("词库暂时不可用；输入仍可继续。","Library unavailable; typing still works.");
   };
  }
  actions.Children.Add(save);panel.Children.Add(actions);panel.Children.Add(feedback);
  return panel;
 }
 private void OpenDetails(){
  SyncDetailContext();if(DetailText().Length==0)return;HidePeek();StopAudio();details.Open();DeepBody.Children.Clear();
  string text=DetailText(),mapping=DetailMapping();
  DeepBody.Children.Add(new TextBlock{Text=T("当前表达","Current expression"),FontWeight=Microsoft.UI.Text.FontWeights.SemiBold});DeepBody.Children.Add(Paragraph(text));
  foreach(var module in DetailState.Modules(prefs)){
   DeepBody.Children.Add(new TextBlock{Text=module switch{"Sentence"=>"Natural Expression","Phrase"=>"Phrase Breakdown",_=>"Vocabulary"},FontSize=17,FontWeight=Microsoft.UI.Text.FontWeights.SemiBold});
   if(module=="Sentence"){
    naturalOutput=Paragraph(mapping.Length>0?mapping:T("正在保留当前原文；可按需完善表达。","Keeping the current expression; an optional refinement may follow."));
    DeepBody.Children.Add(naturalOutput);
    var note=Paragraph(prefs.DemoAi?T("AI 演示模式：结果来自固定 Mock，不联网。","AI demo mode: fixed Mock result, no network."):prefs.CloudAssistance&&privacy.CloudAllowed&&expressionProvider.Available?T("已有表达可继续使用。","The existing expression remains available."):T("云端辅助未启用或暂不可用；本地表达仍可使用。","Cloud assistance is off or unavailable; local expression remains usable."));note.FontSize=11;DeepBody.Children.Add(note);naturalNote=note;
    naturalSource=aiSentenceShadow.Length>0?aiSentenceShadow:expressionShadow.Length>0?expressionShadow:text;
    if(ExpressionMode.Enabled(prefs.CloudAssistance,prefs.DemoAi)&&privacy.CloudAllowed&&expressionProvider.Available&&ExpressionGateway.IsSentence(naturalSource)){
     string naturalLanguage=ExpressionDirection.Source(naturalSource);
     string naturalTarget=ExpressionDirection.NaturalTarget(naturalSource,aiSentenceShadow.Length>0||expressionShadow.Length>0);
     naturalGateway.RequestNatural(naturalSource,naturalLanguage,naturalTarget,true);
     _=ShowNaturalLoading(expressionRevision,note);
    }
   }else{
    var found=catalog.In(text+"\n"+mapping).Where(e=>e.English.Contains(' ')==(module=="Phrase")).ToArray();
    if(found.Length==0)DeepBody.Children.Add(Paragraph(T("暂无本地条目","No local entries")));
    foreach(var entry in found){
     var card=EntryCard(entry.English,"EN");
     if(module=="Phrase")card.Children.Insert(1,Paragraph(entry.Chinese));
     DeepBody.Children.Add(card);
    }
   }
  }
  if(prefs.Examples)DeepBody.Children.Add(Paragraph(T("Examples：本版未提供例句数据。","Examples: no example data in this version.")));
  DeepCard.Visibility=Visibility.Visible;
  DispatcherQueue.TryEnqueue(()=>{if(!closed&&details.DeepOpen)DeepCard.StartBringIntoView(new BringIntoViewOptions{AnimationDesired=false,VerticalAlignmentRatio=0});});
 }
 private void DetailsClick(object sender,RoutedEventArgs e){OpenDetails();Editor.Focus(FocusState.Programmatic);}
 private void CloseDetailsClick(object sender,RoutedEventArgs e){CloseDetails();Editor.Focus(FocusState.Programmatic);}
 private void SettingsClick(object sender,RoutedEventArgs e){
  CloseDetails();if(SettingsCard.Visibility==Visibility.Visible){SettingsCard.Visibility=Visibility.Collapsed;Editor.Focus(FocusState.Programmatic);return;}
  SettingsBody.Children.Clear();
  void Changed(){Save();CloseDetails();}
  void Toggle(string label,bool value,Action<bool> set){var box=new CheckBox{Content=label,IsChecked=value};box.Checked+=(_,_)=>{set(true);Changed();};box.Unchecked+=(_,_)=>{set(false);Changed();};SettingsBody.Children.Add(box);}
  Toggle(T("词汇快速预览","Hover Quick Peek"),prefs.QuickPeek,v=>prefs.QuickPeek=v);
  var delay=new Slider{Header=T("悬停延迟（毫秒）","Hover delay (ms)"),Minimum=200,Maximum=1500,StepFrequency=100,Value=prefs.HoverDelayMs,Width=280,HorizontalAlignment=HorizontalAlignment.Left};
  delay.ValueChanged+=(_,_)=>{prefs.HoverDelayMs=(int)delay.Value;Changed();};SettingsBody.Children.Add(delay);
  var locale=new ComboBox{Header=T("英语发音偏好（音标与声线）","English pronunciation (IPA and voice)"),Width=300};locale.Items.Add("American · 美式");locale.Items.Add("British · 英式");locale.SelectedIndex=prefs.PronunciationLocale=="en-GB"?1:0;
  locale.SelectionChanged+=(_,_)=>{prefs.PronunciationLocale=locale.SelectedIndex==1?"en-GB":"en-US";Changed();};SettingsBody.Children.Add(locale);
  SettingsBody.Children.Add(Paragraph(T("英式音标尚未收录；未安装对应声线时会提示，不会切成其他口音。","British IPA is not included yet. Missing voices are reported without switching accents.")));
  var installed=WindowsSpeechProvider.InstalledLocales();SettingsBody.Children.Add(Paragraph(T("可用系统声线：","Installed voice locales: ")+(installed.Length>0?string.Join(", ",installed):T("未检测到","none detected"))));
  Toggle("Natural Expression",prefs.NaturalExpression,v=>prefs.NaturalExpression=v);Toggle("Phrase Breakdown",prefs.PhraseBreakdown,v=>prefs.PhraseBreakdown=v);Toggle("Vocabulary",prefs.Vocabulary,v=>prefs.Vocabulary=v);Toggle("Examples",prefs.Examples,v=>prefs.Examples=v);
  Toggle(T("AI 演示模式（Mock，不联网、不收费）","AI demo mode (Mock, offline, no charge)"),prefs.DemoAi,v=>{prefs.DemoAi=v;expressionGateway.Invalidate();naturalGateway.Invalidate();expressionContextKey="";UpdateContext();});
  Toggle(T("云端辅助（仅完整句子，默认关闭）","Cloud assistance (complete sentences only; off by default)"),prefs.CloudAssistance,v=>{prefs.CloudAssistance=v;expressionGateway.Invalidate();naturalGateway.Invalidate();expressionContextKey="";UpdateContext();});
  SettingsBody.Children.Add(Paragraph(prefs.DemoAi?T("AI 演示模式已开启：使用有限固定结果，不发送句子、不产生 API 费用。","AI demo mode is on: limited fixed results, no sentence transmission or API charge."):expressionProvider.Available?T("本机已检测到 API Key。启用后当前句子会发送至 OpenAI API。","API key detected. When enabled, the current sentence is sent to OpenAI API."):T("尚未检测到 OPENAI_API_KEY；本地输入可继续。请填写项目根目录 .env 或设置本机环境变量，然后重启程序。","OPENAI_API_KEY was not found. Local input remains available; fill in the project-root .env or set the local environment variable, then restart.")));
  Toggle("IPA",prefs.Ipa,v=>prefs.Ipa=v);Toggle(T("发音","Pronunciation"),prefs.Pronunciation,v=>prefs.Pronunciation=v);Toggle(T("释义","Meaning"),prefs.Meaning,v=>prefs.Meaning=v);Toggle(T("词性","Part of speech"),prefs.PartOfSpeech,v=>prefs.PartOfSpeech=v);
  Toggle(T("Private Mode：不记录学习行为","Private Mode: no learning records"),prefs.PrivateMode,v=>{prefs.PrivateMode=v;SetPrivacy(PrivacyGate.Effective(v,secureSimulation));});
  var secure=new CheckBox{Content=T("模拟安全输入区（本次窗口）","Simulate secure field (this window)"),IsChecked=secureSimulation};
  secure.Checked+=(_,_)=>{secureSimulation=true;SetPrivacy(PrivacyGate.Effective(prefs.PrivateMode,secureSimulation));};
  secure.Unchecked+=(_,_)=>{secureSimulation=false;SetPrivacy(PrivacyGate.Effective(prefs.PrivateMode,secureSimulation));};SettingsBody.Children.Add(secure);
  SettingsCard.Visibility=Visibility.Visible;
 }
}
