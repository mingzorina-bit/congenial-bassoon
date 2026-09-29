using BilingualInput;
int failed=0,count=0;
void Test(string name,Func<bool> f){count++;if(f())Console.WriteLine("PASS "+name);else{failed++;Console.WriteLine("FAIL "+name);}}
Test("CurrentSentenceOnly",()=>ExpressionRange.At("Hello. I think 方案 is better.",17,0).Text==" I think 方案 is better.");
Test("SelectionIsExact",()=>ExpressionRange.At("😀 I think 方案 works",11,2).Text=="方案");
Test("PreviousLineNotIncluded",()=>ExpressionRange.At("旧句\nWe need 优化",13,0).Text=="We need 优化");
Test("StaleResultRejected",()=>!new ExpressionRange(0,2,"方案").Matches("优化"));
Test("UnchangedAccepted",()=>new ExpressionRange(0,2,"方案").Matches("方案"));
Test("EndPunctuationIncluded",()=>ExpressionRange.At("I think 方案.",11,0).Text=="I think 方案.");
Test("EmptySafe",()=>ExpressionRange.At("",0,0).Length==0);
Test("CandidateRejectsContextChange",()=>!new CandidateSnapshot("he",0,"he").Matches("he",new[]{"和"}));
Test("CandidateRejectsCompositionChange",()=>!new CandidateSnapshot("he",0,"he").Matches("design",new[]{"he"}));
Test("CandidateAcceptsCurrent",()=>new CandidateSnapshot("he",0,"和").Matches("he",new[]{"和"}));
Console.WriteLine($"{count} tests, {failed} failures");return failed==0?0:1;
