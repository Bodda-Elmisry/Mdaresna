#requires -Version 7.0
param([switch]$Activate)
$ErrorActionPreference = 'Stop'
$projectPath = Join-Path $PSScriptRoot '../src/Schools/Mdaresna.Schools.Api/Mdaresna.Schools.Api.csproj'
$endpoint = Read-Host 'R2 S3 endpoint (https://ACCOUNT.r2.cloudflarestorage.com)'
$bucket = Read-Host 'Bucket name (for example mdaresna-schools-dev)'
$endpointUri = $null
if (-not [Uri]::TryCreate($endpoint, [UriKind]::Absolute, [ref]$endpointUri) -or $endpointUri.Scheme -ne 'https' -or
    -not $endpointUri.Host.EndsWith('.r2.cloudflarestorage.com') -or -not $endpointUri.IsDefaultPort -or
    $endpointUri.AbsolutePath -ne '/' -or $endpointUri.Query -or $endpointUri.Fragment -or $endpointUri.UserInfo) {
    throw 'Enter the HTTPS R2 account S3 endpoint without bucket paths or query strings.'
}
if ($bucket -notmatch '^[a-z0-9][a-z0-9-]{1,61}[a-z0-9]$') { throw 'Invalid R2 bucket name.' }

function Read-PrivateValue([string]$prompt) {
    $secureValue = Read-Host $prompt -AsSecureString
    $valuePointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secureValue)
    try { return [Runtime.InteropServices.Marshal]::PtrToStringBSTR($valuePointer) }
    finally { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($valuePointer); $secureValue.Dispose() }
}

$settings = @{
    'SchoolDocuments:R2:Endpoint' = $endpoint
    'SchoolDocuments:R2:Bucket' = $bucket
    'SchoolDocuments:R2:AccessKeyId' = (Read-PrivateValue 'Access Key ID (hidden)')
    'SchoolDocuments:R2:SecretAccessKey' = (Read-PrivateValue 'Secret Access Key (hidden)')
}
if ($Activate) { $settings['SchoolDocuments:Provider'] = 'R2' }
if ([string]::IsNullOrWhiteSpace($settings['SchoolDocuments:R2:AccessKeyId']) -or
    [string]::IsNullOrWhiteSpace($settings['SchoolDocuments:R2:SecretAccessKey'])) { throw 'Both R2 access credentials are required.' }
# JSON via stdin: credentials never appear in process arguments or shell history.
$processInfo = [Diagnostics.ProcessStartInfo]::new('dotnet')
$processInfo.UseShellExecute = $false
$processInfo.RedirectStandardInput = $true
$processInfo.RedirectStandardOutput = $true
$processInfo.RedirectStandardError = $true
foreach ($argument in @('user-secrets', 'set', '--project', $projectPath)) { $processInfo.ArgumentList.Add($argument) }
$process = [Diagnostics.Process]::Start($processInfo)
try {
    $outputTask = $process.StandardOutput.ReadToEndAsync()
    $errorTask = $process.StandardError.ReadToEndAsync()
    $process.StandardInput.Write(($settings | ConvertTo-Json -Compress))
    $process.StandardInput.Close()
    $process.WaitForExit()
    $null = $outputTask.GetAwaiter().GetResult()
    $null = $errorTask.GetAwaiter().GetResult()
    if ($process.ExitCode -ne 0) { throw 'Could not save development secrets. Check the .NET SDK and project path.' }
    Write-Host 'R2 settings saved to Schools API development user-secrets. Values were not printed.'
    if (!$Activate) { Write-Host 'Storage provider unchanged. Run the live smoke test before activation.' }
} finally { $settings.Clear(); $process.Dispose() }
