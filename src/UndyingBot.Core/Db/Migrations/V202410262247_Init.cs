using FluentMigrator;

namespace UndyingBot.Core.Db.Migrations;

[Migration(202410262247, "Initial migration")]
public class Init : Migration
{
    public override void Up()
    {
        Create.Table("Quotes")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("Text").AsString().Nullable()
            .WithColumn("Author").AsString().Nullable()
            .WithColumn("Url").AsString().Nullable();
    }

    public override void Down()
    {
        Delete.Table("Quotes");
    }
}