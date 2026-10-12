param([Parameter(Mandatory)][string]$PackagePath)
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path -LiteralPath $PackagePath).Path
$inventoryPath = Join-Path $root 'FILES.sha256.json'
if (-not (Test-Path -LiteralPath $inventoryPath)) { throw 'Package inventory missing' }
$inventory = @(Get-Content -LiteralPath $inventoryPath -Raw | ConvertFrom-Json)
$listed = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
foreach ($item in $inventory) {
  $relative = [string]$item.path
  if ([IO.Path]::IsPathRooted($relative) -or $relative -match '(^|[\\/])\.\.([\\/]|$)') { throw "Unsafe inventory path: $relative" }
  if (-not $listed.Add($relative.Replace('\','/'))) { throw "Duplicate inventory path: $relative" }
  $file = Join-Path $root $relative
  if (-not (Test-Path -LiteralPath $file -PathType Leaf)) { throw "Missing package file: $relative" }
  if ((Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash -ne $item.sha256) { throw "Hash mismatch: $relative" }
}
$actual = @(Get-ChildItem -LiteralPath $root -Recurse -File | Where-Object Name -ne 'FILES.sha256.json')
if ($actual.Count -ne $inventory.Count) { throw "Inventory count mismatch: listed $($inventory.Count), actual $($actual.Count)" }
foreach ($file in $actual) {
  $relative = [IO.Path]::GetRelativePath($root, $file.FullName).Replace('\','/')
  if (-not $listed.Contains($relative)) { throw "Unlisted package file: $relative" }
  if ($relative -match '(^|[\\/])\.env($|[\\/])|OPENAI_API_KEY|project\.assets\.json$') { throw "Sensitive or build-only path: $relative" }
}
$required = @(
  'THIRD_PARTY_SOURCES.md', 'README-TRY.md', 'dependency-assets.json',
  'licenses/librime-BSD.txt', 'licenses/CMUdict-LICENSE.txt',
  'licenses/rime-luna-pinyin-LGPL-3.0.txt',
  'licenses/rime-luna-pinyin-AUTHORS.txt', 'licenses/rime-essay-AUTHORS.txt',
  'data/rime/bilingual.dict.yaml', 'data/lexical/own-v0.0.2.tsv',
  'data/lexical/own-v021.tsv', 'data/details/own-v021.tsv',
  'data/rime/luna_pinyin.dict.yaml', 'data/rime/essay.txt', 'data/rime/SOURCE.md',
  'data/details/own-v004.tsv', 'data/pronunciation/cmudict-us.tsv'
)
foreach ($path in $required) { if (-not $listed.Contains($path)) { throw "Required source or notice missing: $path" } }
if ((Get-FileHash -LiteralPath (Join-Path $root 'data/rime/luna_pinyin.dict.yaml') -Algorithm SHA256).Hash -ne '75BCF6EB3FF62B129882ED89CC22B2D80A5347AA72BCFA2CCC839BAC298E7314') { throw 'Luna Pinyin source hash mismatch' }
if ((Get-FileHash -LiteralPath (Join-Path $root 'data/rime/essay.txt') -Algorithm SHA256).Hash -ne '3E8512EBAA6657E35961B1F5762060717C647E3BF471D5E3B86E783B4E934FFB') { throw 'Rime Essay source hash mismatch' }
$knownData = @(
  'data/details/own-v004.tsv', 'data/details/own-v021.tsv',
  'data/lexical/own-v0.0.2.tsv', 'data/lexical/own-v021.tsv',
  'data/lexical/README.md', 'data/pronunciation/cmudict-us.tsv',
  'data/pronunciation/SOURCE.md', 'data/rime/bilingual.dict.yaml',
  'data/rime/bilingual.schema.yaml', 'data/rime/default.yaml',
  'data/rime/luna_pinyin.dict.yaml', 'data/rime/essay.txt', 'data/rime/SOURCE.md'
)
foreach ($path in $listed) {
  $normalized = $path.Replace('\','/')
  if ($normalized.StartsWith('data/') -and $normalized -notin $knownData) { throw "Unreviewed data file: $path" }
}
Write-Output "Package inventory and reviewed data paths: PASS ($($inventory.Count) files)"
