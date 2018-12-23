//named this TitanMigrations since there's a lot of 3rd party code out there
//using Migration/s in their names for various things.
namespace TitanMigrations

open FluentMigrator

[<Migration(20181221120907L)>]
type AddSchool() =
    inherit Migration()

    override this.Up() =
        this.Create.Table("Schools")
            .WithColumn("Id").AsInt64().PrimaryKey().Identity()
            .WithColumn("UserId").AsString().ForeignKey()
            .WithColumn("Name").AsString().Unique()
            .WithColumn("Principal").AsString() |> ignore

    override this.Down() =
        this.Delete.Table("Schools") |> ignore
