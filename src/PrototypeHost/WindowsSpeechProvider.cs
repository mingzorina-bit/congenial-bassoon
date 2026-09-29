using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media.SpeechSynthesis;
namespace BilingualInput;
internal sealed class WindowsSpeechProvider : IDisposable {
 private MediaPlayer? player;
 private MediaSource? source;
 private SpeechSynthesisStream? stream;
 private long revision;
 private bool disposed;
 public static string[] InstalledLocales(){try{return SpeechSynthesizer.AllVoices.Select(v=>v.Language).Distinct().ToArray();}catch{return [];}}
 public void Stop(){
  revision++;
  try{player?.Pause();if(player!=null)player.Source=null;}catch{}
  player?.Dispose();player=null;source?.Dispose();source=null;stream?.Dispose();stream=null;
 }
 // Only the explicit speaker-button handler calls this. Streams never touch disk.
 public async Task PlayAsync(string text,string locale,Action<string> report){
  if(disposed||string.IsNullOrWhiteSpace(text))return;
  Stop();long ticket=revision;
  try{
   var voices=SpeechSynthesizer.AllVoices;
   int index=DetailState.ChooseVoice(voices.Select(v=>v.Language).ToArray(),locale);
   if(index<0){report("MissingVoice");return;}
   report("Preparing");
   using var synth=new SpeechSynthesizer{Voice=voices[index]};
   var generated=await synth.SynthesizeTextToStreamAsync(text);
   if(disposed||ticket!=revision){generated.Dispose();return;}
   stream=generated;source=MediaSource.CreateFromStream(stream,stream.ContentType);
   player=new MediaPlayer{AutoPlay=false,Source=source};
   player.MediaEnded+=(_,_)=>{if(!disposed&&ticket==revision)report("Ended");};
   player.MediaFailed+=(_,_)=>{if(!disposed&&ticket==revision)report("Failed");};
   player.Play();report("Playing");
  }catch{if(!disposed&&ticket==revision){Stop();report("Failed");}}
 }
 public void Dispose(){disposed=true;Stop();}
}
