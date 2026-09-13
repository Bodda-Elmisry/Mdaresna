namespace Mdaresna.Platform.Bootstrap;

internal sealed record SmsProviderSeedOptions(bool DryRun)
{
    public static SmsProviderSeedOptions Parse(string[] args)
    {
        if (args.Length == 1 && args[0] == "--seed-sms-provider")
        {
            return new SmsProviderSeedOptions(DryRun: false);
        }

        if (args.Length == 2 && args[0] == "--seed-sms-provider" &&
            args[1] == "--dry-run")
        {
            return new SmsProviderSeedOptions(DryRun: true);
        }

        throw new BootstrapUsageException("Invalid SMS provider seed arguments.");
    }
}
