param(
  [string]$PackageRoot = (Split-Path $PSScriptRoot -Parent),
  [switch]$Apply
)
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath($PackageRoot)
$helper = Join-Path $root 'TsfTrialRegistration.exe'
$plan = [pscustomobject]@{
  Action = 'unregister'
  DryRun = -not $Apply.IsPresent
  CLSID = '{8A02E78F-A832-44AF-9A7F-120BD4851479}'
  Profile = '{40E7CBF8-40C3-44F7-9031-1D0CC66138A9}'
  ComRegistryScope = 'HKCU\Software\Classes\CLSID'
  TsfScope = 'Only this fixed trial TSF service, zh-CN profile and keyboard category'
  RequiresElevation = 'May be required by Windows TSF profile policy; not requested automatically'
  AutoActivate = $false
}
if (-not $Apply) { return $plan }
if (-not (Test-Path -LiteralPath $helper -PathType Leaf)) { throw 'Registration helper missing; recover it from the original trial ZIP' }
& $helper unregister
if ($LASTEXITCODE -ne 0) { throw "TSF removal failed: exit $LASTEXITCODE" }
$plan
