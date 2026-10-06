using Microsoft.UI.Xaml.Controls;

namespace BilingualInput;

public sealed partial class MainWindow {
 private TextBlock? naturalOutput;
 private TextBlock? naturalNote;
 private string naturalSource="";
 private void UpdateExpressionRequest(){
  if(session==null)return;
  string source=expressionRange.Text.Trim();
  string key=string.Join("\u001f",expressionRange.Start,source,Editor.SelectionLength,session.Raw,expressionShadow,
   privacy.Ticket(),prefs.CloudAssistance,prefs.PrimaryLanguage);
  if(key==expressionContextKey)return;
  expressionContextKey=key;expressionRevision++;aiSentenceShadow="";naturalExpression="";
  naturalOutput=null;naturalNote=null;
  bool eligible=!closed&&session.Raw.Length==0&&Editor.SelectionLength==0&&expressionShadow.Length==0&&
   prefs.CloudAssistance&&expressionProvider.Available&&privacy.CloudAllowed;
  string from=ExpressionDirection.Source(source),to=ExpressionDirection.ShadowTarget(source);
  expressionGateway.Change(expressionSessionId,expressionRevision,source,from,to,eligible);
  naturalGateway.Change(expressionSessionId,expressionRevision,source,from,to,false);
  if(eligible&&ExpressionGateway.IsSentence(source))_ = ShowShadowLoading(expressionRevision);
 }
 private void OnExpressionResult(ExpressionResult result){
  DispatcherQueue.TryEnqueue(()=>{
   if(closed||result.SessionId!=expressionSessionId||result.Revision!=expressionRevision||
      !prefs.CloudAssistance||!privacy.CloudAllowed||!expressionRange.Matches(Editor.Text))return;
   if(result.Operation==ExpressionOperation.Shadow){
    if(session?.Raw.Length>0||expressionShadow.Length>0)return;
    aiSentenceShadow=result.Text;RenderShadows();
   }else if(details.DeepOpen&&naturalOutput!=null){
    naturalExpression=result.Text;
    naturalOutput.Text=string.Equals(naturalSource.Trim(),result.Text.Trim(),StringComparison.OrdinalIgnoreCase)
     ?T("当前表达已足够自然，无需另造一句。","The current expression is natural; no alternative is needed."):result.Text;
    if(naturalNote!=null)naturalNote.Text=T("按需生成的表达，仅供参考。","On-demand expression for reference.");
   }
  });
 }
 private async Task ShowShadowLoading(long revision){
  await Task.Delay(850);
  if(!closed&&revision==expressionRevision&&aiSentenceShadow.Length==0&&prefs.CloudAssistance&&privacy.CloudAllowed&&
     session?.Raw.Length==0&&expressionShadow.Length==0&&ExpressionGateway.IsSentence(expressionRange.Text))
   ShadowHint.Text=T("正在完善表达…本地输入可继续。","Refining expression… local typing remains available.");
  await Task.Delay(7900);
  if(!closed&&revision==expressionRevision&&aiSentenceShadow.Length==0&&session?.Raw.Length==0&&expressionShadow.Length==0)
   ShadowHint.Text=T("云端表达暂不可用；本地输入可继续。","Cloud expression is unavailable; local typing remains available.");
 }
 private async Task ShowNaturalLoading(long revision,TextBlock note){
  await Task.Delay(850);
  if(!closed&&revision==expressionRevision&&details.DeepOpen&&naturalExpression.Length==0&&ReferenceEquals(note,naturalNote))
   note.Text=T("正在完善表达…","Refining expression…");
  await Task.Delay(7900);
  if(!closed&&revision==expressionRevision&&details.DeepOpen&&naturalExpression.Length==0&&ReferenceEquals(note,naturalNote))
   note.Text=T("云端表达暂不可用；保留已有表达。","Cloud expression is unavailable; existing expression remains.");
 }
}
