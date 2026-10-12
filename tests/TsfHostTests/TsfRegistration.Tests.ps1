$ErrorActionPreference = 'Stop'
$repo = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$tempRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
$folder = [IO.Path]::GetFullPath((Join-Path $tempRoot ("bilingual-tsf-plan-" + [guid]::NewGuid().ToString('N'))))
if (-not $folder.StartsWith($tempRoot,[StringComparison]::OrdinalIgnoreCase)) { throw 'Unsafe fixture path' }
New-Item -ItemType Directory -Path $folder | Out-Null
try {
  New-Item -ItemType File -Path (Join-Path $folder 'BilingualTsf.dll') | Out-Null
  New-Item -ItemType File -Path (Join-Path $folder 'TsfTrialRegistration.exe') | Out-Null
  $register = & (Join-Path $repo 'scripts/Register-TsfTrial.ps1') -PackageRoot $folder
  if ($register.Action -ne 'register' -or -not $register.DryRun) { throw 'Register must default to dry-run' }
  if ($register.CLSID -ne '{8A02E78F-A832-44AF-9A7F-120BD4851479}') { throw 'Unexpected trial CLSID' }
  if ($register.Profile -ne '{40E7CBF8-40C3-44F7-9031-1D0CC66138A9}') { throw 'Unexpected profile' }
  if ($register.DllPath -ne (Join-Path $folder 'BilingualTsf.dll')) { throw 'Incorrect DLL path' }
  if ($register.ComRegistryScope -ne 'HKCU\Software\Classes\CLSID') { throw 'Wrong COM registry scope' }
  if ($register.AutoActivate) { throw 'Trial must not auto-activate' }
  $unregister = & (Join-Path $repo 'scripts/Unregister-TsfTrial.ps1') -PackageRoot $folder
  if ($unregister.Action -ne 'unregister' -or -not $unregister.DryRun) { throw 'Unregister must default to dry-run' }
  if ($unregister.CLSID -ne $register.CLSID -or $unregister.Profile -ne $register.Profile) { throw 'Inverse plan differs' }
  Write-Output 'PASS TSF dry-run registration and inverse removal plan'
} finally {
  Remove-Item -LiteralPath $folder -Recurse -Force -ErrorAction SilentlyContinue
}
