# Security Policy

## Supported versions

BigO X ships as a single, continuously versioned package. Security fixes are made against the **latest
released version** on [NuGet](https://www.nuget.org/packages/BigOX). Please upgrade to the latest version
before reporting an issue.

## Reporting a vulnerability

- **Non-sensitive bugs** (including low-risk security hardening suggestions) may be reported publicly via
  [GitHub Issues](https://github.com/omarbesiso/BigOX/issues).
- **Potential vulnerabilities** should be reported **privately**. Use the **"Contact owners"** channel on
  the [NuGet package page](https://www.nuget.org/packages/BigOX) so that details are not disclosed publicly
  before a fix is available.

Please include enough detail to reproduce the issue (affected version, a minimal repro, and the observed
versus expected behavior). You can expect an acknowledgement and, where the report is valid, a remediation
plan.

## Scope and threat model

BigO X is an in-process utility library. It deliberately keeps a narrow surface:

- It does **not** handle secrets, credentials, or cryptographic primitives.
- It does **not** perform network access or file/stream I/O of its own (stream *helpers* operate only on
  streams you provide).
- It stays **in-process and in-memory**; there is no persistence, no process spawning, and no dynamic code
  execution.

The most common real-world security consideration when using BigO X is **what you put into errors and
result/metadata dictionaries and then log or serialize**. `Error`, `IError`, and the result/authorization
types can carry arbitrary metadata; avoid placing secrets, PII, or internal system details into messages,
codes, or metadata that may be logged or returned to callers. The authorization types are designed with
this in mind — keep `AuthorizationResult.Message` non-sensitive.

## Reflection and trimming note

The dependency-injection registration helpers use assembly scanning (via Scrutor) and reflection. The
library is **not currently annotated for trimming/NativeAOT**; if you deploy in a trimmed or AOT
configuration, validate your DI wiring and add the appropriate preservation hints for your application.
