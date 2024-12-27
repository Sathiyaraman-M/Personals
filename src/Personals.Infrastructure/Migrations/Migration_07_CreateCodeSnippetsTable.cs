using FluentMigrator;
using System.Diagnostics.CodeAnalysis;

namespace Personals.Infrastructure.Migrations;

[SuppressMessage(
    category: "Naming",
    checkId: "CA1707:Identifiers should not contain underscores",
    Justification = "This is a migration file.",
    Scope = "namespace",
    Target = "Personals.Core.Migrations")]
[Migration(7)]
public class Migration_07_CreateCodeSnippetsTable : Migration
{
    public override void Up()
    {
        Create.Table("CodeSnippets")
            .WithColumn("Id").AsGuid().PrimaryKey().NotNullable()
            .WithColumn("Snippet").AsString().NotNullable()
            .WithColumn("Language").AsInt32().NotNullable()
            .WithColumn("Title").AsString().Nullable()
            .WithColumn("Remarks").AsString().Nullable()
            .WithColumn("Tags").AsString().NotNullable()
            .WithColumn("UserId").AsGuid().NotNullable()
            .WithColumn("CreatedOnDate").AsDateTime2().NotNullable()
            .WithColumn("LastModifiedOnDate").AsDateTime2().Nullable();
        Create.ForeignKey("FK_CodeSnippets_UserId")
            .FromTable("CodeSnippets").ForeignColumn("UserId")
            .ToTable("AppUsers").PrimaryColumn("Id");
    }

    [ExcludeFromCodeCoverage]
    public override void Down()
    {
        Delete.Table("CodeSnippets");
    }
}