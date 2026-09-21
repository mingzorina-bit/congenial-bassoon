using Microsoft.UI.Xaml;
namespace BilingualInput;
public partial class App : Application {
 private Window? window;
 public App() {
  UnhandledException+=(_,e)=>WriteStartupFault(e.Exception);
  InitializeComponent();
 }
 private static void WriteStartupFault(Exception error) {
  // Only structural diagnostics, never exception messages or editor text.
  try {
   var dir=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"BilingualInput");
   Directory.CreateDirectory(dir);
   File.WriteAllText(Path.Combine(dir,"startup-fault.txt"),$"{error.GetType().FullName} 0x{error.HResult:X8}\n{error.StackTrace}");
  } catch { }
 }
 protected override void OnLaunched(LaunchActivatedEventArgs args) {
  try { window = new MainWindow(); window.Activate(); }
  catch(Exception error) { WriteStartupFault(error); throw; }
 }
}
