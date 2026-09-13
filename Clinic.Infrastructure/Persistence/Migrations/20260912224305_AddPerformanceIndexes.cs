using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DoctorWorkingHours_DoctorId",
                table: "DoctorWorkingHours");

            migrationBuilder.DropIndex(
                name: "IX_DoctorRatings_DoctorId",
                table: "DoctorRatings");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_PatientId",
                table: "Appointments");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_DoctorId_CreatedAt",
                table: "Notifications",
                columns: new[] { "DoctorId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_DoctorWorkingHours_DoctorId_DayOfWeek",
                table: "DoctorWorkingHours",
                columns: new[] { "DoctorId", "DayOfWeek" });

            migrationBuilder.CreateIndex(
                name: "IX_DoctorRatings_DoctorId_CreatedAt",
                table: "DoctorRatings",
                columns: new[] { "DoctorId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PatientId_AppointmentDateTime",
                table: "Appointments",
                columns: new[] { "PatientId", "AppointmentDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_Status_AppointmentDateTime",
                table: "Appointments",
                columns: new[] { "Status", "AppointmentDateTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notifications_DoctorId_CreatedAt",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_DoctorWorkingHours_DoctorId_DayOfWeek",
                table: "DoctorWorkingHours");

            migrationBuilder.DropIndex(
                name: "IX_DoctorRatings_DoctorId_CreatedAt",
                table: "DoctorRatings");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_PatientId_AppointmentDateTime",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_Status_AppointmentDateTime",
                table: "Appointments");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorWorkingHours_DoctorId",
                table: "DoctorWorkingHours",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorRatings_DoctorId",
                table: "DoctorRatings",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PatientId",
                table: "Appointments",
                column: "PatientId");
        }
    }
}
