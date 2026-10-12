param(
  [string]$PackageRoot = (Split-Path $PSScriptRoot -Parent),
  [string]$StatePath,
  [string]$SignToolPath,
  [switch]$Apply
)
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath($PackageRoot)
$dll = Join-Path $root 'BilingualTsf.dll'
$helper = Join-Path $root 'TsfTrialRegistration.exe'
$rime = Join-Path $root 'rime.dll'
$plan = [pscustomobject]@{
  Action = 'register'
  DryRun = -not $Apply.IsPresent
  CLSID = '{8A02E78F-A832-44AF-9A7F-120BD4851479}'
  Profile = '{40E7CBF8-40C3-44F7-9031-1D0CC66138A9}'
  DllPath = $dll
  ComRegistryScope = 'HKCU\Software\Classes\CLSID'
  TsfScope = 'Windows-managed TSF service, zh-CN profile and keyboard category'
  RequiresElevation = 'May be required by Windows TSF profile policy; not requested automatically'
  AutoActivate = $false
}
if (-not $Apply) { return $plan }
if (-not (Test-Path -LiteralPath $dll -PathType Leaf)) { throw 'Trial DLL missing' }
if (-not (Test-Path -LiteralPath $helper -PathType Leaf)) { throw 'Registration helper missing' }
if (-not (Test-Path -LiteralPath $rime -PathType Leaf)) { throw 'Local Rime engine missing' }
$dllSignature = Get-AuthenticodeSignature -LiteralPath $dll
$helperSignature = Get-AuthenticodeSignature -LiteralPath $helper
$rimeSignature = Get-AuthenticodeSignature -LiteralPath $rime
if ($dllSignature.Status -ne 'Valid' -or $helperSignature.Status -ne 'Valid' -or $rimeSignature.Status -ne 'Valid') {
  throw 'Registration is blocked until the TSF DLL, Rime engine and helper have valid trusted code signatures.'
}
if(-not $StatePath -or -not $SignToolPath){throw 'Local StatePath and official SignToolPath required before registration'}
. (Join-Path $PSScriptRoot 'TsfSigning.Common.ps1')
$expectedRoot=Join-Path (Split-Path ([IO.Path]::GetFullPath($StatePath)) -Parent) 'signed-package'
if($root -ne $expectedRoot){throw 'Registration package must match recorded signed output'}
Invoke-TsfSigningOperation -Mode Verify -StatePath $StatePath -SignToolPath $SignToolPath -Apply|Out-Null
& (Join-Path $PSScriptRoot 'Audit-Sources.ps1') -PackagePath $root -TsfTrial
& $helper register $dll
if ($LASTEXITCODE -ne 0) { throw "TSF registration failed: exit $LASTEXITCODE. Run Unregister-TsfTrial.ps1 -Apply to clean partial state." }
$plan
