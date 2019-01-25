//named this TitanMigrations since there's a lot of 3rd party code out there
//using Migration/s in their names for various things.
namespace TitanMigrations

open FluentMigrator
open Microsoft.EntityFrameworkCore.Internal


[<Migration(20181221120907L)>]
type Initial() =
    inherit Migration()

    override this.Up() =
        //create a users table based on what i see in google claims
        this.Create.Table("User")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity().NotNullable().Unique()
            .WithColumn("Email").AsString().NotNullable().Unique()
            .WithColumn("Phone").AsString().NotNullable()
            .WithColumn("FirstName").AsString().NotNullable()
            .WithColumn("LastName").AsString().NotNullable() |> ignore


        this.Create.Table("School")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity().NotNullable().Unique()
            .WithColumn("UserId").AsInt32().ForeignKey().Unique()
            .WithColumn("Subjects").AsString() //comma separated keywords
            .WithColumn("Location").AsString()
            .WithColumn("Info").AsCustom("text")
            .WithColumn("Name").AsString().NotNullable() |> ignore

        this.Create.ForeignKey("FKSchoolUser").FromTable("School") .ForeignColumn("UserId")
            .ToTable("User").PrimaryColumn("Id").OnDelete(System.Data.Rule.Cascade) |> ignore
        
        
        //tbale to map students in the User table to schools
        this.Create.Table("Student")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity().NotNullable().Unique()
            .WithColumn("SchoolId").AsInt32().ForeignKey()
            .WithColumn("UserId").AsInt32().ForeignKey() |> ignore
            
        this.Create.ForeignKey("FKStudentUser").FromTable("Student")
            .ForeignColumn("UserId").ToTable("User").PrimaryColumn("Id") |> ignore
        this.Create.ForeignKey("FKStudentSchool").FromTable("Student")
            .ForeignColumn("SchoolId").ToTable("School").PrimaryColumn("Id") |> ignore

        //Class descriptions linked to a school.
        this.Create.Table("ClassType")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity().NotNullable().Unique()
            .WithColumn("SchoolId").AsInt32().ForeignKey().NotNullable()
            .WithColumn("Name").AsString().NotNullable()
            .WithColumn("Description").AsString().NotNullable() |> ignore

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
            .WithColumn("UserId").AsInt32().ForeignKey() |> ignore

        this.Create.ForeignKey("FKClassScheduleStudentClassSchedule").FromTable("ClassScheduleStudent")
            .ForeignColumn("ClassScheduleId").ToTable("ClassSchedule").PrimaryColumn("Id") |> ignore
        this.Create.ForeignKey("FKClassScheduleStudentUser").FromTable("ClassScheduleStudent")
            .ForeignColumn("UserId").ToTable("User").PrimaryColumn("Id") |> ignore
        this.Create.UniqueConstraint("ConStudentClassSchedule").OnTable("ClassScheduleStudent")
            .Columns([|"ClassScheduleId"; "UserId"|]) |> ignore

        //students wanting to enrol at a school
        this.Create.Table("Pending")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity().NotNullable().Unique()
            .WithColumn("UserId").AsInt32().NotNullable().ForeignKey()
            .WithColumn("SchoolId").AsInt32().NotNullable().ForeignKey() |> ignore
            
        this.Create.ForeignKey("FKPendingUser").FromTable("Pending")
            .ForeignColumn("UserId").ToTable("User").PrimaryColumn("Id") |> ignore
        this.Create.ForeignKey("FKPendingSchool").FromTable("Pending")
            .ForeignColumn("SchoolId").ToTable("School").PrimaryColumn("Id") |> ignore

        this.Create.Table("TitanClaims")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity().NotNullable()
            .WithColumn("UserId").AsInt32().NotNullable() 
            .WithColumn("Type").AsString().NotNullable()
            .WithColumn("Value").AsString().NotNullable() |> ignore

        this.Create.ForeignKey("FKTitanClaims").FromTable("TitanClaims")
            .ForeignColumn("UserId").ToTable("User").PrimaryColumn("Id").OnDelete(System.Data.Rule.Cascade) |> ignore


        this.Insert.IntoTable("User").Row({Email = "sashang@tewtin.com"; FirstName = "Sashan";
                                           LastName = "Govender"; Phone = ""}) |> ignore
        this.Insert.IntoTable("TitanClaims").Row({UserId = 1; Type = "IsTitan"; Value = "true"})|> ignore
        
    override this.Down() =
        this.Delete.Table("ClassScheduleStudent") |> ignore
        this.Delete.Table("ClassSchedule") |> ignore
        this.Delete.Table("ClassType") |> ignore
        this.Delete.Table("Pending") |> ignore
        this.Delete.Table("Student") |> ignore
        this.Delete.Table("School") |> ignore
        this.Delete.Table("TitanClaims") |> ignore
        this.Delete.Table("User") |> ignore
