using FluentMigrator;
using System.Diagnostics.CodeAnalysis;

namespace Personals.Infrastructure.Migrations;

[SuppressMessage(
    category: "Naming",
    checkId: "CA1707:Identifiers should not contain underscores",
    Justification = "This is a migration file.",
    Scope = "namespace",
    Target = "PayFlow.Core.Migrations")]

[Migration(2)]
public class Migration_02_CreateAppUsersTable : Migration
{
    public override void Up()
    {
        Create.Table("AppUsers")
            .WithColumn("Id").AsGuid().PrimaryKey().NotNullable()
            .WithColumn("Code").AsString(10).Unique().NotNullable()
            .WithColumn("LoginName").AsString(100).Unique().NotNullable()
            .WithColumn("FullName").AsString(100).NotNullable()
            .WithColumn("Address1").AsString(300).NotNullable()
            .WithColumn("Address2").AsString(300).Nullable()
            .WithColumn("City").AsString(50).NotNullable()
            .WithColumn("PostCode").AsString(6).NotNullable()
            .WithColumn("StateCode").AsString(2).NotNullable()
            .WithColumn("EmailAddress").AsString(100).Nullable()
            .WithColumn("PhoneNumber").AsString(20).Nullable()
            .WithColumn("PasswordHash").AsString(100).NotNullable()
            .WithColumn("RefreshToken").AsString(100).Nullable()
            .WithColumn("RefreshTokenExpiryTime").AsDateTime2().Nullable()
            .WithColumn("IsActive").AsBoolean().NotNullable()
            .WithColumn("CreatedByUserName").AsString(100).NotNullable()
            .WithColumn("CreatedByUserId").AsGuid().NotNullable()
            .WithColumn("CreatedOnDate").AsDateTime2().NotNullable()
            .WithColumn("LastModifiedByUserName").AsString(100).Nullable()
            .WithColumn("LastModifiedByUserId").AsGuid().Nullable()
            .WithColumn("LastModifiedOnDate").AsDateTime2().Nullable();
    }

    [ExcludeFromCodeCoverage]
    public override void Down()
    {
        Delete.Table("AppUsers");
    }
}