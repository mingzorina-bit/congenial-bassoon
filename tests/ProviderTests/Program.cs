using BilingualInput;

int count=0,fail=0;
async Task Check(string name,Func<Task<bool>> test){count++;try{if(await test())Console.WriteLine("PASS "+name);else{fail++;Console.WriteLine("FAIL "+name);}}catch(Exception e){fail++;Console.WriteLine("FAIL "+name+": "+e.GetType().Name+" "+e.Message);}}

await Check("LateRevisionCannotOverwrite",async()=>{
 var provider=new PendingProvider();var gate=new PrivacyGate();var output=new List<ExpressionResult>();
 using var gateway=new ExpressionGateway(provider,gate,r=>output.Add(r),TimeSpan.Zero);
 gateway.Change(Guid.NewGuid(),10,"我觉得这个方案还可以继续优化。","ZH","EN",true);
 await provider.WaitFor(1);
 gateway.Change(gateway.SessionId,11,"我觉得这个方案可以改进。","ZH","EN",true);
 await provider.WaitFor(2);
 provider.Complete(1,"I think this approach could be improved.");provider.Complete(0,"STALE");
 await Task.Delay(30);return output.Count==1&&output[0].Text.Contains("improved");
});
await Check("AnotherSessionCannotOverwrite",async()=>{
 var provider=new PendingProvider();var output=new List<ExpressionResult>();using var gateway=new ExpressionGateway(provider,new PrivacyGate(),r=>output.Add(r),TimeSpan.Zero);
 gateway.Change(Guid.NewGuid(),10,"这是一个完整的测试句子。","ZH","EN",true);await provider.WaitFor(1);
 gateway.Change(Guid.NewGuid(),1,"这是第二个完整的句子。","ZH","EN",true);await provider.WaitFor(2);
 provider.Complete(0,"STALE");provider.Complete(1,"This is the second complete sentence.");await Task.Delay(30);
 return output.Count==1&&output[0].Text.StartsWith("This is the second");
});
await Check("ClosingNaturalDetailsKeepsPendingShadow",async()=>{
 var shadowProvider=new PendingProvider();var naturalProvider=new PendingProvider();var received=new List<ExpressionResult>();var privacy=new PrivacyGate();var id=Guid.NewGuid();
 using var shadow=new ExpressionGateway(shadowProvider,privacy,r=>received.Add(r),TimeSpan.Zero);
 using var natural=new ExpressionGateway(naturalProvider,privacy,r=>received.Add(r),TimeSpan.Zero);
 shadow.Change(id,1,"我觉得这个方案还可以继续优化","ZH","EN",true);
 natural.Change(id,1,"我觉得这个方案还可以继续优化","ZH","EN",false);
 await shadowProvider.WaitFor(1);
 natural.RequestNatural("I think this approach works.","EN","EN",true);await naturalProvider.WaitFor(1);
 natural.Invalidate();shadowProvider.Complete(0,"I think this approach could be improved.");naturalProvider.Complete(0,"STALE");
 await Task.Delay(30);return received.Count==1&&received[0].Operation==ExpressionOperation.Shadow;
});
await Check("CommitCancelsPending",async()=>{
 var provider=new PendingProvider();var output=new List<ExpressionResult>();using var gateway=new ExpressionGateway(provider,new PrivacyGate(),r=>output.Add(r),TimeSpan.Zero);
 gateway.Change(Guid.NewGuid(),2,"这是一个完整的测试句子。","ZH","EN",true);await provider.WaitFor(1);
 gateway.Invalidate();provider.Complete(0,"STALE");await Task.Delay(20);return output.Count==0;
});
await Check("PrivacySwitchBlocksBothDirections",async()=>{
 var provider=new PendingProvider();var gate=new PrivacyGate();var output=new List<ExpressionResult>();using var gateway=new ExpressionGateway(provider,gate,r=>output.Add(r),TimeSpan.Zero);
 gateway.Change(Guid.NewGuid(),2,"这是一个完整的测试句子。","ZH","EN",true);await provider.WaitFor(1);
 gate.Set(LearningPrivacy.Secure);provider.Complete(0,"STALE");await Task.Delay(20);
 gateway.Change(Guid.NewGuid(),3,"这是另一个完整的测试句子。","ZH","EN",true);await Task.Delay(20);
 return output.Count==0&&provider.Count==1;
});
await Check("QueuedCallbackRejectedAfterPrivacyFlipOrDetailClose",async()=>{
 var provider=new PendingProvider();var privacy=new PrivacyGate();ExpressionResult? result=null;
 using var gateway=new ExpressionGateway(provider,privacy,r=>result=r,TimeSpan.Zero);
 gateway.Change(Guid.NewGuid(),1,"这是一个完整的测试句子。","ZH","EN",true);await provider.WaitFor(1);
 provider.Complete(0,"This is a complete test sentence.");
 for(int i=0;i<50&&result==null;i++)await Task.Delay(5);
 if(result==null||!gateway.AcceptResult(result))return false;
 privacy.Set(LearningPrivacy.Private);privacy.Set(LearningPrivacy.Normal);
 bool rejectedPrivacy=!gateway.AcceptResult(result);
 gateway.Invalidate();return rejectedPrivacy&&!gateway.AcceptResult(result);
});
await Check("WordAndDisabledNeverCallProvider",async()=>{
 var provider=new PendingProvider();using var gateway=new ExpressionGateway(provider,new PrivacyGate(),_=>{},TimeSpan.Zero);
 gateway.Change(Guid.NewGuid(),1,"优化","ZH","EN",true);gateway.Change(Guid.NewGuid(),2,"这是一个完整的测试句子。","ZH","EN",false);
 await Task.Delay(20);return provider.Count==0;
});
await Check("ShortSentenceEligibleAndOversizedContextBlocked",async()=>{
 await Task.CompletedTask;
 return ExpressionGateway.IsSentence("I agree.")&&ExpressionGateway.IsSentence("你好。")&&
  !ExpressionGateway.IsSentence(new string('测',501)+"。")&&!ExpressionGateway.IsSentence("优化");
});
await Check("CommittedSentenceWithoutPunctuationEligible",async()=>{
 await Task.CompletedTask;
 return ExpressionGateway.IsSentence("我觉得这个方案还可以继续优化")&&
  !ExpressionGateway.IsSentence("我觉得这个")&&!ExpressionGateway.IsSentence("I think");
});
await Check("EnglishSentenceAndChineseMappingKeepCorrectLanguages",async()=>{
 await Task.CompletedTask;
 return ExpressionDirection.Source("I think this approach works.")=="EN"&&
  ExpressionDirection.ShadowTarget("I think this approach works.")=="ZH"&&
  ExpressionDirection.NaturalTarget("我觉得这个方法可行。",true)=="ZH"&&
  ExpressionDirection.NaturalTarget("我觉得这个方法可行。",false)=="EN";
});
await Check("FailuresDoNotPublish",async()=>{
 var p=new FailingProvider();var output=new List<ExpressionResult>();using var g=new ExpressionGateway(p,new PrivacyGate(),r=>output.Add(r),TimeSpan.Zero);
 foreach(var kind in new[]{"timeout","http","invalid","unavailable"}){p.Kind=kind;g.Change(Guid.NewGuid(),1,"这是一个完整的测试句子。","ZH","EN",true);await Task.Delay(20);}
 return output.Count==0&&p.Count==4;
});
await Check("DiagnosticsNeverContainInput",async()=>{
 var provider=new FailingProvider{Kind="http"};var logs=new List<string>();
 using var gateway=new ExpressionGateway(provider,new PrivacyGate(),_=>{},TimeSpan.Zero,logs.Add);
 gateway.Change(Guid.NewGuid(),1,"这是一个完整的测试句子。","ZH","EN",true);
 await Task.Delay(50);
 return logs.Count==1&&logs[0]=="provider_http_failure"&&!logs[0].Contains("测试句子");
});
await Check("ProviderSendsOnlyCurrentSentenceWithNoStorage",async()=>{
 var handler=new CaptureHandler();using var client=new HttpClient(handler);var provider=new OpenAiExpressionProvider(client,()=>"test-key");
 var text=await provider.GenerateAsync(new ExpressionRequest(Guid.NewGuid(),1,2,"我觉得这个方案还可以继续优化。","ZH","EN",ExpressionOperation.Shadow),CancellationToken.None);
 using var payload=System.Text.Json.JsonDocument.Parse(handler.Body);
 return text=="I think this approach could be improved further."&&payload.RootElement.GetProperty("input").GetString()!.Contains("我觉得这个方案还可以继续优化。")&&
  !payload.RootElement.GetProperty("store").GetBoolean()&&!handler.Body.Contains("previous chat")&&handler.Auth=="Bearer test-key"&&handler.Path=="/v1/responses";
});
await Check("ProviderUnavailableWithoutKey",async()=>{
 var handler=new CaptureHandler();using var client=new HttpClient(handler);var provider=new OpenAiExpressionProvider(client,()=>null);
 try{await provider.GenerateAsync(new ExpressionRequest(Guid.NewGuid(),1,2,"这是一个完整的测试句子。","ZH","EN",ExpressionOperation.Shadow),CancellationToken.None);return false;}
 catch(InvalidOperationException){return handler.Calls==0;}
});
await Check("MockIsClearlyFiniteAndDeterministic",async()=>{
 var mock=new MockExpressionProvider();var request=new ExpressionRequest(Guid.NewGuid(),1,1,"我觉得这个方案还可以继续优化。","ZH","EN",ExpressionOperation.Shadow);
 return await mock.GenerateAsync(request,CancellationToken.None)=="I think this approach could be further improved."&&
  await mock.GenerateAsync(request with{Source="另一句话。"},CancellationToken.None)==null&&mock.CallCount==2;
});
await Check("ProviderDoesNotRunOnCallerThread",async()=>{
 int caller=Environment.CurrentManagedThreadId;var observed=new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
 using var gateway=new ExpressionGateway(new ThreadCaptureProvider(observed),new PrivacyGate(),_=>{},TimeSpan.Zero);
 gateway.Change(Guid.NewGuid(),1,"这是一个完整的测试句子。","ZH","EN",true);
 return await observed.Task.WaitAsync(TimeSpan.FromSeconds(2))!=caller;
});
await Check("DebounceSendsOnlyLatestCompleteSentence",async()=>{
 var provider=new MockExpressionProvider();using var gateway=new ExpressionGateway(provider,new PrivacyGate(),_=>{},TimeSpan.FromMilliseconds(35));
 var id=Guid.NewGuid();gateway.Change(id,1,"我觉得这个方案还可以继续优化。","ZH","EN",true);
 gateway.Change(id,2,"我觉得这个方案还可以继续优化。","ZH","EN",true);
 await Task.Delay(100);return provider.CallCount==1;
});
Console.WriteLine($"{count} tests, {fail} failures");return fail==0?0:1;

sealed class PendingProvider:IExpressionProvider {
 private readonly List<TaskCompletionSource<string?>> pending=[];
 public int Count {get{lock(pending)return pending.Count;}}
 public Task<string?> GenerateAsync(ExpressionRequest request,CancellationToken token){var t=new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);lock(pending)pending.Add(t);return t.Task;}
 public async Task WaitFor(int n){for(int i=0;i<100&&Count<n;i++)await Task.Delay(5);if(Count<n)throw new Exception("Provider not called");}
 public void Complete(int index,string text){lock(pending)pending[index].TrySetResult(text);}
}
sealed class FailingProvider:IExpressionProvider {
 public int Count;public string Kind="";
 public Task<string?> GenerateAsync(ExpressionRequest request,CancellationToken token){Count++;return Kind switch{"timeout"=>Task.FromException<string?>(new TimeoutException()),"http"=>Task.FromException<string?>(new HttpRequestException()),"invalid"=>Task.FromResult<string?>(""),_=>Task.FromException<string?>(new InvalidOperationException())};}
}
sealed class CaptureHandler:HttpMessageHandler {
 public string Body="",Auth="",Path="";public int Calls;
 protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token){
  Calls++;Body=await request.Content!.ReadAsStringAsync(token);Auth=request.Headers.Authorization?.ToString()??"";Path=request.RequestUri?.AbsolutePath??"";
  return new HttpResponseMessage(System.Net.HttpStatusCode.OK){Content=new StringContent("{\"output\":[{\"type\":\"message\",\"content\":[{\"type\":\"output_text\",\"text\":\"I think this approach could be improved further.\"}]}]}")};
 }
}
sealed class ThreadCaptureProvider(TaskCompletionSource<int> observed):IExpressionProvider {
 public Task<string?> GenerateAsync(ExpressionRequest request,CancellationToken token){observed.TrySetResult(Environment.CurrentManagedThreadId);return Task.FromResult<string?>(null);}
}
