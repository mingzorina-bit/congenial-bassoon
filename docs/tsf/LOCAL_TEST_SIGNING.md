# Phase 1-A free local development signing

Status: preparation scripts and failure tests. No personal certificate, trusted certificate, TSF registration or activation was created by this task. An actual thumbprint does not exist yet. Target: the designated user's x64 Windows 11 25H2 (build 26200) test machine, subject to its actual security policy. Signature/CI success never establishes external-editor acceptance.

## Reconciliation and package identity

The supplied local `signing-preparation/FREE_LOCAL_TEST_SIGNING_PLAN.md` and earlier repository proposal describe the same phased operation. Retain the repository's **30-day** lifetime (local plan suggested 90), use the existing Microsoft-signed Windows SDK **SignTool** for signing/verification, and use certificate-provider `DeleteKey` plus exact dedicated CNG-key existence checks for cleanup. No SDK download or paid certificate is needed on the current development machine.

CI **38268798738** built `aecad960cbe1afc6abf2d5da060eaddaf10f2a2b`; artifact **11755622047** from run **38267091679** built `f146af645b3cffefe822745cf9c1761068d32e30`. GitHub compare shows only progress and release documents changed. TSF source/tests/packaging were identical, so the older review package contains all repairs. This milestone adds signing scripts, package provenance and read-only status; use its new CI package for the new workflow. `BUILD-PROVENANCE.json` records PR source head, actual checkout/merge commit, run ID and x64 architecture.

## Operation boundaries

`scripts/Local-TsfSigning.ps1` is the entry point. Every mode defaults to a **read-only preview**; `-Apply` acts only on that mode. Trust additionally requires `-ApproveTrust -Thumbprint <exact uppercase fingerprint>`; Revoke requires `-ApproveRevoke -Thumbprint <exact fingerprint>`. Switches do not replace the human confirmation required by this task.

Reuse Audit, Seal, Register and Unregister scripts. Seal now defaults to preview and requires `-Apply`. Helper `status` reads registration: exit 0 means fixed COM/profile/category absent; 6 means residue present; 3 means status unknown. Unknown status blocks certificate removal. Register retains its three-`Valid` signature guard. No signing mode registers or activates an IME.

Run PowerShell **7**, **unelevated**, as the designated user. No LocalMachine store, automatic elevation, TESTSIGNING, security-feature changes or policy bypass is offered. If user-scoped trust is insufficient, stop and record the policy error; widening scope needs another proposal and confirmation.

## Certificate and private key

Dedicated subject: `CN=BilingualInput Phase1A Local Trial`; RSA 2048; SHA-256; Code Signing EKU `1.3.6.1.5.5.7.3.3`; DigitalSignature; 30-day validity. The non-exportable user key belongs to **Microsoft Software Key Storage Provider**, **CurrentUser/My**. Local `state.json` records exact thumbprint, UTC validity, dedicated key name/provider, user/machine, source commit and signed hashes. The private key stays in Windows user key storage: no PFX or password. Export only `public.cer`.

Keep state/CER and outputs local, outside the unsigned extraction. `.gitignore` excludes signing-local/PFX/P12/CER/key files. Upload only named source/test/docs; never local state, signed test outputs or key material. State binding prevents accidental cross-machine/account use; a public CER cannot mathematically prevent independent import elsewhere.

## Preview now; approved execution later

`$packageRoot` is the fresh audited extraction, `$sourceCommit` its verified source head, and `$tool` a trusted official x64 SignTool. The current discovered tool is SDK `10.0.26100.0` from pinned Microsoft.Windows.SDK.BuildTools and has a valid Microsoft signature.

```powershell
$localState = Join-Path $PWD 'signing-local/state.json' # outside $packageRoot
$packageRoot = '<absolute audited package directory>'
$sourceCommit = '<verified 40-character source commit>'
$tool = '<absolute Microsoft-signed SDK x64 signtool.exe>'
./scripts/Local-TsfSigning.ps1 -Mode NewCertificate -StatePath $localState
./scripts/Local-TsfSigning.ps1 -Mode Sign -StatePath $localState -PackageRoot $packageRoot -SourceCommit $sourceCommit -SignToolPath $tool
./scripts/Local-TsfSigning.ps1 -Mode Verify -StatePath $localState -SignToolPath $tool
./scripts/Local-TsfSigning.ps1 -Mode Trust -StatePath $localState
./scripts/Local-TsfSigning.ps1 -Mode Revoke -StatePath $localState -PackageRoot $packageRoot
```

After confirmation for personal-certificate creation/signing, append `-Apply` to NewCertificate, then Sign. Creation failure rolls back the new certificate/key; signing failure removes partial output, leaving the audited original unchanged. SignTool uses `/sha1 <thumbprint> /s My /fd SHA256` (SHA1 identifies the certificate; file digest is SHA256). Only the three core binaries in a fresh `signed-package` are signed; trusted different signers and existing output directories are protected. Microsoft runtime DLLs remain unchanged.

State records original payload and three signed hashes. Before trust, `NotTrusted`/`UnknownError` are pending diagnostics, never PASS; UnknownError is not assumed to mean only chain distrust. Verify, Seal and Register cannot pass until signatures are **Valid**, signers match the selected thumbprint, and `SignTool verify /pa /v` returns zero for all three files.

## Separate trust and installation confirmations

After creation/signing, show actual thumbprint, UTC validity, public CER path/hash, three signed hashes, local machine/account, exact Root/TrustedPublisher changes and inverse preview. Only after those operations are approved:

```powershell
$fingerprint = '<actual uppercase fingerprint from reviewed local state>'
./scripts/Local-TsfSigning.ps1 -Mode Trust -StatePath $localState -Thumbprint $fingerprint -ApproveTrust -Apply
./scripts/Local-TsfSigning.ps1 -Mode Verify -StatePath $localState -SignToolPath $tool -Apply # read-only
./scripts/Seal-SignedTsfTrial.ps1 -PackageRoot '<signed-package path>' -StatePath $localState -SignToolPath $tool # preview
./scripts/Seal-SignedTsfTrial.ps1 -PackageRoot '<signed-package path>' -StatePath $localState -SignToolPath $tool -Apply
```

Trust affects only **CurrentUser/Root** and **CurrentUser/TrustedPublisher**. Journal each intention before import; preserve pre-existing copies. A second-import failure rolls back only this attempt's new copies; rollback failure retains the journal. Seal and Register require `-StatePath $localState -SignToolPath $tool` and repeat payload, signer and SignTool verification immediately before use. Failed Seal restores the old inventory and preserves any existing ZIP. Record sealed hash/source commit, show registration/removal previews, then obtain **separate registration/activation approval**.

## Full revocation

Switch back to the prior IME and close editors that loaded the DLL. Use a matching audited package; the unsigned original is acceptable for read-only status, while an installed package should already be sealed.

```powershell
./scripts/Local-TsfSigning.ps1 -Mode Revoke -StatePath $localState -PackageRoot $packageRoot
# After revocation confirmation; -Unregister is needed only if TSF was installed:
./scripts/Local-TsfSigning.ps1 -Mode Revoke -StatePath $localState -PackageRoot $packageRoot -Thumbprint $fingerprint -ApproveRevoke -Unregister -Apply
```

Verify matching provenance and registration status, reuse Unregister if explicitly requested, and establish COM/profile/category absence **before removing trust**. Failure retains trust/key/state/binaries. Remove only journaled new trust by exact fingerprint, delete owned My certificate with `Remove-Item -DeleteKey`, check the recorded user-scoped CNG key and, if necessary, delete that exact remaining dedicated key through CNG. Never delete arbitrary key files or match certificates by name. Expired owned certificates remain revocable. Keep the local journal; repeated completed revoke is safe.

`-DeleteOutputs` also removes only the validated `signed-package` child, public CER and sealed ZIP whose recorded hash still matches. Unknown/changed ZIPs are preserved for review. Original ZIPs, other certificates/data and `%LOCALAPPDATA%/BilingualInput/tsf-rime` remain; optional cache deletion needs a separate reviewed scope. Retain removal binaries/journal until restoration is verified.

## Manual acceptance conditions

Notepad plus a second named editor: pinyin, visible highlight/actual commit, fifth option, Space/Enter/Esc, English separator. Native/browser password, PIN and private fields: no candidates/retained composition. Mid-composition focus switches and editor closure; if a host rejects sync and deferred cleanup and stale text survives, record FAIL. Unregister/reopen: original keyboard works, trial COM/profile/category absent, owned trust/private key absent. CI/signing replaces none of these checks.

Sources: [IME requirements](https://learn.microsoft.com/en-us/windows/apps/develop/input/input-method-editors), [self-signed testing](https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/code-signing-options#self-signed-certificates--dev-and-testing-only), [New-SelfSignedCertificate](https://learn.microsoft.com/en-us/powershell/module/pki/new-selfsignedcertificate), [SignTool](https://learn.microsoft.com/en-us/windows/win32/seccrypto/signtool), [certificate provider/DeleteKey](https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.security/about/about_certificate_provider), [Import-Certificate](https://learn.microsoft.com/en-us/powershell/module/pki/import-certificate).
