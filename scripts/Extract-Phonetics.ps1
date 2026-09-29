param([Parameter(Mandatory)][string]$DictionaryPath)
$ErrorActionPreference='Stop'
$repo=Split-Path $PSScriptRoot -Parent
$expected='81917843C7F44CE2B094AC63873C2C7A4CF802040792C455BA3CA406891C3D22'
if((Get-FileHash $DictionaryPath -Algorithm SHA256).Hash -ne $expected){throw 'CMUdict source hash mismatch'}
$words=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
Get-Content "$repo/data/lexical/own-v0.0.2.tsv" | Where-Object {$_ -notmatch '^#'} | ForEach-Object {
 $fields=$_ -split "`t"
 $english=if($fields[1] -eq 'EN'){$fields[2]}else{$fields[0]}
 $english -split ' ' | ForEach-Object {[void]$words.Add($_)}
}
$subset=@('# CMUdict 74790861f652b15e4ac49015a90074ad62a27690 | en-US | word TAB original ARPAbet')
Get-Content $DictionaryPath | ForEach-Object {
 $fields=$_ -split ' ',2
 if($words.Contains($fields[0])){$subset+=$fields[0]+"`t"+($fields[1] -split ' #')[0]}
}
New-Item -ItemType Directory "$repo/data/pronunciation" -Force|Out-Null
$subset|Set-Content "$repo/data/pronunciation/cmudict-us.tsv" -Encoding utf8

