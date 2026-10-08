namespace BilingualInput;

internal enum ProjectEnvironmentStatus {
 AlreadyConfigured,
 Loaded,
 FileMissing,
 KeyMissing,
 ReadError
}

internal static class ProjectEnvironment {
 private const string ApiKeyName="OPENAI_API_KEY";

 public static ProjectEnvironmentStatus Load() {
  if(!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(ApiKeyName)))return ProjectEnvironmentStatus.AlreadyConfigured;
  string? root=FindProjectRoot(Environment.CurrentDirectory)??FindProjectRoot(AppContext.BaseDirectory);
  return root is null?ProjectEnvironmentStatus.FileMissing:
   LoadFile(Path.Combine(root,".env"),Environment.GetEnvironmentVariable,Environment.SetEnvironmentVariable);
 }

 internal static ProjectEnvironmentStatus LoadFile(string path,Func<string,string?> getValue,Action<string,string?> setValue) {
  if(!string.IsNullOrWhiteSpace(getValue(ApiKeyName)))return ProjectEnvironmentStatus.AlreadyConfigured;
  if(!File.Exists(path))return ProjectEnvironmentStatus.FileMissing;
  try {
   foreach(string rawLine in File.ReadLines(path)) {
    string line=rawLine.Trim();
    if(line.Length==0||line.StartsWith('#'))continue;
    if(line.StartsWith("export ",StringComparison.OrdinalIgnoreCase))line=line[7..].TrimStart();
    int separator=line.IndexOf('=');
    if(separator<0||!line[..separator].Trim().Equals(ApiKeyName,StringComparison.OrdinalIgnoreCase))continue;
    string value=line[(separator+1)..].Trim();
    if(value.Length>=2&&((value[0]=='"'&&value[^1]=='"')||(value[0]=='\''&&value[^1]=='\'')))value=value[1..^1];
    if(string.IsNullOrWhiteSpace(value))return ProjectEnvironmentStatus.KeyMissing;
    setValue(ApiKeyName,value);
    return ProjectEnvironmentStatus.Loaded;
   }
   return ProjectEnvironmentStatus.KeyMissing;
  } catch(IOException) { return ProjectEnvironmentStatus.ReadError; }
  catch(UnauthorizedAccessException) { return ProjectEnvironmentStatus.ReadError; }
 }

 private static string? FindProjectRoot(string start) {
  var directory=new DirectoryInfo(start);
  while(directory is not null) {
   if(File.Exists(Path.Combine(directory.FullName,"global.json"))&&File.Exists(Path.Combine(directory.FullName,"CMakeLists.txt")))return directory.FullName;
   directory=directory.Parent;
  }
  return null;
 }
}
