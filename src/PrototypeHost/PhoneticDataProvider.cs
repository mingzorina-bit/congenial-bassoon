namespace BilingualInput;
internal sealed record PronunciationEntry(string Word,string Locale,string Notation,string Value,string SourceId,string Arpabet);
internal sealed class PhoneticDataProvider {
 private readonly Dictionary<string,string> entries=new(StringComparer.OrdinalIgnoreCase);
 public const string SourceId="cmudict-7479086";
 public PhoneticDataProvider(string path){try{foreach(var line in File.ReadLines(path)){if(line.StartsWith('#'))continue;var f=line.Split('\t');if(f.Length==2&&ToIpa(f[1])!=null)entries.TryAdd(f[0],f[1]);}}catch{entries.Clear();}}
 public PronunciationEntry? Find(string word,string locale)=>locale=="en-US"&&entries.TryGetValue(word,out var phones)?new(word,locale,"IPA",ToIpa(phones)!,SourceId,phones):null;
 private static readonly Dictionary<string,string> Phones=new(){
  ["AA"]="ɑ",["AE"]="æ",["AH"]="ʌ",["AO"]="ɔ",["AW"]="aʊ",["AY"]="aɪ",["EH"]="ɛ",["ER"]="ɝ",["EY"]="eɪ",["IH"]="ɪ",["IY"]="i",["OW"]="oʊ",["OY"]="ɔɪ",["UH"]="ʊ",["UW"]="u",
  ["B"]="b",["CH"]="tʃ",["D"]="d",["DH"]="ð",["F"]="f",["G"]="ɡ",["HH"]="h",["JH"]="dʒ",["K"]="k",["L"]="l",["M"]="m",["N"]="n",["NG"]="ŋ",["P"]="p",["R"]="r",["S"]="s",["SH"]="ʃ",["T"]="t",["TH"]="θ",["V"]="v",["W"]="w",["Y"]="j",["Z"]="z",["ZH"]="ʒ"};
 // Conservative broad phonemic rendering; stress placement uses longest common onset.
 private static readonly HashSet<string> Onsets=new(){"P R","B R","T R","D R","K R","G R","F R","TH R","SH R","P L","B L","K L","G L","F L","S L","S P","S T","S K","S M","S N","S W","T W","K W","S P R","S T R","S K R","S P L","S K W"};
 public static string? ToIpa(string arpabet){
  var tokens=arpabet.Split(' ',StringSplitOptions.RemoveEmptyEntries);if(tokens.Length==0)return null;
  var result=new string[tokens.Length];var stress=new string[tokens.Length];int previousVowel=-1;
  int vowels=tokens.Count(t=>char.IsDigit(t[^1]));
  for(int i=0;i<tokens.Length;i++){
   var t=tokens[i];bool vowel=char.IsDigit(t[^1]);string key=vowel?t[..^1]:t;
   if(!Phones.TryGetValue(key,out var value)||vowel&&t[^1] is not ('0' or '1' or '2'))return null;
   result[i]=t=="AH0"?"ə":t=="ER0"?"ɚ":value;
   if(vowel){
    if(vowels>1&&t[^1]!='0'){
     int start=i;if(i>previousVowel+1&&tokens[i-1]!="NG")start=i-1;
     for(int n=2;n<=3&&i-n>previousVowel;n++)if(Onsets.Contains(string.Join(" ",tokens.Skip(i-n).Take(n))))start=i-n;
     stress[start]=t[^1]=='1'?"ˈ":"ˌ";
    }previousVowel=i;
   }
  }
  return "/"+string.Concat(result.Select((v,i)=>(stress[i]??"")+v))+"/";
 }
}
