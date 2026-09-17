# Agent Instructions

This repository is maintained by human and AI contributors. Keep enough context in the repository for the next contributor to continue without chat history.

## Before changing code

1. Read `README.md`, `CONTRIBUTING.md`, and `docs/HANDOVER.md`.
2. Fetch the upstream repository and inspect open issues and pull requests for overlapping work.
3. Inspect the working tree and preserve unrelated changes.
4. Confirm the exact Schedule I backend and game version represented by the local reference assemblies.

## Project rules

- Never commit game assemblies, generated IL2CPP assemblies, MelonLoader binaries, logs, save data, credentials, or local toolchains.
- Keep game references outside the repository and pass their root through the documented build property.
- Treat IL2CPP and Mono as separate compatibility targets. Do not claim a target is supported unless it was built and tested with matching assemblies.
- Resolve Harmony targets by exact type, method name, and parameter signature. A successful compile is not proof that patches resolve at runtime.
- Keep changes narrowly scoped. Record discovered but unrelated defects in the handover rather than silently expanding the change.
- Do not merge an upstream pull request or publish a release without explicit maintainer or owner approval.

## Verification and handover

- Run the API compatibility check before building.
- Record the reference identity, commands, results, artifact hash, runtime test status, and known limitations in `docs/HANDOVER.md`.
- Keep build output and distributable archives under ignored `artifacts/` or `bin/` directories.
- Use conventional commits and update the changelog for user-visible changes.
