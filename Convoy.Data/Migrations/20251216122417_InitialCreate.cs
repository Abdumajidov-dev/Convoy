using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Convoy.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "daily_summaries",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    total_locations = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    first_location_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_location_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    total_distance_km = table.Column<double>(type: "double precision", nullable: false, defaultValue: 0.0),
                    average_speed = table.Column<double>(type: "double precision", nullable: true),
                    max_speed = table.Column<double>(type: "double precision", nullable: true),
                    min_latitude = table.Column<double>(type: "double precision", nullable: true),
                    max_latitude = table.Column<double>(type: "double precision", nullable: true),
                    min_longitude = table.Column<double>(type: "double precision", nullable: true),
                    max_longitude = table.Column<double>(type: "double precision", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_daily_summaries", x => x.id);
                    table.ForeignKey(
                        name: "FK_daily_summaries_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "locations",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    latitude = table.Column<double>(type: "double precision", nullable: false),
                    longitude = table.Column<double>(type: "double precision", nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    speed = table.Column<double>(type: "double precision", nullable: true),
                    accuracy = table.Column<double>(type: "double precision", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_locations", x => x.id);
                    table.ForeignKey(
                        name: "FK_locations_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hourly_summaries",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    daily_summary_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    hour = table.Column<int>(type: "integer", nullable: false),
                    location_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    distance_km = table.Column<double>(type: "double precision", nullable: false, defaultValue: 0.0),
                    average_speed = table.Column<double>(type: "double precision", nullable: true),
                    first_location_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_location_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    min_latitude = table.Column<double>(type: "double precision", nullable: true),
                    max_latitude = table.Column<double>(type: "double precision", nullable: true),
                    min_longitude = table.Column<double>(type: "double precision", nullable: true),
                    max_longitude = table.Column<double>(type: "double precision", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hourly_summaries", x => x.id);
                    table.ForeignKey(
                        name: "FK_hourly_summaries_daily_summaries_daily_summary_id",
                        column: x => x.daily_summary_id,
                        principalTable: "daily_summaries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_hourly_summaries_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_daily_summaries_date",
                table: "daily_summaries",
                column: "date");

            migrationBuilder.CreateIndex(
                name: "IX_daily_summaries_user_id",
                table: "daily_summaries",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_daily_summaries_user_id_date",
                table: "daily_summaries",
                columns: new[] { "user_id", "date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hourly_summaries_daily_summary_id",
                table: "hourly_summaries",
                column: "daily_summary_id");

            migrationBuilder.CreateIndex(
                name: "IX_hourly_summaries_daily_summary_id_hour",
                table: "hourly_summaries",
                columns: new[] { "daily_summary_id", "hour" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hourly_summaries_user_id",
                table: "hourly_summaries",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_locations_timestamp",
                table: "locations",
                column: "timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_locations_user_id",
                table: "locations",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_phone",
                table: "users",
                column: "phone",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hourly_summaries");

            migrationBuilder.DropTable(
                name: "locations");

            migrationBuilder.DropTable(
                name: "daily_summaries");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
