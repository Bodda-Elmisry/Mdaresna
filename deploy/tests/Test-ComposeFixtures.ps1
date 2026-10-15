$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
# These are synthetic build-test values, never read a real env file or deploy any service.
$fixtures = @{
    PLATFORM_UPLOAD_ENCRYPTION_KEY = [Convert]::ToBase64String([byte[]]::new(32))
    SCHOOLS_UPLOAD_ENCRYPTION_KEY = [Convert]::ToBase64String([byte[]]::new(32))
    PLATFORM_DOMAIN = 'platform.example.invalid'
    SCHOOLS_DOMAIN = 'schools.example.invalid'
    RABBITMQ_TLS_HOST = 'broker.example.invalid'
    RABBITMQ_AMQPS_URI = 'amqps://fixture:fixture@broker.example.invalid:5671/'
    RABBITMQ_TLS_DIRECTORY = $repoRoot
    GATEWAY_TLS_DIRECTORY = $repoRoot
    SCHOOL_DOCUMENT_STORAGE_PROVIDER = 'Local'
    SCHOOL_DOCUMENT_R2_ENDPOINT = ''
    SCHOOL_DOCUMENT_R2_BUCKET = ''
    SCHOOL_DOCUMENT_R2_ACCESS_KEY_ID = ''
    SCHOOL_DOCUMENT_R2_SECRET_ACCESS_KEY = ''
}
$previous = @{}
try {
    foreach ($key in $fixtures.Keys) {
        $previous[$key] = [Environment]::GetEnvironmentVariable($key, 'Process')
        [Environment]::SetEnvironmentVariable($key, $fixtures[$key], 'Process')
    }
    $baseArgs = @('compose','--env-file',"$repoRoot/.env.docker.example",'-f',"$repoRoot/docker-compose.yml")
    & "$repoRoot/deploy/Test-DockerConfiguration.ps1" -EnvFile "$repoRoot/.env.docker.example"
    foreach ($production in @($false, $true)) {
        $argsToUse = $baseArgs
        if ($production) { $argsToUse += @('-f',"$repoRoot/docker-compose.production.yml") }
        $json = & docker @argsToUse config --format json
        if ($LASTEXITCODE -ne 0) { throw 'Compose fixture resolution failed.' }
        $config = ($json -join "`n") | ConvertFrom-Json
        foreach ($name in @('platform-api','platform-worker','schools-api','platform-web','schools-web')) {
            $service = $config.services.$name
            if (-not $service.read_only -or 'ALL' -notin $service.cap_drop -or -not $service.mem_limit) { throw "Hardening missing for $name." }
            if ($production) {
                if (@($service.ports | Where-Object { $null -ne $_ }).Count -ne 0) { throw "Public port leaked for $name." }
            } elseif (@($service.ports | Where-Object host_ip -NE '127.0.0.1').Count -ne 0) { throw "Non-loopback local port for $name." }
        }
        if ($production) {
            foreach ($name in @('postgres','rabbitmq','redis')) { if (@($config.services.$name.ports | Where-Object { $null -ne $_ }).Count) { throw 'Infrastructure port leaked.' } }
            if ($config.services.rabbitmq.volumes.Count -ne 3) { throw 'Broker persistence or TLS mount missing.' }
            if ($config.services.'platform-api'.environment.ASPNETCORE_ENVIRONMENT -ne 'Production') { throw 'Wrong production environment.' }
            if ($config.services.'schools-api'.environment.SchoolProvisioningConsumer__BrokerUri -notlike 'amqps://*') { throw 'Production AMQPS missing.' }
            if ($config.services.gateway.ports.Count -ne 2) { throw 'Gateway ports missing.' }
        }
    }
    # Negative preflight: missing TLS files must not be reported as deployable.
    $rejected = $false
    try { & "$repoRoot/deploy/Test-DockerConfiguration.ps1" -EnvFile "$repoRoot/.env.docker.example" -Production }
    catch { $rejected = $true }
    if (-not $rejected) { throw 'Expected missing TLS preflight rejection.' }
    [Environment]::SetEnvironmentVariable('SCHOOL_DOCUMENT_STORAGE_PROVIDER', 'R2', 'Process')
    $r2Rejected = $false
    try { & "$repoRoot/deploy/Test-DockerConfiguration.ps1" -EnvFile "$repoRoot/.env.docker.example" }
    catch { $r2Rejected = $true }
    if (-not $r2Rejected) { throw 'Expected missing R2 settings preflight rejection.' }
    [Environment]::SetEnvironmentVariable('SCHOOL_DOCUMENT_R2_ENDPOINT', 'https://fixture.r2.cloudflarestorage.com', 'Process')
    [Environment]::SetEnvironmentVariable('SCHOOL_DOCUMENT_R2_BUCKET', 'schools-test', 'Process')
    [Environment]::SetEnvironmentVariable('SCHOOL_DOCUMENT_R2_ACCESS_KEY_ID', 'fixture', 'Process')
    [Environment]::SetEnvironmentVariable('SCHOOL_DOCUMENT_R2_SECRET_ACCESS_KEY', 'fixture', 'Process')
    & "$repoRoot/deploy/Test-DockerConfiguration.ps1" -EnvFile "$repoRoot/.env.docker.example"
    Write-Output 'Local/production Compose assertions, R2 validation and missing-TLS rejection passed.'
} finally {
    foreach ($key in $fixtures.Keys) { [Environment]::SetEnvironmentVariable($key, $previous[$key], 'Process') }
}
