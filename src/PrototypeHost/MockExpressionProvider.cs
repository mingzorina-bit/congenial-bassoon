namespace BilingualInput;

// Deterministic test provider. Never presented as a live AI result.
internal sealed class MockExpressionProvider : IExpressionProvider {
 public int CallCount {get;private set;}
 public Task<string?> GenerateAsync(ExpressionRequest request,CancellationToken cancellation){
  cancellation.ThrowIfCancellationRequested();CallCount++;
  string source=request.Source.Trim();
  string? result=(request.Operation,source) switch{
   (ExpressionOperation.Shadow,"我觉得这个方案还可以继续优化。")=>"I think this approach could be further improved.",
   (ExpressionOperation.Shadow,"这个设计让双语输入更加自然。")=>"This design makes bilingual input feel more natural.",
   (ExpressionOperation.Shadow,"我们需要进一步优化这个功能。")=>"We need to improve this feature further.",
   (ExpressionOperation.Natural,"I think this approach could be further improved.")=>"I believe this approach could be refined further.",
   _=>null};
  return Task.FromResult(result);
 }
}
