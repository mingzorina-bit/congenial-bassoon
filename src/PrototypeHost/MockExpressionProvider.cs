namespace BilingualInput;

// Deterministic test provider. Never presented as a live AI result.
internal sealed class MockExpressionProvider : IExpressionProvider {
 public int CallCount {get;private set;}
 public Task<string?> GenerateAsync(ExpressionRequest request,CancellationToken cancellation){
  cancellation.ThrowIfCancellationRequested();CallCount++;
  string? result=request.Operation==ExpressionOperation.Shadow&&request.Source.Trim()=="我觉得这个方案还可以继续优化。"
   ?"I think this approach could be further improved.":null;
  return Task.FromResult(result);
 }
}
