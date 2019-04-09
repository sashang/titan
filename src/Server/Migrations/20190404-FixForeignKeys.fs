
namespace TitanMigrations

open FluentMigrator


[<Migration(20190404L)>]
type FixForeignKeys() =
    inherit Migration()
    override this.Up() =
        //we need to add this table for the tutor to school mapping. previously
        //we stuck the userid of the tutor in the school table which was dumb. Ironically
        //we created a Student table to map students to schools. Needed to do the same for tutors.
        //Now we have to fix it.
        this.Create.Table("Tutor")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity().NotNullable().Unique()
            .WithColumn("SchoolId").AsInt32().ForeignKey()
            .WithColumn("UserId").AsInt32().ForeignKey() |> ignore //students user id

        //need to copy the ids from the from the school userid column into the new tutor table
        //before deleting that column
        this.Execute.Sql("""insert into "Tutor" ("UserId", "SchoolId") select "School"."UserId", "School"."Id" from "School";""")

        this.Delete.ForeignKey("FKStudentUser").OnTable("Student") |> ignore
        this.Delete.ForeignKey("FKStudentSchool").OnTable("Student") |> ignore
        this.Delete.ForeignKey("FKSchoolUser").OnTable("School") |> ignore
        this.Delete.Index("IX_School_UserId").OnTable("School") |> ignore
        this.Delete.Column("UserId").FromTable("School") |> ignore

        this.Create.ForeignKey("FKTutorUser").FromTable("Tutor")
            .ForeignColumn("UserId").ToTable("User").PrimaryColumn("Id").OnDeleteOrUpdate(System.Data.Rule.Cascade) |> ignore
        this.Create.ForeignKey("FKTutorSchool").FromTable("Tutor")
            .ForeignColumn("SchoolId").ToTable("School").PrimaryColumn("Id").OnDeleteOrUpdate(System.Data.Rule.Cascade) |> ignore

        //fix up the foreign keys in the student  table
        this.Create.ForeignKey("FKStudentUser").FromTable("Student")
            .ForeignColumn("UserId").ToTable("User").PrimaryColumn("Id").OnDeleteOrUpdate(System.Data.Rule.Cascade) |> ignore
        this.Create.ForeignKey("FKStudentSchool").FromTable("Student")
            .ForeignColumn("SchoolId").ToTable("School").PrimaryColumn("Id").OnDeleteOrUpdate(System.Data.Rule.Cascade) |> ignore

    override this.Down() =
        ()