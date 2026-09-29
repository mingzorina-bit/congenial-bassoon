using System.Text.RegularExpressions;
namespace BilingualInput;
internal sealed record DetailEntry(string English,string Chinese,string Pos,string ZhMeaning,string EnMeaning);
internal sealed class DetailCatalog {
 private readonly List<DetailEntry> entries=[];
 public DetailCatalog(string path){try{foreach(var line in File.ReadLines(path)){if(line.StartsWith('#'))continue;var f=line.Split('\t');if(f.Length==5)entries.Add(new(f[0],f[1],f[2],f[3],f[4]));}}catch{entries.Clear();}}
 public DetailEntry? Find(string text)=>entries.FirstOrDefault(e=>string.Equals(e.English,text,StringComparison.OrdinalIgnoreCase))??entries.FirstOrDefault(e=>e.Chinese==text);
 public DetailEntry[] In(string text)=>entries.Where(e=>Regex.IsMatch(text,@"(?<![\p{L}\p{N}])"+Regex.Escape(e.English)+@"(?![\p{L}\p{N}])",RegexOptions.IgnoreCase|RegexOptions.CultureInvariant)||text.Contains(e.Chinese,StringComparison.Ordinal)).ToArray();
}
