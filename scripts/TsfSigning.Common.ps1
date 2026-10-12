# Local-only OS operations are wrapped so safety tests never modify certificate stores.
$TsfSigningBinaries=@('BilingualTsf.dll','rime.dll','TsfTrialRegistration.exe')
$TsfSigningSubject='CN=BilingualInput Phase1A Local Trial'
function Get-TsfLocalIdentity {
  @{Machine=[Environment]::MachineName;UserSid=[Security.Principal.WindowsIdentity]::GetCurrent().User.Value;
    MachineId=(Get-ItemProperty -LiteralPath 'Registry::HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Cryptography').MachineGuid}
}
function Assert-TsfStandardUser {
  $principal=[Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())
  if($principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Use a standard unelevated PowerShell session'}
}
function Read-TsfSigningState([string]$Path) {
  if(-not(Test-Path -LiteralPath $Path -PathType Leaf)){throw 'Local state missing; preview or create certificate first'}
  $s=Get-Content -LiteralPath $Path -Raw|ConvertFrom-Json -AsHashtable
  if($s.Schema -ne 1 -or $s.Thumbprint -notmatch '^[0-9A-F]{40}$'){throw 'Invalid local signing state'}
  $identity=Get-TsfLocalIdentity
  foreach($field in @('Machine','UserSid','MachineId')){if($s.Identity[$field] -ne $identity[$field]){throw 'State belongs to another machine/account'}}
  return $s
}
function Save-TsfSigningState($State,[string]$Path) {
  $parent=Split-Path ([IO.Path]::GetFullPath($Path)) -Parent
  if(-not(Test-Path -LiteralPath $parent)){New-Item -ItemType Directory -Path $parent|Out-Null}
  $temp="$Path.new"
  $State|ConvertTo-Json -Depth 12|Set-Content -LiteralPath $temp -Encoding utf8
  [IO.File]::Move($temp,$Path,$true)
}
function Get-TsfCertificate([string]$Thumbprint) { Get-Item -LiteralPath "Cert:\CurrentUser\My\$Thumbprint" -ErrorAction Stop }
function Get-TsfPersonalCertificateOrNull([string]$Thumbprint){Get-Item -LiteralPath "Cert:\CurrentUser\My\$Thumbprint" -ErrorAction SilentlyContinue}
function Get-TsfKeyMetadata($Certificate) {
  $rsa=[Security.Cryptography.X509Certificates.RSACertificateExtensions]::GetRSAPrivateKey($Certificate)
  try {
    if($rsa -isnot [Security.Cryptography.RSACng]){throw 'Expected an owned CNG RSA private key'}
    @{Name=$rsa.Key.KeyName;UniqueName=$rsa.Key.UniqueName;Provider=$rsa.Key.Provider.Provider;ExportPolicy=$rsa.Key.ExportPolicy.ToString();MachineKey=$rsa.Key.IsMachineKey}
  } finally {if($rsa){$rsa.Dispose()}}
}
function Assert-TsfCertificate($Certificate,$State,[switch]$PrivateKey,[switch]$AllowExpired) {
  if($Certificate.Thumbprint -cne $State.Thumbprint){throw 'Certificate thumbprint mismatch'}
  if($Certificate.Subject -ne $TsfSigningSubject -or $Certificate.Issuer -ne $TsfSigningSubject){throw 'Unexpected certificate identity'}
  if($Certificate.SignatureAlgorithm.Value -ne '1.2.840.113549.1.1.11'){throw 'SHA-256 RSA certificate signature required'}
  if($Certificate.NotBefore -gt (Get-Date) -or (-not $AllowExpired -and $Certificate.NotAfter -le (Get-Date)) -or
     ($Certificate.NotAfter-$Certificate.NotBefore).TotalDays -gt 31){throw 'Certificate validity outside local 30-day policy'}
  $eku=$Certificate.Extensions|Where-Object {$_.Oid.Value -eq '2.5.29.37'}
  if(-not $eku -or '1.3.6.1.5.5.7.3.3' -notin @($eku.EnhancedKeyUsages|ForEach-Object Value)){throw 'Code Signing EKU missing'}
  $usage=$Certificate.Extensions|Where-Object {$_.Oid.Value -eq '2.5.29.15'}
  if(-not $usage -or -not($usage.KeyUsages -band [Security.Cryptography.X509Certificates.X509KeyUsageFlags]::DigitalSignature)){throw 'DigitalSignature usage missing'}
  $rsa=[Security.Cryptography.X509Certificates.RSACertificateExtensions]::GetRSAPublicKey($Certificate)
  try{if(-not $rsa -or $rsa.KeySize -lt 2048){throw 'RSA 2048 or stronger required'}}finally{if($rsa){$rsa.Dispose()}}
  if($PrivateKey){
    if(-not $Certificate.HasPrivateKey){throw 'Private key missing'}
    $key=Get-TsfKeyMetadata $Certificate
    if($key.MachineKey -or $key.ExportPolicy -ne 'None' -or $key.Provider -ne 'Microsoft Software Key Storage Provider'){throw 'Private key must be user-scoped and non-exportable'}
    if($State.Key -and $State.Key.Name -ne $key.Name){throw 'Owned key container mismatch'}
  }
}
function New-TsfPersonalCertificate {
  New-SelfSignedCertificate -Type CodeSigningCert -Subject $TsfSigningSubject -CertStoreLocation 'Cert:\CurrentUser\My' `
    -Provider 'Microsoft Software Key Storage Provider' -KeyAlgorithm RSA -KeyLength 2048 -HashAlgorithm SHA256 `
    -KeyUsage DigitalSignature -KeyExportPolicy NonExportable -KeyFriendlyName ('BilingualInput-LocalTrial-'+[guid]::NewGuid()) `
    -NotAfter (Get-Date).AddDays(30)
}
function Export-TsfPublicCertificate($Certificate,[string]$Path){Export-Certificate -Cert $Certificate -FilePath $Path -Type CERT|Out-Null}
function Read-TsfPublicCertificate([string]$Path){[Security.Cryptography.X509Certificates.X509Certificate2]::new($Path)}
function Remove-TsfPersonalCertificate([string]$Thumbprint,$Key){
  if(Test-Path -LiteralPath "Cert:\CurrentUser\My\$Thumbprint"){Remove-Item -LiteralPath "Cert:\CurrentUser\My\$Thumbprint" -DeleteKey -ErrorAction Stop}
  if(Test-Path -LiteralPath "Cert:\CurrentUser\My\$Thumbprint"){throw 'Personal certificate removal incomplete'}
  if($Key -and $Key.Name){
    if($Key.Provider -ne 'Microsoft Software Key Storage Provider' -or $Key.MachineKey){throw 'Unsafe dedicated key cleanup metadata'}
    $provider=[Security.Cryptography.CngProvider]::new($Key.Provider)
    if([Security.Cryptography.CngKey]::Exists($Key.Name,$provider,[Security.Cryptography.CngKeyOpenOptions]::None)){
      $ownedKey=[Security.Cryptography.CngKey]::Open($Key.Name,$provider,[Security.Cryptography.CngKeyOpenOptions]::None)
      try{$ownedKey.Delete()}finally{$ownedKey.Dispose()}
    }
    if([Security.Cryptography.CngKey]::Exists($Key.Name,$provider,[Security.Cryptography.CngKeyOpenOptions]::None)){throw 'Dedicated private key still exists; retain local state for recovery'}
  }
}
function Test-TsfTrustCopy([string]$Store,[string]$Thumbprint){Test-Path -LiteralPath "Cert:\CurrentUser\$Store\$Thumbprint"}
function Add-TsfTrustCopy([string]$Store,[string]$CertificateFile){Import-Certificate -FilePath $CertificateFile -CertStoreLocation "Cert:\CurrentUser\$Store"|Out-Null}
function Remove-TsfTrustCopy([string]$Store,[string]$Thumbprint){
  Remove-Item -LiteralPath "Cert:\CurrentUser\$Store\$Thumbprint" -ErrorAction Stop
  if(Test-TsfTrustCopy $Store $Thumbprint){throw "Trust copy still present: $Store"}
}
function Get-TsfFileSignature([string]$Path){Get-AuthenticodeSignature -LiteralPath $Path}
function Assert-TsfSignTool([string]$Path){
  if(-not $Path -or -not(Test-Path -LiteralPath $Path -PathType Leaf)){throw 'Provide official Windows SDK SignToolPath'}
  $sig=Get-AuthenticodeSignature -LiteralPath $Path
  if($sig.Status -ne 'Valid' -or $sig.SignerCertificate.Subject -notmatch 'O=Microsoft Corporation'){throw 'SignTool must have a valid Microsoft signature'}
}
function Invoke-TsfSignTool([string]$Path,[string[]]$Arguments){
  $messages=& $Path @Arguments 2>&1
  $code=$LASTEXITCODE
  if($Arguments[0] -eq 'sign' -and $code -ne 0){throw "SignTool signing failed (exit $code)"}
  return $code
}
function Invoke-TsfPackageAudit([string]$Root){& (Join-Path $PSScriptRoot 'Audit-Sources.ps1') -PackagePath $Root -TsfTrial|Out-Null}
function Get-TsfRegistrationStatus([string]$PackageRoot){
  $helper=Join-Path $PackageRoot 'TsfTrialRegistration.exe'
  if(-not(Test-Path -LiteralPath $helper -PathType Leaf)){throw 'Matching removal/status helper missing'}
  $null=& $helper status
  if($LASTEXITCODE -notin @(0,6)){throw "TSF status cannot be established (exit $LASTEXITCODE)"}
  return $LASTEXITCODE
}
function Invoke-TsfUnregister([string]$PackageRoot){& (Join-Path $PSScriptRoot 'Unregister-TsfTrial.ps1') -PackageRoot $PackageRoot -Apply|Out-Null}
function Remove-TsfSigningOutput([string]$Path,[string]$WorkRoot){
  $root=[IO.Path]::GetFullPath($WorkRoot).TrimEnd('\','/')+[IO.Path]::DirectorySeparatorChar
  $target=[IO.Path]::GetFullPath($Path)
  if(-not $target.StartsWith($root,[StringComparison]::OrdinalIgnoreCase) -or (Split-Path $target -Leaf) -ne 'signed-package'){throw 'Unsafe signing output cleanup target'}
  if(Test-Path -LiteralPath $target){Remove-Item -LiteralPath $target -Recurse -Force}
}
function Assert-TsfNoReparsePoint([string]$Root){
  $items=@(Get-Item -LiteralPath $Root)+@(Get-ChildItem -LiteralPath $Root -Recurse -Force)
  if($items|Where-Object {$_.Attributes -band [IO.FileAttributes]::ReparsePoint}){throw 'Reparse points are not allowed in signing inputs'}
}
function Assert-TsfSignedPayload($State,[string]$Root){
  if(-not $State.Payload -or $State.SignedHashes.Count -ne 3){throw 'Prepared signed output missing'}
  Assert-TsfNoReparsePoint $Root
  $files=@(Get-ChildItem -LiteralPath $Root -Recurse -File -Force)
  # Seal may rebuild inventory, but cannot add any other untracked payload.
  if($files.Count -ne $State.Payload.Count){throw 'Signed payload file count mismatch'}
  foreach($item in $State.Payload){
    if([IO.Path]::IsPathRooted($item.path) -or $item.path -match '(^|[\\/])\.\.([\\/]|$)'){throw 'Unsafe signed payload path'}
    $path=Join-Path $Root $item.path
    $expected=if($State.SignedHashes.ContainsKey($item.path)){$State.SignedHashes[$item.path]}elseif($item.path -eq 'FILES.sha256.json'){continue}else{$item.sha256}
    if(-not(Test-Path -LiteralPath $path -PathType Leaf) -or (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash -ne $expected){throw "Signed payload mismatch: $($item.path)"}
  }
}
function Invoke-TsfSigningOperation {
  [CmdletBinding()]
  param([Parameter(Mandatory)][ValidateSet('NewCertificate','Sign','Verify','Trust','Revoke')][string]$Mode,
    [string]$StatePath=(Join-Path (Split-Path $PSScriptRoot -Parent) 'signing-local/state.json'),
    [string]$PackageRoot,[string]$SourceCommit,[string]$SignToolPath,[string]$Thumbprint,
    [switch]$Apply,[switch]$ApproveTrust,[switch]$ApproveRevoke,[switch]$Unregister,[switch]$DeleteOutputs)
  $ErrorActionPreference='Stop'
  $statePath=[IO.Path]::GetFullPath($StatePath)
  $work=Split-Path $statePath -Parent
  $output=Join-Path $work 'signed-package'
  $cer=Join-Path $work 'public.cer'
  $state=if(Test-Path -LiteralPath $statePath){Read-TsfSigningState $statePath}else{$null}
  $plan=[ordered]@{Action=$Mode;DryRun=(-not $Apply);StatePath=$statePath;Thumbprint=$(if($state){$state.Thumbprint}else{'NOT_CREATED'});
    NotBefore=$(if($state){$state.NotBefore}else{'Created on explicit Apply'});NotAfter=$(if($state){$state.NotAfter}else{'30 days after explicit creation'});
    ValidityDays=30;PersonalStore='Cert:\CurrentUser\My';TrustStores=@('Cert:\CurrentUser\Root','Cert:\CurrentUser\TrustedPublisher');
    Permission='Current user, unelevated; no automatic elevation';PrivateKey='RSA 2048; SHA-256; non-exportable CNG user key; never PFX';
    SignedOutput=$output;CertificateFile=$cer;SourceCommit=$SourceCommit;AutoRegister=$false;AutoActivate=$false;
    Rollback='Unregister and verify absent; remove only journaled trust additions; delete owned My certificate with DeleteKey; optional output cleanup'}
  if(-not $Apply){return [pscustomobject]$plan}
  if($Mode -ne 'Verify'){Assert-TsfStandardUser}
  if(Test-Path -LiteralPath $work){Assert-TsfNoReparsePoint $work}
  if($Mode -eq 'NewCertificate'){
    if($state -or (Test-Path -LiteralPath $cer)){throw 'Refusing to replace existing local certificate/state'}
    $certificate=$null
    try {
      $certificate=New-TsfPersonalCertificate
      $state=@{Schema=1;Identity=(Get-TsfLocalIdentity);Thumbprint=$certificate.Thumbprint;CertificateOwned=$true;
        NotBefore=$certificate.NotBefore.ToUniversalTime().ToString('o');NotAfter=$certificate.NotAfter.ToUniversalTime().ToString('o');
        Key=(Get-TsfKeyMetadata $certificate);Trust=@{};SignedHashes=@{};Phase='CertificateCreated'}
      Assert-TsfCertificate $certificate $state -PrivateKey
      Save-TsfSigningState $state $statePath
      Export-TsfPublicCertificate $certificate $cer
    } catch {
      if($certificate){try{Remove-TsfPersonalCertificate $certificate.Thumbprint $state.Key;if(Test-Path $cer){Remove-Item -LiteralPath $cer};if(Test-Path $statePath){Remove-Item -LiteralPath $statePath}}catch{throw "Certificate creation failed; rollback incomplete; retain local state: $_"}}
      throw
    }
    return [pscustomobject]@{Action=$Mode;DryRun=$false;Thumbprint=$state.Thumbprint;NotBefore=$state.NotBefore;NotAfter=$state.NotAfter;Store='CurrentUser/My';Key=$state.Key;Trusted=$false}
  }
  if(-not $state){throw 'Local certificate state required'}
  if($Mode -in @('Trust','Revoke')){
    if($Mode -eq 'Trust' -and -not $ApproveTrust){throw 'Trust requires -ApproveTrust and exact -Thumbprint after user confirmation'}
    if($Mode -eq 'Revoke' -and -not $ApproveRevoke){throw 'Revoke requires -ApproveRevoke and exact -Thumbprint after user confirmation'}
    if($Thumbprint -cne $state.Thumbprint){throw 'Confirmed thumbprint does not match local state'}
  }
  if($Mode -notin @('Revoke','Verify')){$certificate=Get-TsfCertificate $state.Thumbprint;Assert-TsfCertificate $certificate $state -PrivateKey}
  if($Mode -eq 'Sign'){
    if(-not $PackageRoot -or $SourceCommit -notmatch '^[0-9a-f]{40}$'){throw 'Audited PackageRoot and source commit required'}
    $source=(Resolve-Path -LiteralPath $PackageRoot).Path
    if($work.StartsWith($source.TrimEnd('\','/')+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase) -or $source.StartsWith($work.TrimEnd('\','/')+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase) -or $work -eq $source){throw 'Signing work directory and unsigned package must not overlap'}
    Assert-TsfNoReparsePoint $source
    Invoke-TsfPackageAudit $source
    $provenance=Get-Content -LiteralPath (Join-Path $source 'BUILD-PROVENANCE.json') -Raw|ConvertFrom-Json
    if($provenance.sourceCommit -cne $SourceCommit){throw 'Package source commit mismatch'}
    Assert-TsfSignTool $SignToolPath
    if(Test-Path -LiteralPath $output){throw 'Refusing to overwrite signed output; revoke or use a new work directory'}
    try {
      New-Item -ItemType Directory -Path $output|Out-Null
      Get-ChildItem -LiteralPath $source -Force|Copy-Item -Destination $output -Recurse
      $state.Payload=@(Get-ChildItem -LiteralPath $source -Recurse -File -Force|ForEach-Object {@{path=[IO.Path]::GetRelativePath($source,$_.FullName).Replace('\','/');sha256=(Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash}})
      $state.SourceCommit=$SourceCommit;$state.SourceRun=$provenance.runId;$state.SignedHashes=@{}
      foreach($name in $TsfSigningBinaries){
        $path=Join-Path $output $name
        $existing=Get-TsfFileSignature $path
        if($existing.Status -eq 'Valid' -and $existing.SignerCertificate.Thumbprint -cne $state.Thumbprint){throw "Refusing to replace pre-existing trusted signer: $name"}
        $null=Invoke-TsfSignTool $SignToolPath @('sign','/sha1',$state.Thumbprint,'/s','My','/fd','SHA256',$path)
        $signature=Get-TsfFileSignature $path
        if($signature.Status -notin @('Valid','NotTrusted','UnknownError') -or $signature.SignerCertificate.Thumbprint -cne $state.Thumbprint){throw "Signature or signer mismatch: $name"}
        $state.SignedHashes[$name]=(Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash
      }
      Assert-TsfSignedPayload $state $output
      $state.Phase='SignedPendingTrust';Save-TsfSigningState $state $statePath
    } catch {Remove-TsfSigningOutput $output $work;throw}
    return [pscustomobject]@{Action=$Mode;DryRun=$false;Thumbprint=$state.Thumbprint;SourceCommit=$SourceCommit;SignedOutput=$output;Verified=$false;Status='Trust and full SignTool verification still required'}
  }
  if($Mode -eq 'Verify'){
    $public=Read-TsfPublicCertificate $cer
    try{Assert-TsfCertificate $public $state}finally{$public.Dispose()}
    Assert-TsfSignTool $SignToolPath;Assert-TsfSignedPayload $state $output
    foreach($name in $TsfSigningBinaries){
      $path=Join-Path $output $name;$sig=Get-TsfFileSignature $path
      if($sig.Status -ne 'Valid' -or $sig.SignerCertificate.Thumbprint -cne $state.Thumbprint){throw "Valid trusted signature required: $name ($($sig.Status))"}
      if((Invoke-TsfSignTool $SignToolPath @('verify','/pa','/v',$path)) -ne 0){throw "SignTool verification failed: $name"}
    }
    return [pscustomobject]@{Action=$Mode;DryRun=$false;Verified=$true;Thumbprint=$state.Thumbprint;SourceCommit=$state.SourceCommit;HostAcceptance='NOT TESTED'}
  }
  if($Mode -eq 'Trust'){
    Assert-TsfSignedPayload $state $output
    $public=Read-TsfPublicCertificate $cer
    try{Assert-TsfCertificate $public $state;if($public.HasPrivateKey){throw 'Public CER unexpectedly contains a private key'}}finally{$public.Dispose()}
    $added=[Collections.Generic.List[string]]::new()
    try {
      foreach($store in @('Root','TrustedPublisher')){
        if(Test-TsfTrustCopy $store $state.Thumbprint){
          if(-not $state.Trust.ContainsKey($store)){$state.Trust[$store]=@{Added=$false;PreExisting=$true}}
          continue
        }
        $state.Trust[$store]=@{Added=$true;PreExisting=$false};Save-TsfSigningState $state $statePath
        $added.Add($store)
        Add-TsfTrustCopy $store $cer
        if(-not(Test-TsfTrustCopy $store $state.Thumbprint)){throw "Trust import did not persist: $store"}
      }
      $state.Phase='TrustImported';Save-TsfSigningState $state $statePath
    } catch {
      $failure=$_
      foreach($store in $added){if(Test-TsfTrustCopy $store $state.Thumbprint){Remove-TsfTrustCopy $store $state.Thumbprint};$state.Trust[$store].Added=$false}
      Save-TsfSigningState $state $statePath;throw $failure
    }
    return [pscustomobject]@{Action=$Mode;DryRun=$false;Thumbprint=$state.Thumbprint;Trust=$state.Trust;Registration='Not performed; separate confirmation required'}
  }
  if($Mode -eq 'Revoke'){
    if(-not $PackageRoot){throw 'Matching audited PackageRoot required for status/removal'}
    Invoke-TsfPackageAudit $PackageRoot
    $provenance=Get-Content -LiteralPath (Join-Path $PackageRoot 'BUILD-PROVENANCE.json') -Raw|ConvertFrom-Json
    if($state.SourceCommit -and $provenance.sourceCommit -cne $state.SourceCommit){throw 'Removal package source commit mismatch'}
    if($state.CertificateOwned){
      $certificate=Get-TsfPersonalCertificateOrNull $state.Thumbprint
      if($certificate){
        Assert-TsfCertificate $certificate $state -AllowExpired
        if($certificate.HasPrivateKey){Assert-TsfCertificate $certificate $state -PrivateKey -AllowExpired}
      }
      if(-not $state.Key.Name -or $state.Key.Provider -ne 'Microsoft Software Key Storage Provider'){throw 'Owned key cleanup metadata missing'}
    }
    $status=Get-TsfRegistrationStatus $PackageRoot
    if($status -ne 0){
      if(-not $Unregister){throw 'TSF remains registered; confirm -Unregister before certificate removal'}
      Invoke-TsfUnregister $PackageRoot
      if((Get-TsfRegistrationStatus $PackageRoot) -ne 0){throw 'Unregister incomplete; retain trust/key/state'}
    }
    foreach($store in @('TrustedPublisher','Root')){
      if($state.Trust.ContainsKey($store) -and $state.Trust[$store].Added){
        if(Test-TsfTrustCopy $store $state.Thumbprint){Remove-TsfTrustCopy $store $state.Thumbprint}
        $state.Trust[$store].Added=$false;Save-TsfSigningState $state $statePath
      }
    }
    if($state.CertificateOwned){
      Remove-TsfPersonalCertificate $state.Thumbprint $state.Key;$state.CertificateOwned=$false
    }
    $state.Phase='Revoked';Save-TsfSigningState $state $statePath
    if($DeleteOutputs){
      $sealedZip=Join-Path $work 'signed-package-signed.zip'
      if(Test-Path -LiteralPath $sealedZip){
        if(-not $state.SealedZipHash -or (Get-FileHash -LiteralPath $sealedZip -Algorithm SHA256).Hash -ne $state.SealedZipHash){throw 'Sealed archive is unowned or changed; preserve for manual review'}
        Remove-Item -LiteralPath $sealedZip
      }
      Remove-TsfSigningOutput $output $work;if(Test-Path $cer){Remove-Item -LiteralPath $cer}
    }
    return [pscustomobject]@{Action=$Mode;DryRun=$false;Revoked=$true;StatePath=$statePath;Preserved='Pre-existing trust, other certificates, unrelated user data and TSF cache'}
  }
}
