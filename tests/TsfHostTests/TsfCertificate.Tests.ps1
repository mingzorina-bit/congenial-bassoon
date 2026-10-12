$ErrorActionPreference='Stop'
$repo=Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
. (Join-Path $repo 'scripts/TsfSigning.Common.ps1')
function Check($v,$m){if(-not $v){throw $m}}
function MakeCertificate([string]$Eku,[datetimeoffset]$End,[string]$Hash='SHA256'){
  $rsa=[Security.Cryptography.RSA]::Create(2048)
  $request=[Security.Cryptography.X509Certificates.CertificateRequest]::new('CN=BilingualInput Phase1A Local Trial',$rsa,[Security.Cryptography.HashAlgorithmName]::new($Hash),[Security.Cryptography.RSASignaturePadding]::Pkcs1)
  $oids=[Security.Cryptography.OidCollection]::new();$oids.Add([Security.Cryptography.Oid]::new($Eku))|Out-Null
  $request.CertificateExtensions.Add([Security.Cryptography.X509Certificates.X509EnhancedKeyUsageExtension]::new($oids,$true))
  $request.CertificateExtensions.Add([Security.Cryptography.X509Certificates.X509KeyUsageExtension]::new([Security.Cryptography.X509Certificates.X509KeyUsageFlags]::DigitalSignature,$true))
  try{return $request.CreateSelfSigned([datetimeoffset]::Now.AddDays(-1),$End)}finally{$rsa.Dispose()}
}
function RejectCertificate($cert,$pattern){$blocked=$false;try{Assert-TsfCertificate $cert @{Thumbprint=$cert.Thumbprint}}catch{$blocked=$_.Exception.Message -like $pattern};Check $blocked "Certificate accepted: $pattern"}
$cert=MakeCertificate '1.3.6.1.5.5.7.3.3' ([datetimeoffset]::Now.AddDays(29))
$wrong=MakeCertificate '1.3.6.1.5.5.7.3.1' ([datetimeoffset]::Now.AddDays(29))
$expired=MakeCertificate '1.3.6.1.5.5.7.3.3' ([datetimeoffset]::Now.AddHours(-1))
$weak=MakeCertificate '1.3.6.1.5.5.7.3.3' ([datetimeoffset]::Now.AddDays(29)) 'SHA384'
try {
  Assert-TsfCertificate $cert @{Thumbprint=$cert.Thumbprint}
  RejectCertificate $wrong '*EKU*'
  RejectCertificate $expired '*validity*'
  Assert-TsfCertificate $expired @{Thumbprint=$expired.Thumbprint} -AllowExpired
  RejectCertificate $weak '*SHA-256*'
  Write-Output 'PASS real in-memory certificate EKU, expiry, cleanup allowance and SHA-256 policy; no store writes'
}finally{$cert.Dispose();$wrong.Dispose();$expired.Dispose();$weak.Dispose()}
