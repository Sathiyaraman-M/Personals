using FluentMigrator;
using System.Diagnostics.CodeAnalysis;

namespace Personals.Infrastructure.Migrations;

[SuppressMessage(
    category: "Naming",
    checkId: "CA1707:Identifiers should not contain underscores",
    Justification = "This is a migration file.",
    Scope = "namespace",
    Target = "Personals.Core.Migrations")]
[Migration(9)]
public class Migration_09_RemoveUnusedColumnsInAppUser : Migration
{
    public override void Up()
    {
        Delete.Column("Address1").FromTable("AppUsers");
        Delete.Column("Address2").FromTable("AppUsers");
        Delete.Column("City").FromTable("AppUsers");
        Delete.Column("PostCode").FromTable("AppUsers");
        Delete.Column("StateCode").FromTable("AppUsers");
    }

    public override void Down()
    {
        Alter.Table("AppUsers")
            .AddColumn("Address1").AsString(300).NotNullable()
            .AddColumn("Address2").AsString(300).Nullable()
            .AddColumn("City").AsString(50).NotNullable()
            .AddColumn("PostCode").AsString(6).NotNullable()
            .AddColumn("StateCode").AsString(2).NotNullable();
    }
}