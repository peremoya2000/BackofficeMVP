using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altairis.Backoffice.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SearchIgnoringAccents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_hotels_city",
                table: "hotels");

            migrationBuilder.DropIndex(
                name: "ix_hotels_name",
                table: "hotels");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .Annotation("Npgsql:PostgresExtension:unaccent", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:pg_trgm", ",,");

            // Hand-written: unaccent() is only STABLE, and an index needs an IMMUTABLE function.
            migrationBuilder.Sql("""
                CREATE FUNCTION immutable_unaccent(text) RETURNS text
                LANGUAGE sql IMMUTABLE PARALLEL SAFE STRICT
                RETURN public.unaccent('public.unaccent'::regdictionary, $1);
                """);
            migrationBuilder.Sql("CREATE INDEX ix_hotels_unaccent_name ON hotels USING gin (immutable_unaccent(name) gin_trgm_ops);");
            migrationBuilder.Sql("CREATE INDEX ix_hotels_unaccent_city ON hotels USING gin (immutable_unaccent(city) gin_trgm_ops);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX ix_hotels_unaccent_city;");
            migrationBuilder.Sql("DROP INDEX ix_hotels_unaccent_name;");
            migrationBuilder.Sql("DROP FUNCTION immutable_unaccent(text);");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:unaccent", ",,");

            migrationBuilder.CreateIndex(
                name: "ix_hotels_city",
                table: "hotels",
                column: "city")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_hotels_name",
                table: "hotels",
                column: "name")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
        }
    }
}
