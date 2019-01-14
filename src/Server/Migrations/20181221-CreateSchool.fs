//named this TitanMigrations since there's a lot of 3rd party code out there
//using Migration/s in their names for various things.
namespace TitanMigrations

open FluentMigrator
open System.Collections.Generic
open System.Dynamic

type User = 
    { Email : string
      GivenName : string
      Surname : string }

type TitanClaim =
    { UserId : int
      Type : string
      Value : string }




[<Migration(20181221120907L)>]
type Initial() =
    inherit Migration()

    override this.Up() =
        //create a users table based on what i see in google claims
        this.Create.Table("User")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity().NotNullable().Unique()
            .WithColumn("Email").AsString().NotNullable().Unique()
            .WithColumn("GivenName").AsString().NotNullable()
            .WithColumn("Surname").AsString().NotNullable() |> ignore


        this.Create.Table("School")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity().NotNullable().Unique()
            .WithColumn("UserId").AsInt32().ForeignKey().Unique()
            .WithColumn("Name").AsString().Unique().Nullable()
            .WithColumn("Principal").AsString().Nullable() |> ignore

        this.Create.ForeignKey("FKSchoolUser").FromTable("School") .ForeignColumn("UserId")
            .ToTable("User").PrimaryColumn("Id").OnDelete(System.Data.Rule.Cascade) |> ignore

        //student table.
        this.Create.Table("Student")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity().NotNullable().Unique()
            .WithColumn("UserId").AsInt32().ForeignKey().Unique()
            .WithColumn("Email").AsString().Unique()
            .WithColumn("FirstName").AsString()
            .WithColumn("LastName").AsString() |> ignore

        this.Create.ForeignKey("FKStudentUser").FromTable("Student").ForeignColumn("UserId")
            .ToTable("User").PrimaryColumn("Id").OnDelete(System.Data.Rule.Cascade) |> ignore

        //Table to link a student with a school.
        this.Create.Table("StudentSchool")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity().NotNullable().Unique()
            .WithColumn("SchoolId").AsInt32().ForeignKey().NotNullable()
            .WithColumn("StudentId").AsInt32().ForeignKey().NotNullable() |> ignore

        this.Create.ForeignKey("FKStudentSchoolSchool").FromTable("StudentSchool")
            .ForeignColumn("SchoolId").ToTable("School").PrimaryColumn("Id") |> ignore
        this.Create.ForeignKey("FKStudentSchoolStudent").FromTable("StudentSchool")
            .ForeignColumn("StudentId").ToTable("Student").PrimaryColumn("Id") |> ignore
        
        //Class descriptions linked to a school.
        this.Create.Table("ClassType")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity().NotNullable().Unique()
            .WithColumn("SchoolId").AsInt32().ForeignKey().NotNullable()
            .WithColumn("Name").AsString().Nullable()
            .WithColumn("Description").AsString().Nullable() |> ignore

        this.Create.ForeignKey("FKClassTypeSchool").FromTable("ClassType")
            .ForeignColumn("SchoolId").ToTable("School").PrimaryColumn("Id").OnDelete(System.Data.Rule.Cascade) |> ignore

        //Time a class starts and ends
        this.Create.Table("ClassSchedule")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity().NotNullable().Unique()
            .WithColumn("ClassTypeId").AsInt32().ForeignKey()
            .WithColumn("StartTime").AsDateTimeOffset()
            .WithColumn("EndTime").AsDateTimeOffset() |> ignore

        this.Create.ForeignKey("FKClassScheduleClassType").FromTable("ClassSchedule")
            .ForeignColumn("ClassTypeId").ToTable("ClassType").PrimaryColumn("Id").OnDelete(System.Data.Rule.Cascade) |> ignore
        
        //table to link student with class schedule
        this.Create.Table("ClassScheduleStudent")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity().NotNullable().Unique()
            .WithColumn("ClassScheduleId").AsInt32().ForeignKey()
            .WithColumn("StudentId").AsInt32().ForeignKey() |> ignore

        this.Create.ForeignKey("FKClassScheduleStudentClassSchedule").FromTable("ClassScheduleStudent")
            .ForeignColumn("ClassScheduleId").ToTable("ClassSchedule").PrimaryColumn("Id") |> ignore
        this.Create.ForeignKey("FKClassScheduleStudentStudent").FromTable("ClassScheduleStudent")
            .ForeignColumn("StudentId").ToTable("Student").PrimaryColumn("Id") |> ignore
        this.Create.UniqueConstraint("ConStudentClassSchedule").OnTable("ClassScheduleStudent")
            .Columns([|"ClassScheduleId"; "StudentId"|]) |> ignore

        //table for those registering interest in the app.
        this.Create.Table("Punter")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity().NotNullable().Unique()
            .WithColumn("Email").AsString() |> ignore

        //students wanting to enrol at a school
        this.Create.Table("Pending")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity().NotNullable().Unique()
            .WithColumn("Email").AsString().NotNullable()
            .WithColumn("FirstName").AsString().NotNullable()
            .WithColumn("LastName").AsString().NotNullable()
            .WithColumn("SchoolId").AsInt32().NotNullable() |> ignore

        this.Create.UniqueConstraint("ConPending").OnTable("Pending")
            .Columns([|"Email"; "SchoolId"|]) |> ignore
        this.Create.ForeignKey("FKPendingSchool").FromTable("Pending")
            .ForeignColumn("SchoolId").ToTable("School").PrimaryColumn("Id").OnDelete(System.Data.Rule.Cascade) |> ignore


        this.Create.Table("TitanClaims")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity().NotNullable()
            .WithColumn("UserId").AsInt32().NotNullable() 
            .WithColumn("Type").AsString().NotNullable().Unique()
            .WithColumn("Value").AsString().NotNullable() |> ignore

        this.Create.ForeignKey("FKTitanClaims").FromTable("TitanClaims")
            .ForeignColumn("UserId").ToTable("User").PrimaryColumn("Id").OnDelete(System.Data.Rule.Cascade) |> ignore


        this.Insert.IntoTable("User").Row({Email = "sashang@gmail.com"; GivenName = "Sashan"; Surname = "Govender"}) |> ignore
        this.Insert.IntoTable("TitanClaims").Row({UserId = 1; Type = "IsTitan"; Value = "true"})|> ignore
        this.Insert.IntoTable("User").Row({Email = "sashang@tewtin.com"; GivenName = "Sashan"; Surname = "Govender"}) |> ignore
        this.Insert.IntoTable("TitanClaims").Row({UserId = 2; Type = "IsTitan"; Value = "true"})|> ignore
        
    override this.Down() =
        this.Delete.Table("ClassScheduleStudent") |> ignore
        this.Delete.Table("ClassSchedule") |> ignore
        this.Delete.Table("ClassType") |> ignore
        this.Delete.Table("StudentSchool") |> ignore
        this.Delete.Table("Student") |> ignore
        this.Delete.Table("Pending") |> ignore
        this.Delete.Table("School") |> ignore
        this.Delete.Table("Punter") |> ignore
        this.Delete.Table("TitanClaims") |> ignore
        this.Delete.Table("User") |> ignore
