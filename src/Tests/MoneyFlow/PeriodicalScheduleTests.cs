using System.Reflection;
using BabloBudget.Api;
using Shouldly;

namespace Tests.MoneyFlow;

[TestClass]
public class PeriodicalScheduleTests
{
    [TestMethod]
    [DynamicData(nameof(ValidDatesCases), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(GetTestDisplayName))]
    public void New_OnValidDates_ShouldReturnSchedule(NewScheduleTestCase testCase)
    {
        // Arrange
        var (startingDateUtc, currentDateUtc, period, _) = testCase;
        var dateTimeProvider = TestDateTimeProvider.Create(currentDateUtc);
        
        // Act
        var schedule = PeriodicalSchedule.New(startingDateUtc, period, dateTimeProvider);
        
        // Assert
        schedule.Period.ShouldBe(period);
        schedule.StartingDateUtc.ShouldBe(startingDateUtc);
        schedule.LastCheckedUtc.ShouldBeNull();
    }
    
    [TestMethod]
    [DynamicData(nameof(InvalidDatesCases), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(GetTestDisplayName))]
    public void New_OnInvalidDates_ShouldThrowException(NewScheduleTestCase testCase)
    {
        // Arrange
        var (startingDateUtc, currentDateUtc, period, _) = testCase;
        var dateTimeProvider = TestDateTimeProvider.Create(currentDateUtc);

        var action = () => PeriodicalSchedule.New(startingDateUtc, period, dateTimeProvider);
        
        // Act, Assert
        action.ShouldThrow<ArgumentOutOfRangeException>();
    }

    public static IEnumerable<object[]> ValidDatesCases()
    {
        var currentDateUtc = DateOnly.FromDateTime(new DateTime(2025, 5, 30, 0, 0, 0, DateTimeKind.Utc));
        yield return new NewScheduleTestCase(
            StartingDateUtc: currentDateUtc.AddDays(1),
            CurrentDateUtc: currentDateUtc,
            Period: Period.CreateDaily(),
            DisplayName: "Starts on the next day");
        
        yield return new NewScheduleTestCase(
            StartingDateUtc: currentDateUtc.AddMonths(1),
            CurrentDateUtc: currentDateUtc,
            Period: Period.CreateDaily(),
            DisplayName: "Starts on the next month");
        
        yield return new NewScheduleTestCase(
            StartingDateUtc: currentDateUtc.AddDays(2),
            CurrentDateUtc: currentDateUtc,
            Period: Period.CreateMonthly(),
            DisplayName: "Starts in 2 days");
    }
    
    public static IEnumerable<object[]> InvalidDatesCases()
    {
        var currentDateUtc = DateOnly.FromDateTime(new DateTime(2025, 5, 30, 0, 0, 0, DateTimeKind.Utc));
        yield return new NewScheduleTestCase(
            StartingDateUtc: currentDateUtc,
            CurrentDateUtc: currentDateUtc,
            Period: Period.CreateDaily(),
            DisplayName: "Starts today");
        
        yield return new NewScheduleTestCase(
            StartingDateUtc: currentDateUtc.AddDays(-1),
            CurrentDateUtc: currentDateUtc,
            Period: Period.CreateDaily(),
            DisplayName: "Started yesterday");
    }
    
    public static string GetTestDisplayName(MethodInfo methodInfo, object[] values)
    {
        return $"{methodInfo.Name} ({((NewScheduleTestCase)values[0]).DisplayName})";
    }

}

public sealed record NewScheduleTestCase(DateOnly StartingDateUtc, DateOnly CurrentDateUtc, Period Period, string DisplayName)
{
    public static implicit operator object[](NewScheduleTestCase testCase) =>
        [testCase];
};