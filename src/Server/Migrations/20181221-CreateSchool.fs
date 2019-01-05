//named this TitanMigrations since there's a lot of 3rd party code out there
//using Migration/s in their names for various things.
namespace TitanMigrations

open FluentMigrator

[<Migration(20181221120907L)>]
type Initial() =
    inherit Migration()

    override this.Up() =
        this.Create.Table("School")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("UserId").AsString().ForeignKey().Unique()
            .WithColumn("Name").AsString().Unique().Nullable()
            .WithColumn("Principal").AsString().Nullable() |> ignore

        //create fk to the id column in the asp.net users table
        this.Create.ForeignKey("FKUser").FromTable("School")
            .ForeignColumn("UserId").ToTable("AspNetUsers").PrimaryColumn("Id").OnDelete(System.Data.Rule.Cascade) |> ignore
        
        //student table
        this.Create.Table("Student")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("UserId").AsString().ForeignKey().NotNullable()
            .WithColumn("FirstName").AsString()
            .WithColumn("LastName").AsString() |> ignore

        this.Create.ForeignKey("FKUser").FromTable("Student")
            .ForeignColumn("UserId").ToTable("AspNetUsers").PrimaryColumn("Id").OnDelete(System.Data.Rule.Cascade) |> ignore

        //Table to link a student with a school.
        this.Create.Table("StudentSchool")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("SchoolId").AsInt32().ForeignKey().NotNullable()
            .WithColumn("StudentId").AsInt32().ForeignKey().NotNullable() |> ignore

        this.Create.ForeignKey("FKSchool").FromTable("StudentSchool")
            .ForeignColumn("SchoolId").ToTable("School").PrimaryColumn("Id").OnDelete(System.Data.Rule.Cascade) |> ignore
        this.Create.ForeignKey("FKStudent").FromTable("StudentSchool")
            .ForeignColumn("StudentId").ToTable("Student").PrimaryColumn("Id").OnDelete(System.Data.Rule.Cascade) |> ignore
        
        //Class descriptions linked to a school.
        this.Create.Table("ClassType")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("SchoolId").AsInt32().ForeignKey().NotNullable()
            .WithColumn("Name").AsString().Nullable()
            .WithColumn("Description").AsString().Nullable() |> ignore

        this.Create.ForeignKey("FKSchool").FromTable("ClassType")
            .ForeignColumn("SchoolId").ToTable("School").PrimaryColumn("Id").OnDelete(System.Data.Rule.Cascade) |> ignore

        //Time a class starts and ends
        this.Create.Table("ClassSchedule")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("ClassTypeId").AsInt32().ForeignKey()
            .WithColumn("StartTime").AsDateTimeOffset()
            .WithColumn("EndTime").AsDateTimeOffset() |> ignore

        this.Create.ForeignKey("FKClassType").FromTable("ClassSchedule")
            .ForeignColumn("ClassTypeId").ToTable("ClassType").PrimaryColumn("Id").OnDelete(System.Data.Rule.Cascade) |> ignore
        
        //table to link student with class schedule
        this.Create.Table("ClassScheduleStudent")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("ClassScheduleId").AsInt32().ForeignKey()
            .WithColumn("StudentId").AsInt32().ForeignKey() |> ignore

        this.Create.ForeignKey("FKClassSchedule").FromTable("ClassScheduleStudent")
            .ForeignColumn("ClassScheduleId").ToTable("ClassSchedule").PrimaryColumn("Id").OnDelete(System.Data.Rule.Cascade) |> ignore
        this.Create.ForeignKey("FKStudent").FromTable("ClassScheduleStudent")
            .ForeignColumn("StudentId").ToTable("Student").PrimaryColumn("Id").OnDelete(System.Data.Rule.Cascade) |> ignore
        this.Create.UniqueConstraint("ConStudentClassSchedule").OnTable("ClassScheduleStudent")
            .Columns([|"ClassScheduleId"; "StudentId"|]) |> ignore

    override this.Down() =
        this.Delete.Table("ClassScheduleStudent") |> ignore
        this.Delete.Table("ClassSchedule") |> ignore
        this.Delete.Table("ClassType") |> ignore
        this.Delete.Table("StudentSchool") |> ignore
        this.Delete.Table("Student") |> ignore
        this.Delete.Table("School") |> ignore
