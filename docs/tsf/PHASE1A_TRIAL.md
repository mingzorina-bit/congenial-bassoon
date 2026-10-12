# BilingualInput Phase 1-A TSF trial — preliminary package

This ZIP is an isolated Windows x64 text-service trial for desktop editors. It does not install or activate itself. The existing WinUI v0.2.1 prototype remains the reference for bilingual Shadow and Details. This TSF trial currently covers offline pinyin composition, up to five candidates, Space to select, Enter to commit raw letters, arrows to highlight, and Esc to cancel. Tab and Shift+Enter pass through to the editor. No cloud, learning database, or content logging runs inside the TSF DLL.

## Before installation

Keep this entire folder together. In PowerShell, run `./scripts/Register-TsfTrial.ps1` without `-Apply` to inspect the plan. It prints the fixed CLSID, profile GUID, local DLL path, per-user COM registry scope, and Windows-managed TSF profile scope without making changes. `./scripts/Unregister-TsfTrial.ps1` similarly previews removal.

Do not use `-Apply` until the separate registration/signing and rollback review is complete. This CI package is unsigned, and the registration script rejects it. Microsoft requires third-party IMEs to be digitally signed; this ZIP is therefore a reviewable build, not yet an installable external-editor trial. The trial does not claim compatibility with secure, UI-less, or app-container hosts.

After the TSF DLL, Rime engine and registration helper are signed with a trusted code-signing certificate, run `./scripts/Seal-SignedTsfTrial.ps1 -PackageRoot .` from the extracted folder to rebuild and audit the hash inventory, then complete the installation review. Registration requires an explicit `-Apply` switch and may need elevation depending on Windows TSF policy. Registration enables a zh-CN language profile but does not select it automatically. Keep the signed ZIP so the matching removal helper remains available. The inverse command is `./scripts/Unregister-TsfTrial.ps1 -Apply`; it targets only the trial CLSID/profile and the marked trial COM registry key.

The proposed free one-machine signing procedure is in `docs/tsf/LOCAL_TEST_SIGNING.md`. It has not been executed and requires separate approval before certificate trust import.

## Manual host trial after registration

Select “Bilingual Input TSF Trial” in Windows language settings. In Notepad and one other named desktop editor, type `nihao` and `beijing`; confirm the highlighted candidate equals the actual Space commit, including the fifth option. Type an English word followed by Space and confirm the separator remains. Use Enter for raw text and Esc to cancel. Switch focus mid-composition, including to another editor; no raw composition or candidate popup may remain in the old editor. Repeat after a forced or observed edit failure. Separately check a native password field and a browser password field: no candidates, no retained composition, no text disclosure. Finally unregister, reopen both editors, verify the previous keyboard works and the trial profile is absent. Any failed secure-field or cleanup check blocks Phase 1-A acceptance.

The popup is shown only when TSF exposes a visible desktop host and usable caret location. UI-less and app-container hosts are outside this trial. Host-level behavior, signing, and clean-machine installation are pending human validation; passing CI only confirms build and module-level contracts.

Data: pinned Luna Pinyin and Rime Essay are included only for this authorized trial. Their source notes and licenses are included. Commercial use requires separate review.
