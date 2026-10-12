using Microsoft.Extensions.DependencyInjection;
using VyshKat.Domain;
using Xunit;

namespace VyshKat.Tests;

public sealed class AlwaysPassServiceCenter : IServiceCenter
{
    public int CallCount { get; private set; }
    public bool Inspect(IInventoryItem asset) { CallCount++; return true; }
}

public sealed class AlwaysRejectServiceCenter : IServiceCenter
{
    public bool Inspect(IInventoryItem asset) => false;
}

public class FleetTests
{
    // Test DI container substitutes the interactive service center with the always-pass fake.
    private static (FleetService fleet, AlwaysPassServiceCenter fake) BuildWithFake()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IAssetRepository, InMemoryAssetRepository>();
        services.AddSingleton<AlwaysPassServiceCenter>();
        services.AddSingleton<IServiceCenter>(sp => sp.GetRequiredService<AlwaysPassServiceCenter>());
        services.AddSingleton<FleetService>();
        var provider = services.BuildServiceProvider();
        return (provider.GetRequiredService<FleetService>(), provider.GetRequiredService<AlwaysPassServiceCenter>());
    }

    [Fact]
    public void AlwaysPassFake_AcceptsBothTransportAndEquipment()
    {
        var (fleet, fake) = BuildWithFake();
        Assert.Equal(AcceptanceResult.Accepted, fleet.Register(new ElectricScooter("Электросамокат", 8, 1.5m)));
        Assert.Equal(AcceptanceResult.Accepted, fleet.Register(new Helmet("Шлем", 6)));
        Assert.Equal(2, fake.CallCount);
        Assert.Equal(2, fleet.AssetCount);
    }

    [Fact]
    public void RejectingInspection_PreventsRegistration()
    {
        var fleet = new FleetService(new InMemoryAssetRepository(), new AlwaysRejectServiceCenter());
        var dock = new DockStation("Док", 8, 2m);
        Assert.Equal(AcceptanceResult.Rejected, fleet.Register(dock));
        Assert.Equal("EQ-001", dock.InventoryNumber);
        Assert.Empty(fleet.GetInventory());
    }

    [Fact]
    public void BeginnerRule_IncludesEquipmentAndVehicle_AndExcludesBelowSix()
    {
        var (fleet, _) = BuildWithFake();
        fleet.Register(new Helmet("Шлем", 6));
        fleet.Register(new Bicycle("Велосипед", 5));
        fleet.Register(new DockStation("Док", 9, 4m));
        Assert.Equal(new[] { "EQ-001", "EQ-002" }, fleet.GetBeginnerFriendly().Select(a => a.InventoryNumber));
    }

    [Fact]
    public void SumEnergy_OnlyIncludesEnergyConsumers_AndCountsAllAssets()
    {
        var (fleet, _) = BuildWithFake();
        fleet.Register(new ElectricScooter("Электросамокат", 8, 1.2m));
        fleet.Register(new ChargingCabinet("Шкаф", 6, 3.4m));
        fleet.Register(new Helmet("Шлем", 9));
        fleet.Register(new Bicycle("Велосипед", 7));
        Assert.Equal(4.6m, fleet.TotalDailyEnergyKwh);
        Assert.Equal(4, fleet.AssetCount);
        Assert.Equal(2, fleet.VehicleCount);
    }

    [Fact]
    public void RegularScooter_IsVehicleWithoutEnergyConsumption()
    {
        var (fleet, fake) = BuildWithFake();
        var scooter = new Scooter("Обычный самокат", 7);
        Assert.Equal(AcceptanceResult.Accepted, fleet.Register(scooter));
        Assert.Equal("VH-001", scooter.InventoryNumber);
        Assert.Equal(1, fake.CallCount);
        Assert.Equal(1, fleet.VehicleCount);
        Assert.Equal(0m, fleet.TotalDailyEnergyKwh);
        Assert.Contains(fleet.GetBeginnerFriendly(), asset => asset.InventoryNumber == "VH-001");
        Assert.IsNotAssignableFrom<IEnergyConsumer>(fleet.GetInventory().Single());
    }

    [Fact]
    public void BothScooterTypes_CoexistAndOnlyElectricOneConsumesEnergy()
    {
        var (fleet, _) = BuildWithFake();
        fleet.Register(new Scooter("Самокат", 6));
        fleet.Register(new ElectricScooter("Электросамокат", 8, 2.5m));
        Assert.Equal(2, fleet.VehicleCount);
        Assert.Equal(2.5m, fleet.TotalDailyEnergyKwh);
        Assert.Equal(new[] { "VH-001", "VH-002" }, fleet.GetInventory().Select(a => a.InventoryNumber));
    }

    [Fact]
    public void Fleet_AssignsIndependentSequentialNumbersForVehiclesAndEquipment()
    {
        var (fleet, _) = BuildWithFake();
        var vehicle1 = new Bicycle("Trek", 7);
        var equipment1 = new Helmet("Giro", 8);
        var vehicle2 = new Scooter("Самокат", 9);
        var equipment2 = new ChargingCabinet("Зарядный шкаф", 6, 1.2m);

        Assert.Equal(AcceptanceResult.Accepted, fleet.Register(vehicle1));
        Assert.Equal(AcceptanceResult.Accepted, fleet.Register(equipment1));
        Assert.Equal(AcceptanceResult.Accepted, fleet.Register(vehicle2));
        Assert.Equal(AcceptanceResult.Accepted, fleet.Register(equipment2));

        Assert.Equal("VH-001", vehicle1.InventoryNumber);
        Assert.Equal("EQ-001", equipment1.InventoryNumber);
        Assert.Equal("VH-002", vehicle2.InventoryNumber);
        Assert.Equal("EQ-002", equipment2.InventoryNumber);
    }

    [Fact]
    public void RejectedInspection_ConsumesNumberSoItIsNotReused()
    {
        var repository = new InMemoryAssetRepository();
        var fleet = new FleetService(repository, new AlternatingServiceCenter());
        var rejected = new Bicycle("Отклонённый велосипед", 7);
        var accepted = new Bicycle("Принятый велосипед", 7);

        Assert.Equal(AcceptanceResult.Rejected, fleet.Register(rejected));
        Assert.Equal(AcceptanceResult.Accepted, fleet.Register(accepted));
        Assert.Equal("VH-001", rejected.InventoryNumber);
        Assert.Equal("VH-002", accepted.InventoryNumber);
        Assert.Equal(new[] { "VH-002" }, fleet.GetInventory().Select(a => a.InventoryNumber));
    }

    [Fact]
    public void DuplicateRegistrationOfSameObject_DoesNotRepeatInspection()
    {
        var (fleet, fake) = BuildWithFake();
        var helmet = new Helmet("Шлем", 7);
        Assert.Equal(AcceptanceResult.Accepted, fleet.Register(helmet));
        Assert.Equal(AcceptanceResult.Duplicate, fleet.Register(helmet));
        Assert.Equal(1, fake.CallCount);
        Assert.Equal(1, fleet.AssetCount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public void EaseMustBeInRange(int score) => Assert.Throws<ArgumentOutOfRangeException>(() => new Helmet("Шлем", score));

    [Fact]
    public void NegativeEnergyIsForbidden() => Assert.Throws<ArgumentOutOfRangeException>(() => new ElectricScooter("Электросамокат", 6, -1m));

    private sealed class AlternatingServiceCenter : IServiceCenter
    {
        private int _calls;
        public bool Inspect(IInventoryItem asset) => ++_calls != 1;
    }
}
