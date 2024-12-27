using FluentMigrator;
using System.Diagnostics.CodeAnalysis;

namespace Personals.Infrastructure.Migrations;

[SuppressMessage(
    category: "Naming",
    checkId: "CA1707:Identifiers should not contain underscores",
    Justification = "This is a migration file.",
    Scope = "namespace",
    Target = "Personals.Core.Migrations")]
[Migration(8)]
public class Migration_08_FixLengthIssuesInLinkAndCodeSnippetTables : Migration
{
    public override void Up()
    {
        Alter.Table("Links").AlterColumn("Url").AsString(2000).NotNullable();
        
        Alter.Table("CodeSnippets").AlterColumn("Snippet").AsString(int.MaxValue).NotNullable();
    }

    public override void Down()
    {
        Alter.Table("Links").AlterColumn("Url").AsString().NotNullable();
        
        Alter.Table("CodeSnippets").AlterColumn("Snippet").AsString().NotNullable();
    }
}