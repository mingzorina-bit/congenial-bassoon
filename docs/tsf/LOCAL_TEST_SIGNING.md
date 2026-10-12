# Phase 1-A: one-machine development signing proposal

Status: prepared for review only. Do not run the trust, registration, or activation steps until the user approves them. This is a free, local development path, not a commercial distribution signature. The target is this user's x64 Windows 11 25H2 (build 26200) machine; actual TSF loading is still an experiment.

Microsoft requires third-party IMEs to be digitally signed. PowerShell can create a code-signing test certificate and sign files, but a self-signed certificate is untrusted by default. Importing it into `CurrentUser\Root` and `CurrentUser\TrustedPublisher` is a meaningful trust change. Authenticode `Valid` is necessary for our registration guard; it does **not** prove Windows will load this TSF DLL in every editor or bypass every Windows policy.

## Before any system change

1. Download the Windows CI TSF ZIP for the exact PR commit. Compare the ZIP SHA-256 with GitHub's artifact digest, extract to a new local-only directory, run `scripts/Audit-Sources.ps1 -PackagePath <dir> -TsfTrial`, and keep the original ZIP.
2. Run `scripts/Register-TsfTrial.ps1 -PackageRoot <dir>` and `scripts/Unregister-TsfTrial.ps1 -PackageRoot <dir>` **without** `-Apply` to inspect fixed CLSID, profile and registry scope. Record the exact extracted directory. Do not alter or sign the original CI ZIP.
3. Check the three file signatures first: `BilingualTsf.dll`, `rime.dll`, and `TsfTrialRegistration.exe`. If any already have a trusted valid signature, preserve it. Make a separate working copy for signing.

## Proposed steps after explicit approval

In Windows PowerShell as the target user, set `$trialRoot` to the absolute path of the copied package. No administrator privilege, private-key export, `bcdedit`, Test Mode, Defender exemption, machine-wide certificate store or commercial certificate is proposed.

```powershell
$cert = New-SelfSignedCertificate -Type CodeSigningCert `
  -Subject 'CN=BilingualInput Phase1A Local Trial' `
  -CertStoreLocation 'Cert:\CurrentUser\My' `
  -KeyExportPolicy NonExportable -HashAlgorithm SHA256 `
  -NotAfter (Get-Date).AddDays(30)
$thumbprint = $cert.Thumbprint
$certificateFile = "$trialRoot-LocalTest.cer" # outside the sealed package
Export-Certificate -Cert $cert -FilePath $certificateFile

foreach ($name in @('BilingualTsf.dll','rime.dll','TsfTrialRegistration.exe')) {
  $path = Join-Path $trialRoot $name
  $existing = Get-AuthenticodeSignature -LiteralPath $path
  if ($existing.Status -ne 'Valid') {
    $signed = Set-AuthenticodeSignature -LiteralPath $path -Certificate $cert -HashAlgorithm SHA256
    if ($signed.Status -eq 'HashMismatch' -or $signed.Status -eq 'NotSigned') { throw "Signing failed: $name" }
  }
}
```

The public `.cer` and thumbprint stay local; **never** upload the private key or a PFX. The command is intentionally separate from trusting the certificate. Before import, inspect the exported certificate's subject, thumbprint, validity and `Code Signing` EKU. Import *only this exact certificate* for this user:

```powershell
Import-Certificate -FilePath $certificateFile `
  -CertStoreLocation 'Cert:\CurrentUser\Root'
Import-Certificate -FilePath $certificateFile `
  -CertStoreLocation 'Cert:\CurrentUser\TrustedPublisher'
foreach ($name in @('BilingualTsf.dll','rime.dll','TsfTrialRegistration.exe')) {
  $signature = Get-AuthenticodeSignature -LiteralPath (Join-Path $trialRoot $name)
  if ($signature.Status -ne 'Valid') { throw "Untrusted binary: $name ($($signature.Status))" }
}
```

Compare each signature's signer thumbprint with `$thumbprint` for files newly signed by this certificate; pre-existing trusted signatures must match their original signers. Re-run package audit, then `scripts/Seal-SignedTsfTrial.ps1 -PackageRoot $trialRoot`. **Do not** run `Register-TsfTrial.ps1 -Apply` or activate the profile until the separate registration/activation confirmation. `Seal-SignedTsfTrial.ps1` adds a hash inventory and makes a new ZIP; the `.cer` remains outside the package so the allowlist stays strict.

## Complete local rollback, after any actual trial

Switch to a different IME, close editors that loaded the trial DLL, then run `scripts/Unregister-TsfTrial.ps1 -PackageRoot $trialRoot -Apply`. Confirm its exit is successful, the fixed HKCU COM owner marker is gone, and the trial language profile/category are absent. If removal fails, **retain** the binaries and certificates, retry removal or investigate; the helper deliberately preserves its marker on TSF removal failure. Only after successful unregister, remove the exact certificate thumbprint from `Cert:\CurrentUser\TrustedPublisher`, `Cert:\CurrentUser\Root`, then `Cert:\CurrentUser\My`:

```powershell
foreach ($store in @('TrustedPublisher','Root','My')) {
  $path = "Cert:\CurrentUser\$store\$thumbprint"
  $item = Get-Item -LiteralPath $path -ErrorAction SilentlyContinue
  if ($item) {
    if ($item.Subject -ne 'CN=BilingualInput Phase1A Local Trial') { throw "Unexpected certificate: $path" }
    Remove-Item -LiteralPath $path
  }
  if (Test-Path -LiteralPath $path) { throw "Certificate still trusted: $path" }
}
```

Delete the extracted working copy and local `.cer` last. If `rime.dll` carried a pre-existing trusted signature, do not remove that publisher's certificate.

The current PR has no evidence of actual Notepad, second-editor, password-field, focus-switch, or uninstall behavior. Run those manual checks only after signature, trust, registration and activation are separately approved.

Sources: [Microsoft IME requirements](https://learn.microsoft.com/en-us/windows/apps/develop/input/input-method-editors), [New-SelfSignedCertificate](https://learn.microsoft.com/en-us/powershell/module/pki/new-selfsignedcertificate), [Set-AuthenticodeSignature](https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.security/set-authenticodesignature), [Import-Certificate](https://learn.microsoft.com/en-us/powershell/module/pki/import-certificate).
