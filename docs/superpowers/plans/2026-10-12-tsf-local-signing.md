# TSF local signing preparation implementation plan

> **For agentic workers:** Use superpowers:executing-plans inline, then one independent whole-change review.

**Goal:** Provide reviewable, default-preview scripts for this user's free local TSF test certificate, signing, verification, trust and revocation.
**Architecture:** One PowerShell entry point plus shared validation/transaction functions; reuse existing Audit, Register, Unregister and Seal scripts. Local state binds exact thumbprint, user/machine, source commit and signed file hashes. No certificate or trust operation runs in CI.
**Tech Stack:** Existing PowerShell 7, Microsoft certificate provider, official Windows SDK SignTool. No new dependency.
**Spec:** `docs/tsf/LOCAL_TEST_SIGNING.md`, reconciled with the user-supplied local `signing-preparation/FREE_LOCAL_TEST_SIGNING_PLAN.md`.

## Global constraints

- 30 days (retain existing proposal), RSA 2048, SHA-256, code-signing EKU, DigitalSignature, non-exportable CurrentUser/My key.
- Trust only exact thumbprint in CurrentUser/Root and TrustedPublisher after explicit approval. No elevation, LocalMachine, security-policy changes, automatic registration or activation.
- Original package untouched; no private keys, PFX, passwords or local state uploaded.
- Preserve pre-existing trust; journal imports and revoke only this operation's additions. Use certificate-provider DeleteKey for the owned private key.
- CI/signature success does not satisfy external-host acceptance.

## Review focus

- Default preview calls no certificate/signing/import/removal helper and writes no state.
- Wrong account/machine, source commit, thumbprint or changed payload is rejected.
- Sign/export/import failures preserve unsigned input and roll back only owned outputs.
- Unregister failure keeps trust/key/state available for recovery.
- Untrusted/unknown/invalid signatures cannot pass Verify or Seal.

## Tasks

- [x] Write and run failing PowerShell contract tests for these boundaries, with only OS certificate and signing operations substituted to avoid system changes.
- [x] Implement `scripts/Local-TsfSigning.ps1` and `scripts/TsfSigning.Common.ps1`; run tests to green.
- [x] Make Seal default-preview, add read-only helper `status`, and keep/strengthen existing registration Valid guard.
- [x] Include scripts, guide and source provenance in the unsigned package; extend audit allowlist and CI tests.
- [ ] Reconcile guide, record version comparison and manual gates, independently review, push to PR #10, run Windows CI and audit its artifact.
- [ ] Stop before certificate-store Apply, trust import, elevation, system changes, registration or activation. Provide exact next commands and inverse operations for confirmation.

Ruling: the user's explicit continuation instruction authorizes this implementation within the two existing schemes; no duplicate design-approval round is needed. Local state can enforce intended machine/account usage but cannot prevent someone independently importing a public CER elsewhere.
