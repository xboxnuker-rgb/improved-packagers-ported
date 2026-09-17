# Contributing

## Source of truth

Read `README.md`, `AGENTS.md`, and `docs/HANDOVER.md` before starting. Repository documentation and the current game assemblies are authoritative; chat excerpts and old logs are supporting evidence only.

## Branches and commits

- Branch from this repository's refreshed `main` using `<type>/<short-description>`, for example `fix/schedule-i-compat`.
- Keep the original `GuysWeForgotDre/Improved-Packagers` repository configured as `upstream` when checking for later fixes.
- Use conventional commits such as `fix: support Schedule I 0.4.6f13`.
- Keep one compatibility target or gameplay outcome per branch where practical.
- Open a draft pull request while verification is in progress and make remaining checks explicit.

## Local references

Do not copy proprietary or generated game files into this repository. For IL2CPP development, use the matching game installation's:

- `MelonLoader/net6/`
- `MelonLoader/Il2CppAssemblies/`

Pass the parent `MelonLoader` directory to the build and verification commands described in `README.md`.

## Pull request checklist

Every pull request should state:

- The exact Schedule I, Unity, MelonLoader, and backend versions tested.
- The relevant game assembly hash.
- The compatibility signatures that changed.
- Build and static verification commands and results.
- Runtime scenarios exercised and whether testing used an isolated or full mod set.
- Any remaining warnings, limitations, or follow-up work.
- Confirmation that no game binaries, logs, saves, credentials, or local paths were committed.
