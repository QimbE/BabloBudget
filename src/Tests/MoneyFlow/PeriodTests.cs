using BabloBudget.Api.Domain;
using Shouldly;


namespace Tests.MoneyFlow;

[TestClass]
public class PeriodTests
{
    [TestMethod]
    [DataRow(1, DisplayName = "Daily")]
    [DataRow(7, DisplayName = "Weekly")]
    [DataRow(30, DisplayName = "Monthly")]
    public void FromDays_ValidAmountOfDays_ShouldReturnPeriod(int days)
    {
        // Act
        var result = Period.FromDays(days);
        
        // Assert
        result.Days.ShouldBe(days);
    }
    
    [TestMethod]
    [DataRow(-1, DisplayName = "Negative")]
    [DataRow(22, DisplayName = "Unsupported")]
    public void FromDays_InvalidAmountOfDays_ShouldThrow(int days)
    {
        // Arrange
        var fromDays = () => Period.FromDays(days);
        
        // Assert
        fromDays.ShouldThrow<ArgumentOutOfRangeException>();
    }
}