$ErrorActionPreference='Stop'
$repo=Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
. (Join-Path $repo 'scripts/TsfSigning.Common.ps1')
$work=Join-Path $repo ('artifacts/bilingual-seal-'+[guid]::NewGuid().ToString('N'))
$root=Join-Path $work 'signed-package';New-Item -ItemType Directory -Path $root -Force|Out-Null
$rsa=[Security.Cryptography.RSA]::Create(2048)
$req=[Security.Cryptography.X509Certificates.CertificateRequest]::new('CN=BilingualInput Phase1A Local Trial',$rsa,[Security.Cryptography.HashAlgorithmName]::SHA256,[Security.Cryptography.RSASignaturePadding]::Pkcs1)
$oids=[Security.Cryptography.OidCollection]::new();$oids.Add([Security.Cryptography.Oid]::new('1.3.6.1.5.5.7.3.3'))|Out-Null
$req.CertificateExtensions.Add([Security.Cryptography.X509Certificates.X509EnhancedKeyUsageExtension]::new($oids,$true))
$req.CertificateExtensions.Add([Security.Cryptography.X509Certificates.X509KeyUsageExtension]::new([Security.Cryptography.X509Certificates.X509KeyUsageFlags]::DigitalSignature,$true))
$global:TsfSealCert=$req.CreateSelfSigned([datetimeoffset]::Now.AddDays(-1),[datetimeoffset]::Now.AddDays(29))
$global:TsfSealForeign=$false
function Get-AuthenticodeSignature {param($LiteralPath)
  if($LiteralPath -like '*fake-signtool.ps1'){return [pscustomobject]@{Status='Valid';SignerCertificate=[pscustomobject]@{Subject='O=Microsoft Corporation'}}}
  $signer=if($global:TsfSealForeign){[pscustomobject]@{Thumbprint=('F'*40)}}else{$global:TsfSealCert}
  [pscustomobject]@{Status='Valid';SignerCertificate=$signer}
}
function RejectSeal($pattern){$failed=$false;try{& (Join-Path $repo 'scripts/Seal-SignedTsfTrial.ps1') -PackageRoot $root -StatePath $statePath -SignToolPath $tool -Apply|Out-Null}catch{$failed=$_.Exception.Message -like $pattern; if(-not $failed){Write-Output $_.Exception.Message}};if(-not $failed){throw "Expected sealing rejection: $pattern"}}
try {
  foreach($name in @('BilingualTsf.dll','rime.dll','TsfTrialRegistration.exe','default.yaml')){Set-Content (Join-Path $root $name) 'fixture'}
  $inventory=Join-Path $root 'FILES.sha256.json';Set-Content $inventory 'original inventory'
  $before=(Get-FileHash $inventory).Hash
  $tool=Join-Path $work 'fake-signtool.ps1';Set-Content $tool '$global:LASTEXITCODE=0'
  [IO.File]::WriteAllBytes((Join-Path $work 'public.cer'),$global:TsfSealCert.Export([Security.Cryptography.X509Certificates.X509ContentType]::Cert))
  $statePath=Join-Path $work 'state.json'
  $payload=@(Get-ChildItem $root -File|ForEach-Object{@{path=$_.Name;sha256=(Get-FileHash $_.FullName).Hash}})
  $state=@{Schema=1;Identity=(Get-TsfLocalIdentity);Thumbprint=$global:TsfSealCert.Thumbprint;Payload=$payload;SignedHashes=@{}}
  foreach($name in @('BilingualTsf.dll','rime.dll','TsfTrialRegistration.exe')){$state.SignedHashes[$name]=(Get-FileHash (Join-Path $root $name)).Hash}
  Save-TsfSigningState $state $statePath
  $plan=& (Join-Path $repo 'scripts/Seal-SignedTsfTrial.ps1') -PackageRoot $root
  if(-not $plan.DryRun -or (Get-FileHash $inventory).Hash -ne $before){throw 'Seal preview changed inventory'}
  # All payload/signature guards pass; the real source audit rejects this incomplete fixture.
  RejectSeal '*Unreviewed file*'
  if((Get-FileHash $inventory).Hash -ne $before){throw 'Failed source audit did not restore original inventory'}
  Add-Content (Join-Path $root 'default.yaml') 'tampered'
  RejectSeal '*payload mismatch*'
  Set-Content (Join-Path $root 'default.yaml') 'fixture'
  $global:TsfSealForeign=$true;RejectSeal '*trusted signature*';$global:TsfSealForeign=$false
  Set-Content "$root-signed.zip" 'existing archive'
  $beforeZip=(Get-FileHash "$root-signed.zip").Hash
  RejectSeal '*replace existing*'
  if((Get-FileHash "$root-signed.zip").Hash -ne $beforeZip -or (Get-FileHash $inventory).Hash -ne $before){throw 'Existing archive/inventory was not protected'}
  Write-Output 'PASS Seal preview, audit rollback, changed data, foreign signer and existing ZIP preservation'
}finally{
  $global:TsfSealCert.Dispose();$rsa.Dispose();Remove-Variable TsfSealCert,TsfSealForeign -Scope Global
  $absolute=[IO.Path]::GetFullPath($work);$parent=[IO.Path]::GetFullPath((Join-Path $repo 'artifacts'))+[IO.Path]::DirectorySeparatorChar
  if($absolute.StartsWith($parent,[StringComparison]::OrdinalIgnoreCase) -and (Split-Path $absolute -Leaf) -like 'bilingual-seal-*'){Remove-Item -LiteralPath $absolute -Recurse -Force}
}

