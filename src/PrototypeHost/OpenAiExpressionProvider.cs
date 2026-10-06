using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BilingualInput;

internal sealed class OpenAiExpressionProvider(HttpClient client,Func<string?> apiKey) : IExpressionProvider {
 private static readonly Uri Endpoint=new("https://api.openai.com/v1/responses");
 public bool Available=>!string.IsNullOrWhiteSpace(apiKey());
 public async Task<string?> GenerateAsync(ExpressionRequest request,CancellationToken cancellation){
  string? key=apiKey();if(string.IsNullOrWhiteSpace(key))throw new InvalidOperationException("Provider unavailable");
  string instruction=request.Operation==ExpressionOperation.Shadow
   ?"Translate this complete sentence to the target language. Return one natural sentence only. Preserve meaning; add no facts, entities, intentions or explanations."
   :"Offer one more natural way to express the source sentence in the target language. Preserve meaning; add no facts, entities, intentions or explanations. If it is already natural, return the same sentence.";
  var payload=new {
   model="gpt-5.4-mini",store=false,max_output_tokens=180,
   reasoning=new{effort="none"},
   instructions=instruction,
   input=$"Source language: {request.SourceLanguage}\nTarget language: {request.TargetLanguage}\nSource sentence: {request.Source}"
  };
  using var message=new HttpRequestMessage(HttpMethod.Post,Endpoint);
  message.Headers.Authorization=new AuthenticationHeaderValue("Bearer",key);
  message.Content=new StringContent(JsonSerializer.Serialize(payload),Encoding.UTF8,"application/json");
  using var response=await client.SendAsync(message,HttpCompletionOption.ResponseHeadersRead,cancellation);
  response.EnsureSuccessStatusCode();
  await using var stream=await response.Content.ReadAsStreamAsync(cancellation);
  using var data=await JsonDocument.ParseAsync(stream,cancellationToken:cancellation);
  if(!data.RootElement.TryGetProperty("output",out var output)||output.ValueKind!=JsonValueKind.Array)return null;
  foreach(var item in output.EnumerateArray()){
   if(!item.TryGetProperty("content",out var content)||content.ValueKind!=JsonValueKind.Array)continue;
   foreach(var part in content.EnumerateArray()){
    if(part.TryGetProperty("type",out var type)&&type.GetString()=="output_text"&&
       part.TryGetProperty("text",out var text)&&text.ValueKind==JsonValueKind.String)return text.GetString();
   }
  }
  return null;
 }
}
