$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$vendor = Join-Path $repo '.vendor'
New-Item -ItemType Directory -Path $vendor -Force | Out-Null
$source = Join-Path $vendor 'librime'
if (-not (Test-Path (Join-Path $source 'CMakeLists.txt'))) {
  git clone --depth 1 --branch 1.13.1 https://github.com/rime/librime.git $source
  if ($LASTEXITCODE) { throw 'librime clone failed' }
}
$commit = git -C $source rev-parse HEAD
if ($commit -ne '1c23358157934bd6e6d6981f0c0164f05393b497') { throw 'Unexpected librime revision' }
# Official top-level rime.dll includes GPL plugins. Never use it.
$archive = Join-Path $vendor 'rime-deps.7z'
if (-not (Test-Path $archive)) {
  Invoke-WebRequest 'https://github.com/rime/librime/releases/download/1.13.1/rime-deps-1c23358-Windows-msvc-x64.7z' -OutFile $archive
}
if ((Get-FileHash $archive -Algorithm SHA256).Hash -ne '3EDE059E6C1F4CDD5843CED3205F76666B706E5F55CCF8E56E2D04791A376FF6') { throw 'Dependency archive hash mismatch' }
7z x $archive "-o$source" -y | Out-Null
if ($LASTEXITCODE) { throw 'Dependency extraction failed' }
$boost = Get-ChildItem 'C:/local' -Directory -Filter 'boost_*' | Sort-Object Name -Descending | Select-Object -First 1
if (-not $boost) { throw 'Runner Boost headers unavailable' }
$build = Join-Path $vendor 'rime-build'
$dist = Join-Path $vendor 'rime'
cmake -S $source -B $build -A x64 "-DBOOST_ROOT=$($boost.FullName)" '-DBoost_NO_BOOST_CMAKE=ON' '-DCMAKE_POLICY_VERSION_MINIMUM=3.5' '-DBUILD_TEST=OFF' '-DBUILD_STATIC=ON' '-DENABLE_LOGGING=OFF' '-DBUILD_MERGED_PLUGINS=OFF' '-DENABLE_EXTERNAL_PLUGINS=OFF' "-DCMAKE_INSTALL_PREFIX=$dist"
if ($LASTEXITCODE) { throw 'Rime configuration failed' }
cmake --build $build --config Release --target rime --parallel 4
if ($LASTEXITCODE) { throw 'Rime build failed' }
New-Item -ItemType Directory -Path "$dist/include","$dist/lib" -Force | Out-Null
Copy-Item "$source/src/rime_api.h" "$dist/include/"
Copy-Item "$build/bin/Release/rime.dll" "$dist/lib/"
Copy-Item "$build/src/Release/rime.lib" "$dist/lib/"
Write-Output "Rime built from $commit without plugins or content logging."
