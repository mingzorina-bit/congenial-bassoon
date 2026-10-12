param([string]$ReleaseVersion = 'v0.2.2-tsf-phase1a')
$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
& (Join-Path $PSScriptRoot 'Prepare-Lexicon.ps1')
$out = Join-Path $repo "artifacts/BilingualInput-$ReleaseVersion-win-x64"
if (Test-Path -LiteralPath $out) { throw "TSF package directory already exists: $out" }
New-Item -ItemType Directory -Path $out | Out-Null
Copy-Item (Join-Path $repo 'build/Release/BilingualTsf.dll') $out
Copy-Item (Join-Path $repo 'build/Release/TsfTrialRegistration.exe') $out
Copy-Item (Join-Path $repo '.vendor/rime/lib/rime.dll') $out
New-Item -ItemType Directory -Path (Join-Path $out 'data/rime') -Force | Out-Null
$rimeFiles = @('bilingual.dict.yaml','bilingual.schema.yaml','default.yaml',
  'luna_pinyin.dict.yaml','essay.txt','SOURCE.md')
foreach ($name in $rimeFiles) {
  Copy-Item (Join-Path $repo "data/rime/$name") (Join-Path $out "data/rime/$name")
}
Copy-Item (Join-Path $repo 'licenses') $out -Recurse
Copy-Item (Join-Path $out 'licenses/rime-luna-pinyin-LGPL-3.0.txt') (Join-Path $out 'licenses/rime-essay-LGPL-3.0.txt')
Copy-Item (Join-Path $repo 'THIRD_PARTY_SOURCES.md') $out
Copy-Item (Join-Path $repo 'docs/tsf/PHASE1A_TRIAL.md') (Join-Path $out 'README-TSF-TRIAL.md')
Copy-Item (Join-Path $repo 'docs/tsf/LOCAL_TEST_SIGNING.md') (Join-Path $out 'LOCAL_TEST_SIGNING.md')
$checkout = (& git -C $repo rev-parse HEAD).Trim()
if($LASTEXITCODE -ne 0){throw 'Cannot record checkout commit'}
$sourceCommit=$checkout
if($env:GITHUB_EVENT_PATH -and (Test-Path -LiteralPath $env:GITHUB_EVENT_PATH)){
  $event=Get-Content -LiteralPath $env:GITHUB_EVENT_PATH -Raw|ConvertFrom-Json
  if($event.pull_request.head.sha){$sourceCommit=$event.pull_request.head.sha}
}
@{sourceCommit=$sourceCommit;checkoutCommit=$checkout;runId=$env:GITHUB_RUN_ID;architecture='x64';signed=$false}|
  ConvertTo-Json|Set-Content (Join-Path $out 'BUILD-PROVENANCE.json') -Encoding utf8
New-Item -ItemType Directory -Path (Join-Path $out 'scripts') | Out-Null
foreach ($name in @('Register-TsfTrial.ps1','Unregister-TsfTrial.ps1',
    'Seal-SignedTsfTrial.ps1','Audit-Sources.ps1','Local-TsfSigning.ps1','TsfSigning.Common.ps1')) {
  Copy-Item (Join-Path $repo "scripts/$name") (Join-Path $out "scripts/$name")
}
# App-local VC runtime: external editors should not require Visual Studio.
$vswhere = "${env:ProgramFiles(x86)}/Microsoft Visual Studio/Installer/vswhere.exe"
$vs = & $vswhere -latest -products '*' -property installationPath
$crt = Get-ChildItem "$vs/VC/Redist/MSVC" -Recurse -Directory -Filter 'Microsoft.VC143.CRT' |
  Where-Object { $_.FullName -match '[\\/]x64[\\/]' } | Sort-Object FullName -Descending | Select-Object -First 1
if (-not $crt) { throw 'Microsoft app-local CRT missing' }
Copy-Item "$($crt.FullName)/*.dll" $out -Force
$inventory = foreach ($file in Get-ChildItem -LiteralPath $out -Recurse -File) {
  [ordered]@{
    path = [IO.Path]::GetRelativePath($out,$file.FullName)
    sha256 = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash
  }
}
$inventory | ConvertTo-Json -Depth 3 | Set-Content (Join-Path $out 'FILES.sha256.json') -Encoding utf8
& (Join-Path $PSScriptRoot 'Audit-Sources.ps1') -PackagePath $out -TsfTrial
$zip = "$out.zip"
Compress-Archive -Path "$out/*" -DestinationPath $zip
Get-FileHash -LiteralPath $zip -Algorithm SHA256 | Format-List
