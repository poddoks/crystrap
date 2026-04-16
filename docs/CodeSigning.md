# Code signing

Crystrap already has an MIT software license in the repository root. Code signing is a separate Windows trust feature used to sign the published `.exe`.

## What you need

- A Windows code signing certificate in `.pfx` format from a certificate authority
- The certificate password
- Access to the GitHub repository secrets for Crystrap

## GitHub Actions secrets

Add these repository secrets before creating a release:

- `WINDOWS_CERTIFICATE_PFX`
  Store the raw `.pfx` file as a base64 string.
- `WINDOWS_CERTIFICATE_PASSWORD`
  Store the password for that `.pfx`.

## How the workflow behaves

- `ci-release.yml` publishes `Crystrap.exe`
- If both signing secrets are present, the workflow runs `Scripts/sign-release.ps1`
- The script restores the `.pfx`, signs the published executable with `signtool`, and timestamps it
- The workflow then verifies the Authenticode signature before uploading the release artifact

## Creating the base64 secret locally

PowerShell:

```powershell
[Convert]::ToBase64String([IO.File]::ReadAllBytes("C:\path\to\crystrap-signing-cert.pfx"))
```

## Notes

- MIT licensing and code signing are different things. The license controls usage rights; signing improves publisher trust on Windows.
- For the strongest SmartScreen reputation, an EV certificate is usually better than a standard certificate.
- Unsigned builds still work, but Windows is more likely to warn users about them.
