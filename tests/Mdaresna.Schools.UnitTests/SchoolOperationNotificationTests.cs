using Mdaresna.Schools.Api.Auth;
using Mdaresna.Schools.Domain.Identity;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Mdaresna.Schools.UnitTests;

public sealed class SchoolOperationNotificationTests
{
    private static SchoolsDbContext Context(bool sqlServer) => sqlServer
        ? new SqlServerSchoolsDbContext(new DbContextOptionsBuilder<SqlServerSchoolsDbContext>().UseSqlServer("Server=localhost;Database=design_only;Integrated Security=true;TrustServerCertificate=true").Options)
        : new PostgreSqlSchoolsDbContext(new DbContextOptionsBuilder<PostgreSqlSchoolsDbContext>().UseNpgsql("Host=localhost;Database=design_only;Username=design_only").Options);

    [Fact]
    public void Actor_is_not_notified_and_same_event_is_added_once_per_recipient()
    {
        using var db = Context(false); var user=Guid.NewGuid(); var actor=Guid.NewGuid();
        bool Add(Guid recipient,string key)=>SchoolOperationNotifications.Add(db,recipient,actor,key,"TeachingPlanChanged","اعتماد","Approved","تفاصيل","Details","TeachingPlan",Guid.NewGuid().ToString("D"),DateTimeOffset.UtcNow);
        Assert.False(Add(actor,"event")); Assert.False(Add(Guid.Empty,"event"));
        Assert.True(Add(user,"event"));Assert.False(Add(user,"event"));Assert.True(Add(Guid.NewGuid(),"event"));
        Assert.Equal(2,db.SchoolUserNotifications.Local.Count);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void Both_providers_have_unique_recipient_event_index_with_nullable_legacy_keys(bool sqlServer)
    {
        using var db=Context(sqlServer);var entity=db.Model.FindEntityType(typeof(SchoolUserNotification))!;
        var index=Assert.Single(entity.GetIndexes(),i=>i.Properties.Select(p=>p.Name).SequenceEqual(new[]{"RecipientUserId","EventKey"}));
        Assert.True(index.IsUnique);Assert.True(entity.FindProperty("EventKey")!.IsNullable);
        Assert.Equal(250,entity.FindProperty("EventKey")!.GetMaxLength());
        if(sqlServer)Assert.Contains("IS NOT NULL",index.GetFilter()!);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void Bounded_evaluation_reminder_query_translates_on_both_providers(bool sqlServer)
    {
        using var db=Context(sqlServer);var now=DateTimeOffset.UtcNow;
        var sql=SchoolOperationReminderService.EvaluationTasks(db,now.AddHours(-24),"SchoolTaskOverdue",now.AddDays(-4),Guid.NewGuid()).ToQueryString();
        Assert.Contains("NOT EXISTS",sql);Assert.Contains("ORDER BY",sql);Assert.Contains("EventKey",sql);
        Assert.Contains(sqlServer?"TOP":"LIMIT",sql);
    }

    [Theory]
    [InlineData(23,0)] [InlineData(24,1)] [InlineData(47,1)] [InlineData(48,2)]
    public void Reminder_and_escalation_boundaries_are_24_and_48_hours(int hours,int expected)
    {
        var now=new DateTimeOffset(2026,10,4,12,0,0,TimeSpan.Zero);
        Assert.Equal(expected,SchoolOperationReminderService.DueLevel(now.AddHours(-hours),now));
    }

    [Fact]
    public void Notification_clipping_preserves_emoji_and_storage_limits()
    {
        using var db=Context(false);var body=new string('ع',998)+"😄"+new string('ع',200);
        SchoolOperationNotifications.Add(db,Guid.NewGuid(),Guid.Empty,"event","Type",new string('ع',210),"Title",body,body,"TeachingPlan","id",DateTimeOffset.UtcNow);
        var row=Assert.Single(db.SchoolUserNotifications.Local);
        Assert.True(row.TitleAr.Length<=200);Assert.True(row.BodyAr.Length<=1000);
        Assert.False(char.IsSurrogate(row.BodyAr[^2]));Assert.EndsWith("…",row.BodyAr);
    }
    [Fact]
    public void Only_the_known_event_index_collision_is_retryable()
    {
        var notification = new DbUpdateException("duplicate",new Npgsql.PostgresException("duplicate","ERROR","ERROR","23505",constraintName:"IX_school_user_notifications_RecipientUserId_EventKey"));
        var other = new DbUpdateException("duplicate",new Npgsql.PostgresException("duplicate","ERROR","ERROR","23505",constraintName:"IX_students_StudentCode"));
        Assert.True(SchoolOperationNotifications.IsDuplicateEvent(notification));Assert.False(SchoolOperationNotifications.IsDuplicateEvent(other));
    }
}
