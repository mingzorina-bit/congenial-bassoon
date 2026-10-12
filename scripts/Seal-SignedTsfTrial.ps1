param([Parameter(Mandatory)][string]$PackageRoot)
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path -LiteralPath $PackageRoot).Path
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
$inventory | ConvertTo-Json -Depth 3 | Set-Content $inventoryPath -Encoding utf8
& (Join-Path $PSScriptRoot 'Audit-Sources.ps1') -PackagePath $root -TsfTrial
$zip = "${root}-signed.zip"
if (Test-Path -LiteralPath $zip) { throw "Refusing to replace existing signed ZIP: $zip" }
Compress-Archive -Path "$root/*" -DestinationPath $zip
Get-FileHash -LiteralPath $zip -Algorithm SHA256 | Format-List
