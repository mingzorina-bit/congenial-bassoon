# Phase 1-A: TSF feasibility design

Status: design for the next trial milestone, 2026-10-12. Authority: frozen A-15/A-16, `docs/POST_ALPHA_ROADMAP.md`, and the user's successful v0.2.1 trial and direction to advance. This is a Windows desktop feasibility build, not a system-wide release claim.

## User outcome and success boundary

The user wants to type with BilingualInput outside its prototype window. A successful spike proves that an explicitly selected TSF profile can use the existing local Core to compose `nihao` and `beijing`, show candidates, change highlight, commit Chinese with Space, preserve raw input with Enter, and cancel with Esc in two external desktop editors. Notepad and a second installed desktop editor are tested and named in the trial record; the user's own input and screenshots are never uploaded. A password/secure field and a host where TSF is unavailable are separate negative tests. Success in two named editors does not imply compatibility with browsers, Office, Electron, all password fields, or the Windows secure desktop.

## Architecture

Build a separate x64 C++20 COM in-process DLL, `BilingualTsf.dll`, that implements the minimum TSF processor/key-event interfaces. It adapts TSF keystrokes and edit sessions to the existing `InputSession` and `RimeAdapter`; candidate ranking, commit text and Shadow rules remain in Core. The DLL's activation holds one serialized Rime adapter per process and one composition state per active TSF context. No WinUI, SQLite, OpenAI provider, background process, global keyboard hook or network request loads into the host application.

Keep the first candidate UI compact: five visible items, highlighted item and raw composition, anchored near the caret in desktop hosts. Respect TSF's host UI visibility contract; if the host denies custom UI, omit the popup and retain keyboard/raw fallback. Composition text must be inserted and updated only inside TSF edit sessions. End the composition on commit; cancel clears it without inserting stale text. Focus loss and deactivation release popup, context references and pending input. No asynchronous result may edit a document after the focus changes.

The spike uses local pinyin and the existing curated Shadow data only. It does not change the product's key meanings: Space selects Primary, Enter inserts raw, arrows change highlight, Esc cancels. Tab remains a Details action in the WinUI prototype; the TSF spike must not steal Tab in an external editor until a real external Details surface exists. Likewise Shift+Enter is not advertised in the external host until its insertion and UI behavior are proven. This deliberate narrower host surface prevents claiming unimplemented Phase 0 behavior.

## Registration, security and failure handling

The build must not auto-register or auto-enable the service. Package a separate, explicit opt-in registration and matching unregister path; record the class/profile GUIDs, target DLL path, exact registry/TSF scope, required permissions, and rollback steps before running either. Registration and signing are reviewed before any installation on the user's computer. Build/CI and static checks do not require registration. Do not set the default keyboard or silently change machine-wide registry state. If the required registration scope or rollback cannot be established, stop before installing.

The TSF DLL never requests surrounding text or invokes Cloud, Learning or logging. It should decline composition when TSF reports a disabled keyboard or known sensitive input scope; when sensitivity is unknown, it uses local-only conversion and retains no context across fields. Password-field behavior must be measured, because the prototype's simulated Secure toggle does not prove true TSF detection. Any uncertainty is reported as an unmet security gate rather than hidden behind a PASS. If Rime data or COM/TSF calls fail, the service yields the key to the host or commits the raw input through a valid edit session; it must not lose keystrokes or crash the editor.

## Acceptance evidence and stopping point

- CI builds the x64 DLL and tests its pure key-to-Core state adapter; no registry mutation in CI.
- A source/package audit confirms only approved Luna/Essay data and project-authored bilingual rows, with their notices. No secrets or user text are packaged.
- A registration dry-run/inventory and an unregister review show exactly what changes and how to reverse them. Actual installation requires the final review gate.
- Human trial, after explicit installation approval, records the two editor names/versions, typing and focus cases, candidate display, Space/Enter/Esc, and normal keyboard restoration after disable/unregister. A secure-field trial is recorded separately; a failure keeps Phase 1-A incomplete.
- Stop at the first trial-ready TSF spike. Candidate paging, abbreviation/fuzzy pinyin and broad Details remain in `docs/USER_FEEDBACK_V021.md` for a later milestone.

## References

- [Microsoft TSF architecture](https://learn.microsoft.com/en-us/windows/win32/tsf/architecture)
- [Microsoft custom IME requirements](https://learn.microsoft.com/en-us/windows/apps/develop/input/input-method-editor-requirements)
- [Microsoft composition/edit-session guidance](https://learn.microsoft.com/en-us/windows/win32/tsf/compositions)
- [Microsoft text-service registration](https://learn.microsoft.com/en-us/windows/win32/tsf/text-service-registration)
- [Microsoft key event sink](https://learn.microsoft.com/en-us/windows/win32/api/msctf/nn-msctf-itfkeyeventsink)
- [Microsoft security considerations](https://learn.microsoft.com/en-us/windows/win32/tsf/tsf-security-information)

