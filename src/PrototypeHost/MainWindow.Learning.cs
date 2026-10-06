using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
namespace BilingualInput;
public sealed partial class MainWindow {
 private bool secureSimulation;
 private static string LearningKey(string lemma)=>"own-v004:"+lemma.ToLowerInvariant();
 private void SetPrivacy(LearningPrivacy mode){
  expressionGateway.Invalidate();naturalGateway.Invalidate();expressionContextKey="";aiSentenceShadow="";naturalExpression="";
  privacy.Set(mode);encounters.Reset();session?.Privacy(mode);CloseDetails();LibraryCard.Visibility=Visibility.Collapsed;
  if(mode==LearningPrivacy.Secure){expressionShadow="";ShadowRow.Children.Clear();}
  UpdateContext();Refresh();
 }
 private void LibraryClick(object sender,RoutedEventArgs e){
  CloseDetails();SettingsCard.Visibility=Visibility.Collapsed;
  if(LibraryCard.Visibility==Visibility.Visible){LibraryCard.Visibility=Visibility.Collapsed;Editor.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);return;}
  RenderLibraryBody();LibraryCard.Visibility=Visibility.Visible;
 }
 private void RenderLibraryBody(){
  LibraryBody.Children.Clear();
  if(!privacy.LearningAllowed){LibraryBody.Children.Add(Paragraph(T("隐私模式下不读取收藏词库。","Library is unavailable in privacy mode.")));}
  else {
   var items=learning.All();
   if(items.Count==0)LibraryBody.Children.Add(Paragraph(T("还没有收藏词条。也可能是词库暂时不可用；输入仍可继续。","No saved words yet, or Library is temporarily unavailable. Input still works.")));
   foreach(var item in items){
    var label=new StackPanel{Spacing=3};
    label.Children.Add(new TextBlock{Text=$"{item.Lemma} · {item.PartOfSpeech}",FontWeight=Microsoft.UI.Text.FontWeights.SemiBold});
    label.Children.Add(Paragraph(item.Meaning));
    label.Children.Add(Paragraph((item.Ipa.Length>0?item.Ipa+" · ":"")+T("再遇见 ","Seen again ")+item.EncounterCount));
    LibraryBody.Children.Add(label);
   }
  }
 }
}
