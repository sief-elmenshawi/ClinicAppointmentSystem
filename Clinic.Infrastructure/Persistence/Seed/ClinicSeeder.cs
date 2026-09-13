using Clinic.Domain.Entities;
using Clinic.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Clinic.Infrastructure.Persistence.Seed;

public static class ClinicSeeder
{
    private const string Password = "P@ssw0rd";

    private static readonly string[][] ClinicSpecs =
    {
        new[] { "عيادة الأسنان", "طب الأسنان" },
        new[] { "عيادة الباطنة", "الباطنة العامة" },
        new[] { "عيادة الصدر", "أمراض الصدر والجهاز التنفسي" },
        new[] { "عيادة القلب", "أمراض القلب والأوعية الدموية" },
        new[] { "عيادة العظام", "جراحة العظام" },
        new[] { "عيادة العيون", "طب وجراحة العيون" },
        new[] { "معمل التحاليل", "معمل التحاليل" },
    };

    /// <summary>
    /// 2 دكاترة لكل عيادة، كل دكتور 2-3 أيام في الأسبوع، 3 ساعات في اليوم الواحد.
    /// </summary>
    private static readonly (string Clinic, (string Name, decimal Fee, string Phone, (DayOfWeek, int, int)[] Hours)[] Doctors)[] ClinicDoctors =
    {
        ("عيادة الأسنان", new[]
        {
            ("د. أحمد سمير", 150m, "01000000010", new[] { (DayOfWeek.Saturday, 10, 13), (DayOfWeek.Monday, 10, 13), (DayOfWeek.Tuesday, 17, 20) }),
            ("د. منى حسن", 180m, "01000000011", new[] { (DayOfWeek.Sunday, 11, 14), (DayOfWeek.Wednesday, 18, 21) }),
        }),
        ("عيادة الباطنة", new[]
        {
            ("د. خالد عبد الرحمن", 120m, "01000000012", new[] { (DayOfWeek.Saturday, 13, 16), (DayOfWeek.Tuesday, 10, 13), (DayOfWeek.Thursday, 10, 13) }),
            ("د. سارة محمود", 130m, "01000000013", new[] { (DayOfWeek.Sunday, 15, 18), (DayOfWeek.Wednesday, 13, 16) }),
        }),
        ("عيادة الصدر", new[]
        {
            ("د. محمد إبراهيم", 140m, "01000000014", new[] { (DayOfWeek.Saturday, 9, 12), (DayOfWeek.Monday, 13, 16), (DayOfWeek.Thursday, 13, 16) }),
            ("د. هالة مصطفى", 150m, "01000000015", new[] { (DayOfWeek.Sunday, 10, 13), (DayOfWeek.Tuesday, 13, 16) }),
        }),
        ("عيادة القلب", new[]
        {
            ("د. عمر عبد الله", 250m, "01000000016", new[] { (DayOfWeek.Saturday, 11, 14), (DayOfWeek.Monday, 18, 21), (DayOfWeek.Wednesday, 13, 16) }),
            ("د. نادية كامل", 260m, "01000000017", new[] { (DayOfWeek.Sunday, 17, 20), (DayOfWeek.Thursday, 18, 21) }),
        }),
        ("عيادة العظام", new[]
        {
            ("د. طارق فياض", 200m, "01000000018", new[] { (DayOfWeek.Saturday, 12, 15), (DayOfWeek.Tuesday, 17, 20), (DayOfWeek.Thursday, 10, 13) }),
            ("د. شيماء عادل", 210m, "01000000019", new[] { (DayOfWeek.Sunday, 13, 16), (DayOfWeek.Wednesday, 10, 13) }),
        }),
        ("عيادة العيون", new[]
        {
            ("د. يوسف رشاد", 170m, "01000000020", new[] { (DayOfWeek.Saturday, 10, 13), (DayOfWeek.Monday, 10, 13), (DayOfWeek.Wednesday, 17, 20) }),
            ("د. مريم صبري", 180m, "01000000021", new[] { (DayOfWeek.Sunday, 11, 14), (DayOfWeek.Tuesday, 11, 14) }),
        }),
        ("معمل التحاليل", new[]
        {
            ("أ. أحمد عاطف", 50m, "01000000022", new[] { (DayOfWeek.Saturday, 9, 12), (DayOfWeek.Monday, 9, 12), (DayOfWeek.Thursday, 16, 19) }),
            ("أ. رنا سامي", 50m, "01000000023", new[] { (DayOfWeek.Sunday, 9, 12), (DayOfWeek.Wednesday, 16, 19) }),
        }),
    };

    public static async Task SeedClinicsAndDoctorsAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        if (await context.Departments.AnyAsync(d => d.Name == "عيادة الأسنان"))
            return;

        // 1) العيادات والتخصصات
        var departments = new Dictionary<string, Department>();

        foreach (var pair in ClinicSpecs)
        {
            var department = new Department
            {
                Name = pair[0],
                Specializations = { new Specialization { Name = pair[1] } }
            };

            context.Departments.Add(department);
            departments[pair[0]] = department;
        }

        await context.SaveChangesAsync();

        // 2) الدكاترة — كل واحد في SaveChange منفصل حتى لا يحصل تعارض في الـ Batch
        foreach (var clinicGroup in ClinicDoctors)
        {
            var department = departments[clinicGroup.Clinic];
            var specialization = department.Specializations.First();

            foreach (var doctorDef in clinicGroup.Doctors)
            {
                var email = ToEmail(doctorDef.Name);

                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    PhoneNumber = doctorDef.Phone
                };

                var createResult = await userManager.CreateAsync(user, Password);
                if (!createResult.Succeeded)
                    throw new InvalidOperationException(
                        $"Seed doctor failed: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");

                var roleResult = await userManager.AddToRoleAsync(user, "Doctor");
                if (!roleResult.Succeeded)
                    throw new InvalidOperationException(
                        $"Seed role failed: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");

                var doctor = new Doctor
                {
                    FullName = doctorDef.Name,
                    ApplicationUserId = user.Id,
                    SpecializationId = specialization.Id,
                    ConsultationFee = doctorDef.Fee,
                    PhoneNumber = doctorDef.Phone
                };

                foreach (var (day, startHour, endHour) in doctorDef.Hours)
                {
                    doctor.WorkingHours.Add(new DoctorWorkingHour
                    {
                        DayOfWeek = day,
                        StartTime = TimeSpan.FromHours(startHour),
                        EndTime = TimeSpan.FromHours(endHour),
                        SlotDurationMinutes = 30
                    });
                }

                context.Doctors.Add(doctor);
                await context.SaveChangesAsync();
            }
        }

        // 3) مريض تجريبي
        if (await userManager.FindByEmailAsync("patient@clinic.com") is null)
        {
            var patientUser = new ApplicationUser
            {
                UserName = "patient@clinic.com",
                Email = "patient@clinic.com",
                EmailConfirmed = true,
                PhoneNumber = "01111111111"
            };

            var createResult = await userManager.CreateAsync(patientUser, Password);
            if (!createResult.Succeeded)
                throw new InvalidOperationException(
                    $"Seed patient failed: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");

            var roleResult = await userManager.AddToRoleAsync(patientUser, "Patient");
            if (!roleResult.Succeeded)
                throw new InvalidOperationException(
                    $"Seed patient role failed: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");

            context.Patients.Add(new Patient
            {
                FullName = "مريض تجريبي",
                ApplicationUserId = patientUser.Id,
                DateOfBirth = new DateTime(1995, 1, 1)
            });

            await context.SaveChangesAsync();
        }
    }

    private static string ToEmail(string name)
    {
        var normalized = name
            .Replace("د. ", "")
            .Replace("أ. ", "")
            .Replace(" ", ".")
            .ToLowerInvariant();

        return $"{normalized}@clinic.com";
    }
}