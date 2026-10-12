$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$items = @(
  @{
    Path = 'data/rime/luna_pinyin.dict.yaml'
    Url = 'https://raw.githubusercontent.com/rime/rime-luna-pinyin/56b934b099dfbeab842320f13aa8b461a6ab3e42/luna_pinyin.dict.yaml'
    Sha256 = '75BCF6EB3FF62B129882ED89CC22B2D80A5347AA72BCFA2CCC839BAC298E7314'
  },
  @{
    Path = 'data/rime/essay.txt'
    Url = 'https://raw.githubusercontent.com/rime/rime-essay/054920de4f54c9e5994276a96a4fc2a35cb51aa3/essay.txt'
    Sha256 = '3E8512EBAA6657E35961B1F5762060717C647E3BF471D5E3B86E783B4E934FFB'
  }
)
foreach ($item in $items) {
  $target = Join-Path $repo $item.Path
  if (-not (Test-Path -LiteralPath $target)) {
    New-Item -ItemType Directory -Path (Split-Path $target -Parent) -Force | Out-Null
    $temporary = "$target.download"
    try {
      Invoke-WebRequest -Uri $item.Url -OutFile $temporary
      if ((Get-FileHash -LiteralPath $temporary -Algorithm SHA256).Hash -ne $item.Sha256) {
        throw "Lexicon source hash mismatch: $($item.Path)"
      }
      Move-Item -LiteralPath $temporary -Destination $target
    } finally {
      if (Test-Path -LiteralPath $temporary) { Remove-Item -LiteralPath $temporary }
    }
  }
  if ((Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash -ne $item.Sha256) {
    throw "Lexicon source hash mismatch: $($item.Path)"
  }
}
$license = Join-Path $repo 'licenses/rime-luna-pinyin-LGPL-3.0.txt'
if ((Get-FileHash -LiteralPath $license -Algorithm SHA256).Hash -ne 'DA7EABB7BAFDF7D3AE5E9F223AA5BDC1EECE45AC569DC21B3B037520B4464768') {
  throw 'Rime lexicon license hash mismatch'
}
Write-Output 'Pinned Luna Pinyin and Rime Essay source hashes: PASS'
