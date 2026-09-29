using BilingualInput;
var cases = new (string Name,int Code,bool Shift,bool Caps,bool Composing,bool Shortcut,InputAction Want,int Value)[] {
 ("LettersCompose",65,false,false,false,false,InputAction.Type,97),
 ("ShiftCase",65,true,false,false,false,InputAction.Type,65),
 ("CapsShiftRestoresLowercase",65,true,true,false,false,InputAction.Type,97),
 ("EnterIsRaw",13,false,false,true,false,InputAction.CoreKey,0),
 ("SpaceUsesPrimary",32,false,false,true,false,InputAction.CoreKey,1),
 ("NumberSelects",50,false,false,true,false,InputAction.Select,1),
 ("ShadowNumberSelects",50,true,false,true,false,InputAction.SelectShadow,1),
 ("ShortcutPreservesPendingText",86,false,false,true,true,InputAction.FlushRaw,0),
 ("ApostropheIsPinyinDelimiter",222,false,false,true,false,InputAction.Type,39),
 ("CommaFollowsComposition",188,false,false,true,false,InputAction.FlushPrimary,0),
 ("ZeroFollowsComposition",48,false,false,true,false,InputAction.FlushPrimary,0),
 ("NumpadFollowsComposition",96,false,false,true,false,InputAction.FlushPrimary,0),
 ("NumpadOperatorFollowsComposition",107,false,false,true,false,InputAction.FlushPrimary,0),
 ("QuoteFollowsComposition",222,true,false,true,false,InputAction.FlushPrimary,0),
 ("HomePreservesPendingText",36,false,false,true,false,InputAction.FlushRaw,0),
 ("DeletePreservesPendingText",46,false,false,true,false,InputAction.FlushRaw,0),
 ("CommaWithoutCompositionPasses",188,false,false,false,false,InputAction.Pass,0)
};
int failures=0;
foreach(var c in cases){
 var actual=KeyboardRouter.Decide(c.Code,c.Shift,c.Caps,c.Composing,c.Shortcut);
 bool ok=actual.Action==c.Want && actual.Value==c.Value;
 Console.WriteLine($"{(ok?"PASS":"FAIL")} {c.Name}");
 if(!ok)failures++;
}
Console.WriteLine($"{cases.Length} routing tests, {failures} failures");
return failures==0?0:1;

