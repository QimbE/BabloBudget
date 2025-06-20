﻿using System.Reflection;
using BabloBudget.Api.Domain;
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

    [TestMethod]
    [DynamicData(nameof(TryMarkCheckedTestCases), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(TryMarkCheckedTestDisplayName))]
    public void TryMarkChecked_ShouldReturnExpectedResult(TryMarkCheckedTestCase testCase)
    {
        // Arrange
        var (sourceSchedule, currentDateUtc, expectedSchedule, _) = testCase;
        var dateTimeProvider = TestDateTimeProvider.Create(currentDateUtc);
        
        // Act
        var result = sourceSchedule.TryMarkChecked(dateTimeProvider);
        
        // Assert
        result.ShouldBe(expectedSchedule);
    }

    [TestMethod]
    [DynamicData(nameof(IsOnTimeTestCases), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(IsOnTimeTestDisplayName))]
    public void IsOnTime_ShouldReturnExpectedResult(IsOnTimeTestCase testCase)
    {
        // Arrange
        var (schedule, currentDateUtc, expectedIsOnTime, _) = testCase;
        
        var dateTimeProvider = TestDateTimeProvider.Create(currentDateUtc);

        // Act
        var result = schedule.IsOnTime(dateTimeProvider);

        // Assert
        result.ShouldBe(expectedIsOnTime);
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
        
        yield return new ExistingScheduleTestCase(
            StartingDateUtc: currentDateUtc,
            LastCheckedUtc: currentDateUtc.AddDays(1),
            CurrentDateUtc: currentDateUtc.AddDays(1),
            Period: Period.CreateMonthly(),
            DisplayName: "Last time checked out of schedule");
    }
    
    public static IEnumerable<object[]> TryMarkCheckedTestCases()
    {
        #region SourceSchedule
        var currentDateUtc = DateOnly.FromDateTime(
            new DateTime(2025, 5, 30, 0, 0, 0, DateTimeKind.Utc));
        var startingDateUtc = currentDateUtc.AddDays(1);

        var sourceSchedule = PeriodicalSchedule.New(
            startingDateUtc,
            Period.CreateDaily(),
            TestDateTimeProvider.Create(currentDateUtc));
        #endregion

        #region Checked 2 periods after start, check on schedule expected
        var checkedTime = startingDateUtc.AddDays(2);

        var expectedSchedule1 = PeriodicalSchedule.Existing(
            startingDateUtc,
            startingDateUtc, // expecting next date on schedule
            Period.CreateDaily(),
            TestDateTimeProvider.Create(checkedTime));
        
        yield return new TryMarkCheckedTestCase(
            sourceSchedule,
            checkedTime,
            expectedSchedule1,
            "Checked 2 periods after start, check on schedule expected");
        #endregion

        #region Checked before start

        var checkedTime3 = startingDateUtc.AddDays(-1);
        yield return new TryMarkCheckedTestCase(
            sourceSchedule,
            checkedTime3,
            null,
            "Checked before start");

        #endregion

        #region SourceSchedule2
        var sourceSchedule2 = PeriodicalSchedule.Existing(
            startingDateUtc,
            startingDateUtc.AddDays(2),
            Period.CreateDaily(),
            TestDateTimeProvider.Create(startingDateUtc.AddDays(2)));
        #endregion

        #region Already checked before
        var checkedTime4 = startingDateUtc.AddDays(1);
        
        yield return new TryMarkCheckedTestCase(
            sourceSchedule2,
            checkedTime4,
            null,
            "Already checked before");

        #endregion
        
        #region Checked 2 periods after last check, check on schedule expected
        var checkedTime2 = sourceSchedule2.LastCheckedUtc!.Value.AddDays(2);
        var expectedSchedule2 = PeriodicalSchedule.Existing(
            startingDateUtc,
            sourceSchedule2.LastCheckedUtc!.Value.AddDays(1),
            Period.CreateDaily(),
            TestDateTimeProvider.Create(checkedTime2));
        
        yield return new TryMarkCheckedTestCase(
            sourceSchedule2,
            checkedTime2,
            expectedSchedule2,
            "Checked 2 periods after last check, check on schedule expected");

        #endregion
    }

    public static IEnumerable<object[]> IsOnTimeTestCases()
    {
        var currentDateUtc = DateOnly.FromDateTime(
            new DateTime(year: 2025, month: 5, day: 30, hour: 0, minute: 0, second: 0, kind: DateTimeKind.Utc));
        var currentDateTimeProvider = TestDateTimeProvider.Create(currentDateUtc);
        
        var startingDateUtc = currentDateUtc.AddDays(1);

        var newSchedule = PeriodicalSchedule.New(
            startingDateUtc: startingDateUtc,
            period: Period.CreateDaily(),
            dateTimeProvider: currentDateTimeProvider);

        yield return new IsOnTimeTestCase(
            Schedule: newSchedule,
            CurrentDateUtc: currentDateUtc,
            IsOnTime: false,
            DisplayName: "Never checked, time has not come yet");
        
        
        yield return new IsOnTimeTestCase(
            Schedule: newSchedule,
            CurrentDateUtc: startingDateUtc.AddDays(1),
            IsOnTime: true,
            DisplayName: "Never checked, time has come");

        var existingSchedule = PeriodicalSchedule.Existing(
            startingDateUtc,
            startingDateUtc.AddDays(1),
            Period.CreateDaily(),
            TestDateTimeProvider.Create(startingDateUtc.AddDays(1)));
        
        yield return new IsOnTimeTestCase(
            Schedule: existingSchedule,
            CurrentDateUtc: existingSchedule.LastCheckedUtc!.Value,
            IsOnTime: false,
            DisplayName: "Checked today, time has not come yet");
        
        yield return new IsOnTimeTestCase(
            Schedule: existingSchedule,
            CurrentDateUtc: existingSchedule.LastCheckedUtc!.Value.AddDays(1),
            IsOnTime: true,
            DisplayName: "Checked yesterday, time has come");
    }
    
    public static string GetNewTestDisplayName(MethodInfo methodInfo, object[] values) =>
        $"{methodInfo.Name} ({((NewScheduleTestCase)values[0]).DisplayName})";
    
    public static string GetExistingTestDisplayName(MethodInfo methodInfo, object[] values) =>
        $"{methodInfo.Name} ({((ExistingScheduleTestCase)values[0]).DisplayName})";
    
    public static string TryMarkCheckedTestDisplayName(MethodInfo methodInfo, object[] values) =>
        $"{methodInfo.Name} ({((TryMarkCheckedTestCase)values[0]).DisplayName})";
    
    public static string IsOnTimeTestDisplayName(MethodInfo methodInfo, object[] values) =>
        $"{methodInfo.Name} ({((IsOnTimeTestCase)values[0]).DisplayName})";
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

public sealed record TryMarkCheckedTestCase(
    PeriodicalSchedule SourceSchedule,
    DateOnly CurrentDateUtc,
    PeriodicalSchedule? ExpectedSchedule,
    string DisplayName)
{
    public static implicit operator object[](TryMarkCheckedTestCase testCase) =>
        [testCase];
}

public sealed record IsOnTimeTestCase(
    PeriodicalSchedule Schedule,
    DateOnly CurrentDateUtc,
    bool IsOnTime,
    string DisplayName)
{
    public static implicit operator object[](IsOnTimeTestCase testCase) =>
        [testCase];
}