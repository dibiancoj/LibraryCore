using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryCore.IntegrationTests.Migrations
{
    public partial class InsertStatesSp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            const string sql = @"SET ANSI_NULLS ON
                                GO
                                SET QUOTED_IDENTIFIER ON
                                GO

                                CREATE PROCEDURE dbo.CreateState
	                                @TestId UniqueIdentifier,
	                                @Description varchar(25)
                                AS
                                BEGIN
                                   insert into states(TestId, Description) values(@TestId, @Description);
                                END
                                GO";

            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("drop procedure dbo.CreateState");
        }
    }
}