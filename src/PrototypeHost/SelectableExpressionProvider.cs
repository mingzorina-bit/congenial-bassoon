namespace BilingualInput;

internal static class ExpressionMode {
 public static bool Enabled(bool cloudAssistance,bool demoAi)=>cloudAssistance||demoAi;
}

internal sealed class SelectableExpressionProvider(
 IExpressionProvider live,
 IExpressionProvider demo,
 Func<bool> demoEnabled,
 Func<bool> liveAvailable) : IExpressionProvider {
 public bool DemoEnabled=>demoEnabled();
 public bool Available=>DemoEnabled||liveAvailable();
 public Task<string?> GenerateAsync(ExpressionRequest request,CancellationToken cancellation)=>
  (DemoEnabled?demo:live).GenerateAsync(request,cancellation);
}
