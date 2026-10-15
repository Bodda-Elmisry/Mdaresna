param([Parameter(Mandatory)][string]$EnvFile, [switch]$Production)
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path $PSScriptRoot -Parent
$composeArgs = @('compose', '--env-file', (Resolve-Path -LiteralPath $EnvFile).Path, '-f', "$repoRoot/docker-compose.yml")
if ($Production) { $composeArgs += @('-f', "$repoRoot/docker-compose.production.yml") }
# Capture resolved config in memory. Never print it: it includes server secrets.
$configJson = & docker @composeArgs config --format json
if ($LASTEXITCODE -ne 0) { throw 'Compose configuration could not be resolved.' }
$config = ($configJson -join "`n") | ConvertFrom-Json
$routing = $config.services.'schools-api'.environment
$storageProvider = $routing.SchoolDocuments__Provider
if ($storageProvider -notin @('Local', 'R2')) { throw 'School document provider must be Local or R2.' }
if ($storageProvider -eq 'R2') {
    $storageEndpoint = $null
    if (-not [Uri]::TryCreate($routing.SchoolDocuments__R2__Endpoint, [UriKind]::Absolute, [ref]$storageEndpoint) -or
        $storageEndpoint.Scheme -ne 'https' -or -not $storageEndpoint.Host.EndsWith('.r2.cloudflarestorage.com') -or
        -not $storageEndpoint.IsDefaultPort -or $storageEndpoint.AbsolutePath -ne '/' -or $storageEndpoint.Query -or $storageEndpoint.Fragment -or $storageEndpoint.UserInfo) {
        throw 'R2 requires an HTTPS account S3 endpoint without paths or query strings.'
    }
    if ($routing.SchoolDocuments__R2__Bucket -notmatch '^[a-z0-9][a-z0-9-]{1,61}[a-z0-9]$') { throw 'R2 bucket name is invalid.' }
    foreach ($key in @('SchoolDocuments__R2__AccessKeyId', 'SchoolDocuments__R2__SecretAccessKey')) {
        if ([string]::IsNullOrWhiteSpace($routing.$key)) { throw 'R2 credentials must be supplied from secret storage.' }
    }
    $uploadLimit = 0; $uploadConcurrency = 0
    if (-not [int]::TryParse($routing.SchoolDocuments__R2__MaxUploadMiB, [ref]$uploadLimit) -or $uploadLimit -lt 1 -or $uploadLimit -gt 256 -or
        -not [int]::TryParse($routing.SchoolDocuments__R2__ConcurrentUploads, [ref]$uploadConcurrency) -or $uploadConcurrency -lt 1 -or $uploadConcurrency -gt 8) {
        throw 'R2 upload buffer or concurrency limits are invalid.'
    }
}
if ($routing.SchoolRoutingCache__Enabled -eq 'true') {
    if ($routing.SchoolRoutingCacheConsumer__Enabled -ne 'true') { throw 'Monthly routing cache requires the event consumer.' }
    if ([string]::IsNullOrWhiteSpace($routing.SchoolRoutingCacheConsumer__BrokerUri)) { throw 'Routing cache consumer needs broker configuration.' }
    $routingTtl = 0
    if (-not [int]::TryParse($routing.SchoolRoutingCache__TtlSeconds, [ref]$routingTtl) -or $routingTtl -lt 1 -or $routingTtl -gt 2592000) { throw 'School routing cache TTL must be 1-2592000 seconds (30 days).' }
    if ([string]::IsNullOrWhiteSpace($routing.SchoolRoutingCache__RedisConnection)) { throw 'School routing cache needs Redis connection from secret storage.' }
}
if (@($config.services.redis.ports | Where-Object { $null -ne $_ }).Count -gt 0) { throw 'Redis must stay internal.' }
foreach ($name in @('platform-api','platform-worker','schools-api','platform-web','schools-web')) {
    $service = $config.services.$name
    if (-not $service.read_only -or 'ALL' -notin $service.cap_drop) { throw "$name lacks runtime hardening." }
    if ($Production -and @($service.ports | Where-Object { $null -ne $_ }).Count -gt 0) { throw "$name must not expose a production host port." }
}
foreach ($name in @('platform-api','schools-api')) {
    $settings = $config.services.$name.environment
    $bytes = $null
    try {
        $keyId = $settings.UploadEncryption__ActiveKeyId
        if ($keyId -notmatch '^[A-Za-z0-9_-]{1,64}$') { throw 'Invalid active key id.' }
        $keySetting = "UploadEncryption__Keys__$keyId"
        $bytes = [Convert]::FromBase64String($settings.$keySetting)
        if ($bytes.Length -ne 32) { throw 'Invalid length.' }
    } catch { throw "$name needs a valid 256-bit upload key from secret storage." }
    finally { if ($bytes) { [Array]::Clear($bytes, 0, $bytes.Length) } }
}
if ($Production) {
    foreach ($domain in @($config.services.gateway.environment.PLATFORM_DOMAIN, $config.services.gateway.environment.SCHOOLS_DOMAIN)) {
        if ($domain -notmatch '^(?=.{1,253}$)[A-Za-z0-9](?:[A-Za-z0-9.-]*[A-Za-z0-9])?$' -or $domain.Contains('..')) { throw 'Public domains must be DNS names without schemes, ports or paths.' }
    }
    foreach ($name in @('platform-api','platform-worker','schools-api')) {
        if ($config.services.$name.environment.ASPNETCORE_ENVIRONMENT -ne 'Production') { throw "$name is not in Production." }
    }
    foreach ($name in @('platform-worker','schools-api')) {
        foreach ($setting in $config.services.$name.environment.PSObject.Properties) {
            if ($setting.Name.EndsWith('__BrokerUri') -and -not $setting.Value.StartsWith('amqps://')) { throw "$name requires AMQPS." }
        }
    }
    foreach ($name in @('postgres','rabbitmq')) {
        if (@($config.services.$name.ports | Where-Object { $null -ne $_ }).Count -gt 0) { throw "$name must stay internal." }
    }
    foreach ($name in @('gateway', 'rabbitmq')) {
        $mount = $config.services.$name.volumes | Where-Object target -EQ '/run/tls'
        if ($mount) {
            if (-not (Test-Path -LiteralPath $mount.source -PathType Container)) { throw 'TLS directory is missing.' }
            $required = if ($name -eq 'gateway') { @('fullchain.pem','privkey.pem') } else { @('ca.pem','cert.pem','key.pem') }
            foreach ($file in $required) {
                if (-not (Test-Path -LiteralPath (Join-Path $mount.source $file) -PathType Leaf)) { throw "Required TLS file $file is missing." }
            }
        }
    }
}
$config = $null
$configJson = $null
Write-Output 'Docker configuration checks passed. Certificate trust, database readiness and capacity still require staging tests.'
