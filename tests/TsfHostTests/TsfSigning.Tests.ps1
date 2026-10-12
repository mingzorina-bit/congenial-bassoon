$ErrorActionPreference='Stop'
$repo=Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$common=Join-Path $repo 'scripts/TsfSigning.Common.ps1'
if(-not(Test-Path -LiteralPath $common)){ throw 'RED: local signing implementation missing' }
. $common
$testRoot=Join-Path $repo ('artifacts/bilingual-signing-'+[guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $testRoot -Force | Out-Null
$script:total=0
function Check($value,$message){if(-not $value){throw $message}}
function Reject([scriptblock]$action,[string]$pattern){$blocked=$false;try{& $action|Out-Null}catch{$blocked=$_.Exception.Message -like $pattern};Check $blocked "Expected rejection: $pattern"}
function Case($name,[scriptblock]$action){& $action;$script:total++;Write-Output "PASS $name"}
function Get-TsfLocalIdentity { @{Machine='fixture';UserSid='S-1-5-21-fixture';MachineId='fixture-id'} }
function Assert-TsfStandardUser {}
function Assert-TsfCertificate { param($Certificate,$State,[switch]$PrivateKey,[switch]$AllowExpired) if($Certificate.Thumbprint -ne $State.Thumbprint){throw 'Certificate thumbprint mismatch'} }
function Get-TsfCertificate { param($Thumbprint) [pscustomobject]@{Thumbprint=$Thumbprint;Subject='CN=BilingualInput Phase1A Local Trial';NotBefore=(Get-Date).AddDays(-1);NotAfter=(Get-Date).AddDays(29)} }
function Get-TsfPersonalCertificateOrNull { param($Thumbprint) Get-TsfCertificate $Thumbprint }
function Read-TsfPublicCertificate { param($Path) $c=[pscustomobject]@{Thumbprint=$script:thumb;HasPrivateKey=$false};$c|Add-Member -MemberType ScriptMethod -Name Dispose -Value {};return $c }
function Get-TsfKeyMetadata { param($Certificate) @{Name='test-key';Provider='Microsoft Software Key Storage Provider';ExportPolicy='None';MachineKey=$false} }
function New-TsfPersonalCertificate { Get-TsfCertificate $script:thumb }
function Export-TsfPublicCertificate { param($Certificate,$Path) throw 'public export refused' }
function Assert-TsfSignTool { param($Path) }
function Test-TsfTrustCopy { param($Store,$Thumbprint) $script:stores.Contains($Store) }
function Add-TsfTrustCopy { param($Store,$CertificateFile) if($Store -eq $script:failImport){throw 'import refused'};$script:stores.Add($Store)|Out-Null }
function Remove-TsfTrustCopy { param($Store,$Thumbprint) $script:stores.Remove($Store)|Out-Null }
function Remove-TsfPersonalCertificate { param($Thumbprint) $script:deletedKey=$true }
function Get-TsfRegistrationStatus { param($PackageRoot) $script:registrationStatus }
function Invoke-TsfUnregister { param($PackageRoot) throw 'unregister refused' }
function Invoke-TsfPackageAudit { param($Root) if(-not(Test-Path (Join-Path $Root 'FILES.sha256.json'))){throw 'inventory missing'} }
function Get-TsfFileSignature { param($Path) [pscustomobject]@{Status=$script:signatureStatus;SignerCertificate=[pscustomobject]@{Thumbprint=$script:thumb};StatusMessage='fixture'} }
function Invoke-TsfSignTool { param($Path,$Arguments) if($Arguments[0] -eq 'sign'){ if($script:failSign){throw 'signing refused'};Add-Content -LiteralPath $Arguments[-1] -Value 'signature';return 0 };return $script:verifyExit }
$script:thumb='1111111111111111111111111111111111111111'
$script:stores=[Collections.Generic.HashSet[string]]::new()
$script:signatureStatus='NotTrusted';$script:verifyExit=1;$script:failSign=$false;$script:failImport='';$script:registrationStatus=0;$script:deletedKey=$false
$source=Join-Path $testRoot 'source';New-Item -ItemType Directory $source|Out-Null
foreach($name in @('BilingualTsf.dll','rime.dll','TsfTrialRegistration.exe','other.txt')){Set-Content (Join-Path $source $name) 'unsigned'}
$commit='aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa'
@{sourceCommit=$commit;runId='123'}|ConvertTo-Json|Set-Content (Join-Path $source 'BUILD-PROVENANCE.json')
$inventory=Get-ChildItem $source -File|ForEach-Object{@{path=$_.Name;sha256=(Get-FileHash $_.FullName).Hash}}
$inventory|ConvertTo-Json|Set-Content (Join-Path $source 'FILES.sha256.json')
$work=Join-Path $testRoot 'local';$statePath=Join-Path $work 'state.json'
function FreshState {
  New-Item -ItemType Directory -Force $work|Out-Null
  $state=@{Schema=1;Identity=(Get-TsfLocalIdentity);Thumbprint=$script:thumb;CertificateOwned=$true;Trust=@{};SignedHashes=@{};Key=@{Name='test-key';Provider='Microsoft Software Key Storage Provider'};Phase='CertificateCreated'}
  Save-TsfSigningState $state $statePath
  Set-Content (Join-Path $work 'public.cer') 'public fixture'
}
try {
  Case 'Every mode defaults to preview without state or certificate writes' {
    foreach($mode in @('NewCertificate','Sign','Verify','Trust','Revoke')) {
      $p=Invoke-TsfSigningOperation -Mode $mode -StatePath $statePath -PackageRoot $source -SourceCommit $commit
      Check $p.DryRun "$mode changed default";Check (-not(Test-Path $statePath)) 'Preview wrote state'
    }
  }
  Case 'Public export failure removes newly created personal certificate and key' {
    $script:deletedKey=$false
    Reject {Invoke-TsfSigningOperation -Mode NewCertificate -StatePath $statePath -Apply} '*public export refused*'
    Check $script:deletedKey 'Creation rollback omitted owned key';Check (-not(Test-Path $statePath)) 'Failed certificate state left usable'
  }
  FreshState
  Case 'Wrong machine is rejected before trust' {
    $s=Read-TsfSigningState $statePath;$s.Identity.Machine='other';Save-TsfSigningState $s $statePath
    Reject {Invoke-TsfSigningOperation -Mode Trust -StatePath $statePath -Apply -ApproveTrust -Thumbprint $script:thumb} '*machine/account*'
    Check ($script:stores.Count -eq 0) 'Wrong machine imported trust';FreshState
  }
  Case 'Trust needs explicit thumbprint confirmation' {
    Reject {Invoke-TsfSigningOperation -Mode Trust -StatePath $statePath -Apply} '*ApproveTrust*'
    Reject {Invoke-TsfSigningOperation -Mode Trust -StatePath $statePath -Apply -ApproveTrust -Thumbprint ('2'*40)} '*thumbprint*'
  }
  Case 'Wrong source commit rejected without signing output' {
    Reject {Invoke-TsfSigningOperation -Mode Sign -StatePath $statePath -PackageRoot $source -SourceCommit ('b'*40) -SignToolPath 'fixture.exe' -Apply} '*source commit*'
    Check (-not(Test-Path (Join-Path $work 'signed-package'))) 'Wrong commit copied output'
  }
  Case 'Signing failure leaves original unchanged and removes partial output' {
    $before=(Get-FileHash (Join-Path $source 'BilingualTsf.dll')).Hash;$script:failSign=$true
    Reject {Invoke-TsfSigningOperation -Mode Sign -StatePath $statePath -PackageRoot $source -SourceCommit $commit -SignToolPath 'fixture.exe' -Apply} '*signing refused*'
    Check ((Get-FileHash (Join-Path $source 'BilingualTsf.dll')).Hash -eq $before) 'Unsigned source changed'
    Check (-not(Test-Path (Join-Path $work 'signed-package'))) 'Partial output remained';$script:failSign=$false
  }
  Case 'Untrusted signatures produce pending output and cannot pass verification' {
    Invoke-TsfSigningOperation -Mode Sign -StatePath $statePath -PackageRoot $source -SourceCommit $commit -SignToolPath 'fixture.exe' -Apply|Out-Null
    Reject {Invoke-TsfSigningOperation -Mode Verify -StatePath $statePath -SignToolPath 'fixture.exe' -Apply} '*trusted signature*'
    $script:signatureStatus='UnknownError'
    Reject {Invoke-TsfSigningOperation -Mode Verify -StatePath $statePath -SignToolPath 'fixture.exe' -Apply} '*trusted signature*'
    $script:signatureStatus='Valid';$script:verifyExit=0
    $p=Invoke-TsfSigningOperation -Mode Verify -StatePath $statePath -SignToolPath 'fixture.exe' -Apply;Check $p.Verified 'Valid verification failed'
  }
  Case 'Changed payload cannot be verified by valid signatures alone' {
    Add-Content (Join-Path $work 'signed-package/other.txt') 'tampered'
    Reject {Invoke-TsfSigningOperation -Mode Verify -StatePath $statePath -SignToolPath 'fixture.exe' -Apply} '*payload*'
    Copy-Item (Join-Path $source 'other.txt') (Join-Path $work 'signed-package/other.txt') -Force
  }
  Case 'Second trust import failure rolls back only newly added Root' {
    $script:failImport='TrustedPublisher'
    Reject {Invoke-TsfSigningOperation -Mode Trust -StatePath $statePath -Thumbprint $script:thumb -Apply -ApproveTrust} '*import refused*'
    Check ($script:stores.Count -eq 0) 'Root left trusted after failure'
    $script:stores.Add('Root')|Out-Null
    Reject {Invoke-TsfSigningOperation -Mode Trust -StatePath $statePath -Thumbprint $script:thumb -Apply -ApproveTrust} '*import refused*'
    Check ($script:stores.Contains('Root')) 'Pre-existing Root was removed';$script:failImport=''
  }
  Case 'Unregister failure retains trust and private key for recovery' {
    $script:registrationStatus=6;$script:deletedKey=$false
    Reject {Invoke-TsfSigningOperation -Mode Revoke -StatePath $statePath -PackageRoot $source -Thumbprint $script:thumb -Apply -ApproveRevoke -Unregister} '*unregister refused*'
    Check (-not $script:deletedKey) 'Deleted key after unregister failure';Check ($script:stores.Contains('Root')) 'Removed trust after failed unregister';$script:registrationStatus=0
  }
  Case 'Successful revoke preserves pre-existing trust and deletes owned private key' {
    Invoke-TsfSigningOperation -Mode Revoke -StatePath $statePath -PackageRoot $source -Thumbprint $script:thumb -Apply -ApproveRevoke|Out-Null
    Check $script:deletedKey 'Owned key was not deleted';Check ($script:stores.Contains('Root')) 'Pre-existing trust lost'
  }
  Case 'Explicit output cleanup removes only recorded signed archive and copy' {
    $zip=Join-Path $work 'signed-package-signed.zip';Set-Content $zip 'owned ZIP'
    $s=Read-TsfSigningState $statePath;$s.SealedZipHash=(Get-FileHash $zip).Hash;Save-TsfSigningState $s $statePath
    Invoke-TsfSigningOperation -Mode Revoke -StatePath $statePath -PackageRoot $source -Thumbprint $script:thumb -Apply -ApproveRevoke -DeleteOutputs|Out-Null
    Check (-not(Test-Path $zip) -and -not(Test-Path (Join-Path $work 'signed-package'))) 'Owned signed output remained'
    Check (Test-Path (Join-Path $source 'BilingualTsf.dll')) 'Original input was removed'
  }
  Write-Output "$script:total signing safety tests passed; no real certificate store changes"
} finally {
  $absolute=[IO.Path]::GetFullPath($testRoot);$parent=[IO.Path]::GetFullPath((Join-Path $repo 'artifacts'))
  if($absolute.StartsWith($parent,[StringComparison]::OrdinalIgnoreCase) -and (Split-Path $absolute -Leaf) -like 'bilingual-signing-*'){Remove-Item -LiteralPath $absolute -Recurse -Force}
}
