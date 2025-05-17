using System.Reflection;
using BabloBudget.Api;
using Shouldly;

namespace Tests.MoneyFlow;

[TestClass]
public class PeriodicalScheduleTests
{
    [TestMethod]
    [DynamicData(nameof(NewValidDatesCases), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(GetNewTestDisplayName))]
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
    [DynamicData(nameof(NewInvalidDatesCases), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(GetNewTestDisplayName))]
    public void New_OnInvalidDates_ShouldThrowException(NewScheduleTestCase testCase)
    {
        // Arrange
        var (startingDateUtc, currentDateUtc, period, _) = testCase;
        var dateTimeProvider = TestDateTimeProvider.Create(currentDateUtc);

        var action = () => PeriodicalSchedule.New(startingDateUtc, period, dateTimeProvider);
        
        // Act, Assert
        action.ShouldThrow<ArgumentOutOfRangeException>();
    }
    
    [TestMethod]
    [DynamicData(nameof(ExistingValidDatesCases), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(GetExistingTestDisplayName))]
    public void Existing_OnValidDates_ShouldReturnSchedule(ExistingScheduleTestCase testCase)
    {
        // Arrange
        var (startingDateUtc, lastCheckedUtc, currentDateUtc, period, _) = testCase;
        var dateTimeProvider = TestDateTimeProvider.Create(currentDateUtc);
        
        // Act
        var schedule = PeriodicalSchedule.Existing(startingDateUtc, lastCheckedUtc, period, dateTimeProvider);
        
        // Assert
        schedule.Period.ShouldBe(period);
        schedule.StartingDateUtc.ShouldBe(startingDateUtc);
        schedule.LastCheckedUtc.ShouldBe(lastCheckedUtc);
    }
    
    [TestMethod]
    [DynamicData(nameof(ExistingInvalidDatesCases), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(GetExistingTestDisplayName))]
    public void Existing_OnInvalidDates_ShouldReturnSchedule(ExistingScheduleTestCase testCase)
    {
        // Arrange
        var (startingDateUtc, lastCheckedUtc, currentDateUtc, period, _) = testCase;
        var dateTimeProvider = TestDateTimeProvider.Create(currentDateUtc);
        var action = () => PeriodicalSchedule.Existing(startingDateUtc, lastCheckedUtc, period, dateTimeProvider);

        // Act, Assert
        action.ShouldThrow<ArgumentOutOfRangeException>();
    }

    public static IEnumerable<object[]> NewValidDatesCases()
    {
        var currentDateUtc = DateOnly.FromDateTime(new DateTime(2025, 5, 30, 0, 0, 0, DateTimeKind.Utc));
        yield return new NewScheduleTestCase(
            StartingDateUtc: currentDateUtc.AddDays(1),
            CurrentDateUtc: currentDateUtc,
            Period: Period.CreateDaily(),
            DisplayName: "Starting on the next day");
        
        yield return new NewScheduleTestCase(
            StartingDateUtc: currentDateUtc.AddMonths(1),
            CurrentDateUtc: currentDateUtc,
            Period: Period.CreateDaily(),
            DisplayName: "Starting on the next month");
        
        yield return new NewScheduleTestCase(
            StartingDateUtc: currentDateUtc.AddDays(2),
            CurrentDateUtc: currentDateUtc,
            Period: Period.CreateMonthly(),
            DisplayName: "Starting in 2 days");
    }
    
    public static IEnumerable<object[]> NewInvalidDatesCases()
    {
        var currentDateUtc = DateOnly.FromDateTime(new DateTime(2025, 5, 30, 0, 0, 0, DateTimeKind.Utc));
        yield return new NewScheduleTestCase(
            StartingDateUtc: currentDateUtc,
            CurrentDateUtc: currentDateUtc,
            Period: Period.CreateDaily(),
            DisplayName: "Starting today");
        
        yield return new NewScheduleTestCase(
            StartingDateUtc: currentDateUtc.AddDays(-1),
            CurrentDateUtc: currentDateUtc,
            Period: Period.CreateDaily(),
            DisplayName: "Started yesterday");
    }
    
    public static IEnumerable<object[]> ExistingValidDatesCases()
    {
        var currentDateUtc = DateOnly.FromDateTime(new DateTime(2025, 5, 30, 0, 0, 0, DateTimeKind.Utc));
        yield return new ExistingScheduleTestCase(
            StartingDateUtc: currentDateUtc.AddDays(1),
            LastCheckedUtc: null,
            CurrentDateUtc: currentDateUtc,
            Period: Period.CreateDaily(),
            DisplayName: "Starting on the next day");
        
        yield return new ExistingScheduleTestCase(
            StartingDateUtc: currentDateUtc,
            LastCheckedUtc: currentDateUtc,
            CurrentDateUtc: currentDateUtc,
            Period: Period.CreateDaily(),
            DisplayName: "Last checked today");
        
        yield return new ExistingScheduleTestCase(
            StartingDateUtc: currentDateUtc.AddDays(-2),
            LastCheckedUtc: currentDateUtc.AddDays(-1),
            CurrentDateUtc: currentDateUtc,
            Period: Period.CreateDaily(),
            DisplayName: "Last checked yesterday");
    }
    
    public static IEnumerable<object[]> ExistingInvalidDatesCases()
    {
        var currentDateUtc = DateOnly.FromDateTime(new DateTime(2025, 5, 30, 0, 0, 0, DateTimeKind.Utc));
        yield return new ExistingScheduleTestCase(
            StartingDateUtc: currentDateUtc.AddDays(1),
            LastCheckedUtc: currentDateUtc,
            CurrentDateUtc: currentDateUtc,
            Period: Period.CreateDaily(),
            DisplayName: "Starting on the next day, already checked");
        
        yield return new ExistingScheduleTestCase(
            StartingDateUtc: currentDateUtc,
            LastCheckedUtc: currentDateUtc.AddDays(1),
            CurrentDateUtc: currentDateUtc,
            Period: Period.CreateDaily(),
            DisplayName: "Starting today, checked tomorrow");
    }
    
    public static string GetNewTestDisplayName(MethodInfo methodInfo, object[] values) =>
        $"{methodInfo.Name} ({((NewScheduleTestCase)values[0]).DisplayName})";
    
    public static string GetExistingTestDisplayName(MethodInfo methodInfo, object[] values) =>
        $"{methodInfo.Name} ({((ExistingScheduleTestCase)values[0]).DisplayName})";
}

public sealed record NewScheduleTestCase(
    DateOnly StartingDateUtc,
    DateOnly CurrentDateUtc,
    Period Period,
    string DisplayName)
{
    public static implicit operator object[](NewScheduleTestCase testCase) =>
        [testCase];
}

public sealed record ExistingScheduleTestCase(
    DateOnly StartingDateUtc,
    DateOnly? LastCheckedUtc, 
    DateOnly CurrentDateUtc,
    Period Period,
    string DisplayName)
{
    public static implicit operator object[](ExistingScheduleTestCase testCase) =>
        [testCase];
}