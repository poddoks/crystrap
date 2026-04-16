param(
    [Parameter(Mandatory = $true)]
    [string]$CertificateBase64,

    [Parameter(Mandatory = $true)]
    [string]$CertificatePassword,

    [Parameter(Mandatory = $true)]
    [string]$FilePath,

    [string]$TimestampUrl = "http://timestamp.digicert.com"
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $FilePath)) {
    throw "File to sign was not found: $FilePath"
}

$certificatePath = Join-Path $env:RUNNER_TEMP "crystrap-signing-cert.pfx"
[System.IO.File]::WriteAllBytes($certificatePath, [System.Convert]::FromBase64String($CertificateBase64))

try {
    $signtool = Get-ChildItem "${env:ProgramFiles(x86)}\Windows Kits\10\bin" -Recurse -Filter signtool.exe |
        Sort-Object FullName -Descending |
        Select-Object -First 1 -ExpandProperty FullName

    if (-not $signtool) {
        throw "signtool.exe was not found on this runner."
    }

    & $signtool sign /fd SHA256 /td SHA256 /tr $TimestampUrl /f $certificatePath /p $CertificatePassword $FilePath

    if ($LASTEXITCODE -ne 0) {
        throw "signtool.exe exited with code $LASTEXITCODE"
    }
}
finally {
    if (Test-Path $certificatePath) {
        Remove-Item $certificatePath -Force
    }
}
