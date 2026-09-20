$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$out = Join-Path $repo 'artifacts/BilingualInput-v0.0.1-win-x64'
New-Item -ItemType Directory -Path $out -Force | Out-Null
dotnet publish "$repo/src/PrototypeHost/PrototypeHost.csproj" -c Release -r win-x64 --self-contained true -o $out
if ($LASTEXITCODE) { throw 'WinUI publish failed' }
Copy-Item "$repo/build/Release/BilingualNative.dll" $out
Copy-Item "$repo/.vendor/rime/lib/rime.dll" $out
Copy-Item "$repo/data" $out -Recurse -Force
Copy-Item "$repo/THIRD_PARTY_SOURCES.md","$repo/README-TRY.md" $out
Copy-Item "$repo/licenses" $out -Recurse -Force
# App-local Microsoft VC runtime: the target machine must not need build tools.
$vswhere = "${env:ProgramFiles(x86)}/Microsoft Visual Studio/Installer/vswhere.exe"
$vs = & $vswhere -latest -products '*' -property installationPath
$crt = Get-ChildItem "$vs/VC/Redist/MSVC" -Recurse -Directory -Filter 'Microsoft.VC143.CRT' |
  Where-Object { $_.FullName -match '[\\/]x64[\\/]' } | Sort-Object FullName -Descending | Select-Object -First 1
if (-not $crt) { throw 'Microsoft app-local CRT missing' }
Copy-Item "$($crt.FullName)/*.dll" $out -Force
Copy-Item "$repo/src/PrototypeHost/obj/project.assets.json" "$out/dependency-assets.json"
$files = Get-ChildItem $out -Recurse -File
$inventory = foreach($f in $files) {
 [ordered]@{path=[IO.Path]::GetRelativePath($out,$f.FullName);sha256=(Get-FileHash $f.FullName).Hash}
}
$inventory | ConvertTo-Json -Depth 3 | Set-Content "$out/FILES.sha256.json" -Encoding utf8
Compress-Archive -Path "$out/*" -DestinationPath "$repo/artifacts/BilingualInput-v0.0.1-win-x64.zip" -Force
Get-FileHash "$repo/artifacts/BilingualInput-v0.0.1-win-x64.zip" | Format-List
