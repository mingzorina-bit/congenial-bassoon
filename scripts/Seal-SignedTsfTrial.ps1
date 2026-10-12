param([Parameter(Mandatory)][string]$PackageRoot,[string]$StatePath,[string]$SignToolPath,[switch]$Apply)
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path -LiteralPath $PackageRoot).Path
if(-not $Apply){return [pscustomobject]@{Action='seal';DryRun=$true;PackageRoot=$root;OutputZip="${root}-signed.zip";StatePath=$StatePath;Requires='Recorded payload, exact signer, SignTool, trusted signatures and package audit';AutoRegister=$false}}
$zip = "${root}-signed.zip"
if (Test-Path -LiteralPath $zip) { throw "Refusing to replace existing signed ZIP: $zip" }
if(-not $StatePath -or -not $SignToolPath){throw 'Local StatePath and official SignToolPath required before sealing'}
. (Join-Path $PSScriptRoot 'TsfSigning.Common.ps1')
$expectedRoot=Join-Path (Split-Path ([IO.Path]::GetFullPath($StatePath)) -Parent) 'signed-package'
if($root -ne $expectedRoot){throw 'Seal package must match the recorded signed output'}
Invoke-TsfSigningOperation -Mode Verify -StatePath $StatePath -SignToolPath $SignToolPath -Apply|Out-Null
$inventoryPath = Join-Path $root 'FILES.sha256.json'
foreach ($name in @('BilingualTsf.dll','rime.dll','TsfTrialRegistration.exe')) {
  $path = Join-Path $root $name
  if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Missing signed binary: $name" }
  if ((Get-AuthenticodeSignature -LiteralPath $path).Status -ne 'Valid') {
    throw "Trusted signature missing or invalid: $name"
  }
}
$inventory = foreach ($file in Get-ChildItem -LiteralPath $root -Recurse -File |
    Where-Object FullName -ne $inventoryPath) {
  [ordered]@{
    path = [IO.Path]::GetRelativePath($root,$file.FullName)
    sha256 = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash
  }
}
$previous = if(Test-Path -LiteralPath $inventoryPath){[IO.File]::ReadAllBytes($inventoryPath)}else{$null}
try {
  $inventory | ConvertTo-Json -Depth 3 | Set-Content $inventoryPath -Encoding utf8
  & (Join-Path $PSScriptRoot 'Audit-Sources.ps1') -PackagePath $root -TsfTrial
  Compress-Archive -Path "$root/*" -DestinationPath $zip
  $localState=Read-TsfSigningState $StatePath
  $localState.SealedZipHash=(Get-FileHash -LiteralPath $zip -Algorithm SHA256).Hash
  $localState.Phase='Sealed'
  Save-TsfSigningState $localState $StatePath
} catch {
  if($null -ne $previous){[IO.File]::WriteAllBytes($inventoryPath,$previous)}elseif(Test-Path $inventoryPath){Remove-Item -LiteralPath $inventoryPath}
  if(Test-Path -LiteralPath $zip){Remove-Item -LiteralPath $zip}
  throw
}
Get-FileHash -LiteralPath $zip -Algorithm SHA256 | Format-List
