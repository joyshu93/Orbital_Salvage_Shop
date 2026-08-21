# Curio Clerk 1.0.0 release-evidence index

Evidence status: PENDING_DEVELOPER_EVIDENCE

This directory is a sanitized index and template. It does not claim that a test, build, device run, Seller Portal action, certification, or RC decision has occurred.

## Expected evidence

| Evidence | Status | Planned record |
| --- | --- | --- |
| Automated tests | Not run | Task 11 `automated-tests.md` with developer-supplied counts and relative retained-log paths. |
| AAB inspection | Not run | Task 9 sanitized manifest/inspection output and developer-confirmed SHA-256. |
| Owned-device validation | Pending developer evidence | Task 11 `owned-device.md`. |
| Remote Test Lab | Pending developer evidence | Task 11 `remote-test-lab.md`. |
| Service validation | Pending developer evidence | Task 11 `service-validation.md`. |
| RC decision | Pending developer evidence | Task 11 `rc-decision.md` with the developer's dated decision. |

## Repository safety rules

- Do not commit identity documents, account records, financial information, credentials, access tokens, signing material, keystore material, real ad identifiers, or Seller Portal secrets.
- Do not commit raw logs containing personal data or account details.
- Record repository-relative artifact or sanitized log locations only; do not commit machine-absolute paths.
- Do not invent pass counts, device results, AAB hashes, account status, certification status, or release decisions.
- Replace a pending state only from developer-supplied evidence for the exact Git SHA and release candidate.
