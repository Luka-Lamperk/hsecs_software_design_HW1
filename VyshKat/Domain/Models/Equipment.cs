namespace VyshKat.Domain;

public abstract class Equipment : Asset
{
    protected Equipment(string name, int easeOfUse) : base(name, easeOfUse) { }
}

public sealed class Helmet : Equipment
{
    public Helmet(string name, int easeOfUse) : base(name, easeOfUse) { }
}

public sealed class DockStation : Equipment, IEnergyConsumer
{
    public DockStation(string name, int easeOfUse, decimal dailyEnergyKwh)
        : base(name, easeOfUse)
    {
        if (dailyEnergyKwh < 0)
            throw new ArgumentOutOfRangeException(nameof(dailyEnergyKwh));

        DailyEnergyKwh = dailyEnergyKwh;
    }

    public decimal DailyEnergyKwh { get; }
}

public sealed class ChargingCabinet : Equipment, IEnergyConsumer
{
    public ChargingCabinet(string name, int easeOfUse, decimal dailyEnergyKwh)
        : base(name, easeOfUse)
    {
        if (dailyEnergyKwh < 0)
            throw new ArgumentOutOfRangeException(nameof(dailyEnergyKwh));

        DailyEnergyKwh = dailyEnergyKwh;
    }

    public decimal DailyEnergyKwh { get; }
}
