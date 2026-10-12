namespace VyshKat.Domain;

public abstract class Vehicle : Asset
{
    protected Vehicle(string name, int easeOfUse) : base(name, easeOfUse) { }
}

/// A regular, non-electric scooter.
public sealed class Scooter : Vehicle
{
    public Scooter(string name, int easeOfUse) : base(name, easeOfUse) { }
}

public sealed class ElectricScooter : Vehicle, IEnergyConsumer
{
    public ElectricScooter(string name, int easeOfUse, decimal dailyEnergyKwh)
        : base(name, easeOfUse)
    {
        if (dailyEnergyKwh < 0)
            throw new ArgumentOutOfRangeException(nameof(dailyEnergyKwh));

        DailyEnergyKwh = dailyEnergyKwh;
    }

    public decimal DailyEnergyKwh { get; }
}

public sealed class Bicycle : Vehicle
{
    public Bicycle(string name, int easeOfUse) : base(name, easeOfUse) { }
}

public sealed class ElectricBicycle : Vehicle, IEnergyConsumer
{
    public ElectricBicycle(string name, int easeOfUse, decimal dailyEnergyKwh)
        : base(name, easeOfUse)
    {
        if (dailyEnergyKwh < 0)
            throw new ArgumentOutOfRangeException(nameof(dailyEnergyKwh));

        DailyEnergyKwh = dailyEnergyKwh;
    }

    public decimal DailyEnergyKwh { get; }
}
