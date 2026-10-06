namespace BilingualInput;

internal enum ExpressionOperation { Shadow, Natural }
internal sealed record ExpressionRequest(Guid SessionId,long Revision,long RequestId,string Source,string SourceLanguage,string TargetLanguage,ExpressionOperation Operation);
internal sealed record ExpressionResult(Guid SessionId,long Revision,long RequestId,ExpressionOperation Operation,string Text);
internal interface IExpressionProvider {
 Task<string?> GenerateAsync(ExpressionRequest request,CancellationToken cancellation);
}

internal static class ExpressionDirection {
 public static string Source(string text)=>text.Any(c=>c>='\u4e00'&&c<='\u9fff')?"ZH":"EN";
 public static string ShadowTarget(string text)=>Source(text)=="ZH"?"EN":"ZH";
 public static string NaturalTarget(string source,bool alreadyMapped)=>alreadyMapped?Source(source):"EN";
}

// Owns the request lifetime. The provider never decides whether text may leave the device.
internal sealed class ExpressionGateway : IDisposable {
 private readonly IExpressionProvider provider;
 private readonly PrivacyGate privacy;
 private readonly Action<ExpressionResult> publish;
 private readonly Action<string>? diagnostic;
 private readonly TimeSpan debounce;
 private readonly object sync=new();
 private CancellationTokenSource? pending;
 private long requestId;
 private long revision;
 private bool disposed;
 private Guid sessionId;
 public Guid SessionId {get{lock(sync)return sessionId;}}
 public ExpressionGateway(IExpressionProvider provider,PrivacyGate privacy,Action<ExpressionResult> publish,TimeSpan? debounce=null,Action<string>? diagnostic=null){
  this.provider=provider;this.privacy=privacy;this.publish=publish;this.debounce=debounce??TimeSpan.FromMilliseconds(600);this.diagnostic=diagnostic;
 }
 public static bool IsSentence(string text){
  text=text.Trim();if(text.Length<3||text.Length>500)return false;
  int han=text.Count(c=>c>='\u4e00'&&c<='\u9fff');
  int words=text.Split(' ',StringSplitOptions.RemoveEmptyEntries).Length;
  bool ended=text[^1] is '.' or '!' or '?' or '。' or '！' or '？';
  return ended?(han>=2||words>=2):(han>=10||words>=6);
 }
 public void Change(Guid sessionId,long contextRevision,string source,string sourceLanguage,string targetLanguage,bool enabled){
  Invalidate();lock(sync){this.sessionId=sessionId;revision=contextRevision;}
  if(!enabled||!privacy.CloudAllowed||!IsSentence(source))return;
  Start(source,sourceLanguage,targetLanguage,ExpressionOperation.Shadow,debounce);
 }
 public void RequestNatural(string source,string sourceLanguage,string targetLanguage,bool enabled){
  if(!enabled||!privacy.CloudAllowed||!IsSentence(source))return;
  Invalidate();Start(source,sourceLanguage,targetLanguage,ExpressionOperation.Natural,TimeSpan.Zero);
 }
 private void Start(string source,string sourceLanguage,string targetLanguage,ExpressionOperation operation,TimeSpan delay){
  var tokenSource=new CancellationTokenSource();
  ExpressionRequest request;
  lock(sync){
   if(disposed){tokenSource.Dispose();return;}
   pending=tokenSource;
   request=new ExpressionRequest(sessionId,revision,++requestId,source.Trim(),sourceLanguage,targetLanguage,operation);
  }
  long privacyRevision=privacy.Ticket();
  _=Run(request,privacyRevision,delay,tokenSource);
 }
 private async Task Run(ExpressionRequest request,long privacyRevision,TimeSpan delay,CancellationTokenSource lifetime){
  try{
   if(delay>TimeSpan.Zero)await Task.Delay(delay,lifetime.Token);
   if(!Current(request,privacyRevision,lifetime))return;
   using var timeout=CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token);
   timeout.CancelAfter(TimeSpan.FromSeconds(8));
   var text=await Task.Run(async()=>{
    if(!Current(request,privacyRevision,lifetime))return null;
    return await provider.GenerateAsync(request,timeout.Token);
   },timeout.Token);
   if(!Current(request,privacyRevision,lifetime)||timeout.IsCancellationRequested)return;
   if(string.IsNullOrWhiteSpace(text)||text.Length>500){diagnostic?.Invoke("provider_invalid_response");return;}
   publish(new ExpressionResult(request.SessionId,request.Revision,request.RequestId,request.Operation,text.Trim()));
  }catch(OperationCanceledException){if(!lifetime.IsCancellationRequested)diagnostic?.Invoke("provider_timeout");}
   catch(TimeoutException){diagnostic?.Invoke("provider_timeout");}
   catch(HttpRequestException){diagnostic?.Invoke("provider_http_failure");}
   catch(Exception){diagnostic?.Invoke("provider_unavailable");} // Never log exception messages or source text.
  finally {lock(sync){if(ReferenceEquals(pending,lifetime))pending=null;}lifetime.Dispose();}
 }
 private bool Current(ExpressionRequest request,long privacyRevision,CancellationTokenSource lifetime){
  lock(sync){
   if(disposed||lifetime.IsCancellationRequested||!ReferenceEquals(pending,lifetime)||sessionId!=request.SessionId||
      revision!=request.Revision||requestId!=request.RequestId)return false;
  }
  return privacy.AcceptCloud(privacyRevision);
 }
 public void Invalidate(){CancellationTokenSource? old;lock(sync){requestId++;old=pending;pending=null;}try{old?.Cancel();}catch(ObjectDisposedException){}}
 public void Dispose(){lock(sync){if(disposed)return;disposed=true;}Invalidate();}
}
