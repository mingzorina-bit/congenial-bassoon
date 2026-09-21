using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.System;
using Windows.UI.Core;
namespace BilingualInput;
public sealed partial class MainWindow : Window {
 private NativeSession? session;
 private bool closed;
 public MainWindow() {
  InitializeComponent();
  Title="Bilingual Input — Candidate Foundation";
  AppWindow.Resize(new Windows.Graphics.SizeInt32(920,720));
  Editor.InputScope=new InputScope { Names = { new InputScopeName(InputScopeNameValue.AlphanumericHalfWidth) } };
  Closed += (_,_)=>{closed=true;session?.Dispose();};
  Root.Loaded += async (_,_)=>{
   try {
    var created=await Task.Run(()=>new NativeSession());
    if(closed){created.Dispose();return;}
    session=created;
    Status.Text=created.Ready?"本地输入已就绪 · 不需要网络或 API Key":"候选词表暂不可用 · 仍可输入并用 Enter 提交原文";
   } catch { Status.Text="本地候选未能启动 · 仍可在编辑区直接输入"; }
   if(!closed){Editor.IsEnabled=true;Editor.Focus(FocusState.Programmatic);Refresh();}
  };
 }
 private static bool Down(VirtualKey key)=>
  (Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(key)&CoreVirtualKeyStates.Down)!=0;
 private void EditorKeyDown(object sender,KeyRoutedEventArgs e) {
  if(session==null) return;
  bool shift=Down(VirtualKey.Shift);
  int code=(int)e.Key;
  bool caps=(Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.CapitalLock)&CoreVirtualKeyStates.Locked)!=0;
  var decision=KeyboardRouter.Decide(code,shift,caps,session.Raw.Length>0,Down(VirtualKey.Control)||Down(VirtualKey.Menu));
  switch(decision.Action) {
   case InputAction.Type: session.Type((char)decision.Value);e.Handled=true;break;
   case InputAction.CoreKey: e.Handled=session.Key(decision.Value,shift);break;
   case InputAction.Select: session.Select(decision.Value);e.Handled=true;break;
   case InputAction.Reserve: e.Handled=true;break;
   case InputAction.FlushPrimary: session.Key(1);break;
   case InputAction.FlushRaw: session.Key(0);break;
  }
  Refresh();
  if(shift && e.Key==VirtualKey.Enter) Status.Text="Shadow 将在 v0.0.2 加入；当前内容已保留。";
 }
 private void Refresh() {
  if(session==null)return;
  string commit=session.TakeCommit();
  if(commit.Length>0) {
   int start=Editor.SelectionStart;
   Editor.Text=Editor.Text.Remove(start,Editor.SelectionLength).Insert(start,commit);
   Editor.SelectionStart=start+commit.Length;Editor.SelectionLength=0;
  }
  Composition.Text=session.Raw.Length==0?"准备输入":session.Raw;
  CandidateRow.Children.Clear();
  var candidates=session.Candidates;
  for(int i=0;i<candidates.Length;i++){
   int index=i;
   var button=new Button{Content=$"{i+1}  {candidates[i]}",FontSize=18,IsTabStop=false};
   if(i==session.Highlighted) button.Style=(Style)Application.Current.Resources["AccentButtonStyle"];
   button.Click+=(_,_)=>{session.Select(index);Refresh();Editor.Focus(FocusState.Programmatic);};
   button.PointerEntered+=(_,_)=>{session.Highlight(index);UpdateHighlights();};
   CandidateRow.Children.Add(button);
  }
 }
 private void UpdateHighlights(){
  if(session==null)return;
  for(int i=0;i<CandidateRow.Children.Count;i++)
   ((Button)CandidateRow.Children[i]).Style=i==session.Highlighted?(Style)Application.Current.Resources["AccentButtonStyle"]:null;
 }
 private void ThemeChanged(object sender,SelectionChangedEventArgs e) {
  if(Root!=null)Root.RequestedTheme=ThemeChoice.SelectedIndex switch{1=>ElementTheme.Light,2=>ElementTheme.Dark,_=>ElementTheme.Default};
 }
}
