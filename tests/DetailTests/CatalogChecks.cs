using BilingualInput;
internal static class CatalogChecks {
 public static void Run(Action<string,Action> test,Action<bool> check){
  var path=Path.GetTempFileName();var extension=Path.GetTempFileName();
  try{
   File.WriteAllText(path,"refine\t优化\tv.\t改进；完善\tImprove the details.\nfurther improve\t继续优化\tphrase\t进一步改进\tMake something better still.\nwork\t工作\tv. / n.\t工作\tDo a job.\n");
   File.WriteAllText(extension,"Beijing\t北京\tproper noun\t中国的首都\tThe capital city of China.\nBeijing area\t北京地区\tn. phrase\t北京及周边地区\tBeijing and its surrounding area.\n");
   var c=new DetailCatalog(path);
   test("ExactCatalogLookup",()=>{check(c.Find("refine")?.Pos=="v.");check(c.Find("refinement")==null);});
   test("NoWordFragmentsInSentence",()=>{check(c.In("homework is useful").Length==0);check(c.In("Refine this work.").Length==2);});
   test("PhraseUsesWholeEntry",()=>check(c.Find("further improve")?.Chinese=="继续优化"));
   test("MissingCatalogIsSafe",()=>check(new DetailCatalog("missing.tsv").In("refine").Length==0));
   test("TrialExtensionDetails",()=>{var both=new DetailCatalog(path,extension);check(both.Find("北京")?.English=="Beijing");check(both.Find("北京地区")?.English=="Beijing area");check(both.In("北京地区\nBeijing area").Any(e=>e.English=="Beijing area"));});
  }finally{File.Delete(path);File.Delete(extension);}
 }
}
