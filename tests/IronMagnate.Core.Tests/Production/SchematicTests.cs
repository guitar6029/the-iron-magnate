using IronMagnate.Core.Production;
using IronMagnate.Core.Resources;
using Xunit;

namespace IronMagnate.Core.Tests.Production;

public class SchematicTests
{
    [Fact]
    public void CreatesASchematicWithCatalogResources()
    {
        var schematic = CreateSchematic();

        Assert.Equal("Iron plate", schematic.Name);
        Assert.Equal(3, schematic.Inputs[ResourceType.IronOre]);
        Assert.Equal(ResourceType.CopperOre, schematic.Output);
        Assert.Equal(2, schematic.OutputQuantity);
        Assert.Equal(15, schematic.ProcessingMinutes);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void RejectsBlankNames(string name)
    {
        Assert.Throws<ArgumentException>(() =>
            new Schematic(name, ValidInputs(), ResourceType.CopperOre, 2, 15));
    }

    [Fact]
    public void RejectsNullInputs()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new Schematic("Iron plate", null!, ResourceType.CopperOre, 2, 15));
    }

    [Fact]
    public void RejectsEmptyInputs()
    {
        Assert.Throws<ArgumentException>(() =>
            new Schematic("Iron plate", new Dictionary<ResourceType, int>(), ResourceType.CopperOre, 2, 15));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RejectsNonPositiveInputQuantities(int quantity)
    {
        Assert.Throws<ArgumentException>(() =>
            new Schematic(
                "Iron plate",
                new Dictionary<ResourceType, int> { [ResourceType.IronOre] = quantity },
                ResourceType.CopperOre,
                2,
                15));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RejectsNonPositiveOutputQuantities(int quantity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Schematic("Iron plate", ValidInputs(), ResourceType.CopperOre, quantity, 15));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RejectsNonPositiveProcessingMinutes(int minutes)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Schematic("Iron plate", ValidInputs(), ResourceType.CopperOre, 2, minutes));
    }

    [Fact]
    public void RejectsAnInputResourceTypeMissingFromTheCatalog()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Schematic(
                "Iron plate",
                new Dictionary<ResourceType, int> { [(ResourceType)99] = 3 },
                ResourceType.CopperOre,
                2,
                15));
    }

    [Fact]
    public void RejectsAnOutputResourceTypeMissingFromTheCatalog()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Schematic("Iron plate", ValidInputs(), (ResourceType)99, 2, 15));
    }

    [Fact]
    public void CopiesInputsAndPreventsExternalMutation()
    {
        var inputs = ValidInputs();
        var schematic = new Schematic("Iron plate", inputs, ResourceType.CopperOre, 2, 15);

        inputs[ResourceType.IronOre] = 9;
        inputs.Add(ResourceType.Coal, 1);

        Assert.Equal(3, schematic.Inputs[ResourceType.IronOre]);
        Assert.False(schematic.Inputs.ContainsKey(ResourceType.Coal));
        Assert.Throws<NotSupportedException>(() =>
            ((IDictionary<ResourceType, int>)schematic.Inputs)[ResourceType.IronOre] = 9);
    }

    private static Schematic CreateSchematic() =>
        new("Iron plate", ValidInputs(), ResourceType.CopperOre, 2, 15);

    private static Dictionary<ResourceType, int> ValidInputs() =>
        new() { [ResourceType.IronOre] = 3 };
}
