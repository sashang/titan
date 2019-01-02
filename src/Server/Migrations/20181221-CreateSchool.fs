//named this TitanMigrations since there's a lot of 3rd party code out there
//using Migration/s in their names for various things.
namespace TitanMigrations

open FluentMigrator

[<Migration(20181221120907L)>]
type AddSchool() =
    inherit Migration()

    override this.Up() =
        this.Create.Table("Schools")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity().NotNullable()
            .WithColumn("UserId").AsString().ForeignKey()
            .WithColumn("Name").AsString().Unique().Nullable()
            .WithColumn("Principal").AsString().Nullable() |> ignore

        //create fk to the id column in the asp.net users table
        this.Create.ForeignKey("FKUser").FromTable("Schools")
            .ForeignColumn("UserId").ToTable("AspNetUsers").PrimaryColumn("Id").OnDelete(System.Data.Rule.Cascade) |> ignore

    override this.Down() =
        this.Delete.Table("Schools") |> ignore
