# 🏥 Clinic Appointment System

نظام إدارة حجوزات عيادة مبني بـ **.NET 10** باستخدام **Clean Architecture**, **CQRS**, و **MediatR**.

مشروع شامل يغطي: Authentication/Authorization, Background Jobs, Domain Events, Soft Delete, Audit Logging, Specification Pattern, Structured Logging, Rate Limiting, و Unit Testing.

---

## 📐 Architecture

المشروع مبني على **Clean Architecture** مقسّم لـ 4 Layers + مشروع اختبارات، مع تدفق اعتمادية (Dependency) من الخارج للداخل فقط:

```
Clinic.API              → Controllers, Middlewares, Program.cs (الطبقة الخارجية)
Clinic.Infrastructure    → EF Core, Identity, JWT, Hangfire, Serilog, External Services
Clinic.Application       → CQRS (Commands/Queries), Validators, Interfaces, Behaviors
Clinic.Domain            → Entities, Enums (لا يعتمد على أي طبقة أخرى)
Clinic.Application.Tests → Unit Tests (xUnit + Moq + FluentAssertions)
```

### أهم القرارات المعمارية

| القرار | السبب |
|---|---|
| **CQRS + MediatR** | فصل عمليات الكتابة (Commands) عن القراءة (Queries) |
| **Result Pattern** | تجنّب استخدام Exceptions للتحكم في منطق العمل (Business Logic) |
| **Dependency Inversion** | `Application` يعرّف Interfaces (`IApplicationDbContext`, `IIdentityService`, إلخ) والـ `Infrastructure` ينفذها |
| **Pipeline Behaviors** | Validation, Logging, و Audit Logging تلقائي لكل Request عبر MediatR |
| **Unique Index على مستوى DB** | ضمان حقيقي ضد الـ Double Booking حتى في حالة الـ Race Conditions |
| **Domain Events (MediatR Notifications)** | فصل الآثار الجانبية (زي إرسال الإيميل) عن الـ Command الأساسي |
| **Specification Pattern** | استعلامات قابلة لإعادة الاستخدام بدل تكرار `.Where()` |
| **Soft Delete صريح** | تعديل الحقول يدويًا في الـ Handler بدل الاعتماد على `.Remove()` (راجع قسم "دروس مستفادة") |

---

## 🗂️ Project Structure

```
Clinic.Domain/
├── Common/{BaseEntity, ISoftDelete}.cs
├── Enums/AppointmentStatus.cs
└── Entities/
    ├── Doctor.cs, Patient.cs, Specialization.cs
    ├── DoctorWorkingHour.cs, DoctorUnavailability.cs
    ├── Appointment.cs, DoctorRating.cs
    ├── AuditLog.cs, RefreshToken.cs

Clinic.Application/
├── Common/
│   ├── Result.cs, PagedResult.cs, JwtSettings.cs
│   ├── Behaviors/
│   │   ├── ValidationBehavior.cs
│   │   ├── LoggingBehavior.cs
│   │   └── AuditLoggingBehavior.cs
│   └── Specifications/
│       ├── BaseSpecification.cs
│       └── SpecificationEvaluator.cs
├── Interfaces/
│   ├── IApplicationDbContext.cs, IIdentityService.cs
│   ├── IJwtTokenGenerator.cs, ICurrentUserService.cs
│   ├── IEmailService.cs, IAppointmentCleanupService.cs
└── Features/
    ├── Auth/Commands/{Login, Register, RefreshToken}
    ├── Specializations/{Commands,Queries}
    ├── Doctors/
    │   ├── Commands/{CreateDoctor, AddWorkingHour, AddUnavailability, DeleteDoctor}
    │   ├── Queries/{GetAvailableSlots, GetDoctorsBySpecialization, GetDoctorRatings}
    │   └── Specifications/DoctorsBySpecializationSpec.cs
    ├── Patients/Commands/CreatePatient
    └── Appointments/
        ├── Commands/{CreateAppointment, Complete, Cancel, Reschedule, RateDoctor}
        ├── Queries/{GetDoctorAppointments, GetPatientAppointments}
        └── Events/{AppointmentCreatedEvent, AppointmentCreatedEventHandler}

Clinic.Infrastructure/
├── Identity/ApplicationUser.cs
├── Persistence/
│   ├── ApplicationDbContext.cs (+ Soft Delete query filters)
│   ├── ApplicationDbContextFactory.cs (Design-time factory للـ Migrations)
│   ├── Configurations/ (Fluent API لكل Entity)
│   ├── Migrations/
│   └── Seed/{RoleSeeder, AdminSeeder}.cs
└── Services/
    ├── IdentityService.cs, JwtTokenGenerator.cs
    ├── CurrentUserService.cs, EmailService.cs
    └── AppointmentCleanupService.cs (Hangfire Job)

Clinic.API/
├── Controllers/
│   ├── AuthController.cs (+ Rate Limiting)
│   ├── SpecializationsController.cs, DoctorsController.cs
│   ├── PatientsController.cs, AppointmentsController.cs
├── Middlewares/ExceptionHandlingMiddleware.cs (+ ProblemDetails)
└── Program.cs

Clinic.Application.Tests/
├── TestApplicationDbContext.cs (EF Core InMemory)
├── CreateAppointmentCommandHandlerTests.cs
└── CreateSpecializationCommandHandlerTests.cs
```

---

## ⚙️ متطلبات التشغيل

- .NET 10 SDK
- SQL Server (Local instance)
- Postman (أو أي أداة اختبار API)

---

## 🚀 خطوات التشغيل

### 1. اضبط الـ Connection String
📁 `Clinic.API/appsettings.json`
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=ClinicDb;Trusted_Connection=True;TrustServerCertificate=True"
}
```

### 2. ثبّت أداة EF Core (لو مش متثبتة)
```bash
dotnet tool install --global dotnet-ef
```

### 3. اعمل الـ Migration وحدّث الداتابيز
```bash
dotnet ef database update --project Clinic.Infrastructure --startup-project Clinic.API
```

### 4. شغّل المشروع
```bash
dotnet run --project Clinic.API
```

عند أول تشغيل، يتم تلقائيًا:
- إنشاء الـ Roles: `Admin`, `Doctor`, `Patient`
- إنشاء حساب Admin افتراضي: **Email:** `admin@clinic.com` / **Password:** `Admin@123`
- تسجيل Recurring Job في Hangfire (إلغاء الحجوزات المعلّقة تلقائيًا)

### 5. جرب الـ API
- Swagger: `https://localhost:7277/swagger`
- Hangfire Dashboard: `https://localhost:7277/hangfire`
- Health Check: `https://localhost:7277/health`
- أو استخدم `http://localhost:5136` مباشرة في Postman

### 6. شغّل الـ Unit Tests
```bash
dotnet test
```

---

## 🔑 Authentication & Authorization

النظام يستخدم **JWT Bearer Authentication** (Access Token + Refresh Token) مع 3 أدوار (Roles):

| Role | الصلاحيات |
|---|---|
| **Admin** | إضافة Doctors, Specializations, تحديد Working Hours/Unavailability, حذف الأطباء |
| **Doctor** | استعراض حجوزاته، إكمال (Complete) الحجز بعد الكشف |
| **Patient** | التسجيل الذاتي، حجز موعد، استعراض حجوزاته، إلغاء/تأجيل الحجز، تقييم الطبيب |

> **تسجيل الدخول:**
> - **Doctors:** يتم إضافتهم عن طريق الـ Admin فقط.
> - **Patients:** يقدروا يسجلوا نفسهم مباشرة عبر `/api/auth/register` (الـ Role بتتحدد Hardcoded على "Patient" في الكود، ومفيش أي طريقة للمستخدم يختار Role تانية - حماية ضد Privilege Escalation).

للوصول لأي Endpoint محمي:
```
Authorization: Bearer <accessToken>
```

لتجديد الـ Token بعد انتهاء صلاحيته (60 دقيقة):
```
POST /api/auth/refresh
{ "refreshToken": "<refreshToken>" }
```

⚠️ **Rate Limiting:** كل الـ Endpoints تحت `/api/auth/*` محدودة بـ 5 محاولات كل دقيقة (Fixed Window)، وبعد تجاوزها بيرجع `429 Too Many Requests`.

---

## 📋 API Endpoints

### 🔐 Auth
| Method | Endpoint | Auth |
|---|---|---|
| POST | `/api/auth/register` | ❌ (Patient فقط) |
| POST | `/api/auth/login` | ❌ |
| POST | `/api/auth/refresh` | ❌ |

### 🏷️ Specializations
| Method | Endpoint | Auth |
|---|---|---|
| POST | `/api/specializations` | Admin |
| GET | `/api/specializations?pageNumber=&pageSize=` | ❌ |

### 👨‍⚕️ Doctors
| Method | Endpoint | Auth |
|---|---|---|
| POST | `/api/doctors` | Admin |
| DELETE | `/api/doctors/{id}` | Admin (Soft Delete) |
| POST | `/api/doctors/{doctorId}/working-hours` | Admin |
| POST | `/api/doctors/{doctorId}/unavailability` | Admin |
| GET | `/api/doctors/{doctorId}/available-slots?date=yyyy-MM-dd` | ❌ |
| GET | `/api/doctors/by-specialization/{specializationId}` | ❌ |
| GET | `/api/doctors/{doctorId}/ratings` | ❌ |

### 🧑‍🦰 Patients
| Method | Endpoint | Auth |
|---|---|---|
| POST | `/api/patients` | Admin |

### 📅 Appointments
| Method | Endpoint | Auth |
|---|---|---|
| POST | `/api/appointments` | Authenticated |
| PUT | `/api/appointments/{id}/complete` | Doctor (صاحب الحجز فقط) |
| DELETE | `/api/appointments/{id}/cancel` | صاحب الحجز أو Admin |
| PUT | `/api/appointments/{id}/reschedule` | Patient (صاحب الحجز فقط) |
| POST | `/api/appointments/{id}/rate` | Patient (صاحب الحجز، بعد Completed فقط) |
| GET | `/api/appointments/doctor/{doctorId}?date=&pageNumber=&pageSize=` | Authenticated |
| GET | `/api/appointments/patient/{patientId}` | Authenticated |

### 🩺 System
| Method | Endpoint | الوصف |
|---|---|---|
| GET | `/health` | حالة الاتصال بقاعدة البيانات |
| GET | `/hangfire` | لوحة تحكم الـ Background Jobs |

---

## 🛡️ منع الـ Double Booking (أهم Business Rule في المشروع)

يتم التعامل مع مشكلة حجز نفس الميعاد من قِبل شخصين في نفس اللحظة (Race Condition) عبر دفاعين متتاليين:

1. **Application-level check**: تحقق مسبق في الـ Handler قبل أي عملية إدخال (تحسين للأداء، يرفض أغلب المحاولات بدري)
2. **Database-level protection**: `Unique Index` على `(DoctorId, AppointmentDateTime)` في جدول `Appointments` (مع استثناء الحجوزات الملغية) — هذا هو الضمان الحقيقي، حيث يرفض SQL Server أي محاولة إدخال متعارضة حتى لو نجحت في تخطي الفحص الأول بسبب توقيت متزامن

عند حدوث تعارض على مستوى الداتابيز، يتم اصطياد الـ `DbUpdateException` وإرجاع رسالة واضحة للمستخدم بدلاً من خطأ 500. هذا المنطق مغطّى بـ **Unit Tests** في `Clinic.Application.Tests`.

---

## 🧩 حالات الحجز (Appointment Status)

| القيمة | الحالة | الوصف |
|---|---|---|
| 1 | Pending | تم الحجز، في انتظار التأكيد (يُلغى تلقائيًا بعد ساعة عبر Hangfire) |
| 2 | Confirmed | تم تأكيد الحجز |
| 3 | Completed | تم الكشف فعليًا (يضيفه الطبيب، ويفتح إمكانية التقييم) |
| 4 | Cancelled | تم إلغاء الحجز |
| 5 | NoShow | المريض لم يحضر |

---

## 🧵 فيتشرز متقدمة

### Background Jobs (Hangfire)
`IAppointmentCleanupService` يشتغل كل ساعة (Recurring Job) ويلغي أي حجز فضل `Pending` لأكتر من ساعة من غير تأكيد. يُسجَّل عبر `IRecurringJobManager` (الـ instance-based API، وليس الـ static API القديم — راجع "دروس مستفادة").

### Domain Events
عند نجاح `CreateAppointmentCommand`, يتم نشر `AppointmentCreatedEvent` عبر `IPublisher`. الـ `AppointmentCreatedEventHandler` (منفصل تمامًا عن الـ Command) يستقبل الحدث ويرسل إيميل تأكيد (مسجَّل في الـ Console عبر `EmailService` الوهمية للتعلّم).

### Audit Logging
`AuditLoggingBehavior` (Pipeline Behavior) يسجّل كل Command (باسمه، بيانات الطلب، واسم المستخدم) في جدول `AuditLogs`. البيانات الحساسة (Password, Token, Secret) تُستبدل تلقائيًا بـ `***REDACTED***` قبل التسجيل.

### Soft Delete
`Doctor` و `Patient` عليهم `ISoftDelete` (`IsDeleted`, `DeletedAt`) + Global Query Filter. أي جدول مرتبط بـ `Doctor` (`Appointment`, `DoctorWorkingHour`, `DoctorRating`, `DoctorUnavailability`) له نفس الفلتر لضمان الاتساق.

### Specification Pattern
`DoctorsBySpecializationSpec` مثال حي على فصل منطق الاستعلام (Criteria, Includes, OrderBy) عن الـ Handler، عبر `BaseSpecification<T>` و `SpecificationEvaluator<T>`.

### Structured Logging (Serilog)
يسجّل في الـ Console (منسّق) وفي ملفات (`Logs/clinic-log-.txt`, rolling يوميًا, بالاحتفاظ بآخر 14 يوم). مستوى EF Core مضبوط على `Warning` لتقليل الضوضاء.

### Rate Limiting
Fixed Window Limiter (5 طلبات/دقيقة) مطبّق على `AuthController` بالكامل عبر `[EnableRateLimiting("AuthPolicy")]`.

### ProblemDetails (RFC 9110)
كل الأخطاء (Validation أو غير متوقعة) ترجع بصيغة موحدة (`type`, `title`, `status`, `detail`, `instance`) بدل شكل JSON مخصص.

---

## 🧪 Unit Tests

مشروع `Clinic.Application.Tests` (xUnit + Moq + FluentAssertions + EF Core InMemory) يغطي:
- إنشاء حجز ناجح
- رفض دكتور/مريض غير موجود
- رفض ميعاد خارج ساعات العمل
- رفض ميعاد غير متطابق مع مدة الـ Slot
- رفض حجز مكرر لنفس الميعاد (Double Booking - على مستوى الـ Application logic)
- التأكد من نشر `AppointmentCreatedEvent` بعد نجاح الحجز
- إنشاء/رفض تخصص مكرر

```bash
dotnet test
```

⚠️ **ملحوظة:** الـ InMemory Provider لا يحاكي الـ Unique Constraints بنفس دقة SQL Server، فاختبار الـ Race Condition الحقيقي (`DbUpdateException`) غير مغطّى بالكامل في الـ Unit Tests — التغطية الكاملة لهذا السيناريو تحتاج Integration Tests ضد قاعدة بيانات حقيقية.

---

## 🎓 دروس مستفادة (Known Issues & Fixes)

توثيق لمشاكل حقيقية واجهناها أثناء البناء، وهي قيّمة تعليميًا بقدر الحل نفسه:

| المشكلة | السبب | الحل |
|---|---|---|
| `.Remove()` فشل بـ `DbUpdateException` (FK constraint) رغم وجود Soft Delete | EF Core بيبني أمر `DELETE` فعلي فور استدعاء `.Remove()`، والـ Override في `SaveChangesAsync` مبيغيّرش الحالة بالوقت الكافي في كل الحالات | تعديل `IsDeleted`/`DeletedAt` يدويًا وصراحةً في الـ Handler بدل `.Remove()` |
| `RecurringJob.AddOrUpdate` (static) رمى `JobStorage.Current not initialized` | الـ Static API بتاع Hangfire مش بيتهيّأ تلقائيًا في بعض السيناريوهات الحديثة | استخدام `IRecurringJobManager` (instance-based) عبر DI |
| أخطاء الـ Validation اترجع للـ Client صح (400) لكن اتسجلت في Serilog كـ 500 | ترتيب الـ Middleware: `ExceptionHandlingMiddleware` كان مسجّل بعد `UseSerilogRequestLogging`، فـ Serilog يشوف الـ Exception قبل ما يتم التعامل معه | `UseSerilogRequestLogging()` ثم `UseMiddleware<ExceptionHandlingMiddleware>()` مباشرة بعده |
| `AuditLoggingBehavior` كان بيسجل الباسورد Plain Text في قاعدة البيانات | `JsonSerializer.Serialize(request)` مباشر من غير أي تصفية | تصفية أي property اسمها يحتوي "password"/"token"/"secret" واستبدالها بـ `***REDACTED***` قبل التسجيل |
| `AnyAsync`/`ToListAsync` تعارضت مع دوال جديدة في `System.Linq.AsyncEnumerable` (.NET 10) | .NET 10 أضاف BCL جديد فيه أسماء methods متطابقة مع EF Core | `using static Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions;` عند الحاجة |

---

## 🔧 التقنيات المستخدمة

- **.NET 10** / ASP.NET Core Web API
- **Entity Framework Core** (SQL Server Provider)
- **ASP.NET Core Identity** + **JWT Bearer Authentication** + Refresh Tokens
- **MediatR** (CQRS + Pipeline Behaviors + Domain Events/Notifications)
- **FluentValidation**
- **Hangfire** (Background Jobs + SQL Server Storage)
- **Serilog** (Console + Rolling File Sinks)
- **xUnit + Moq + FluentAssertions** (Unit Testing)
- **Built-in .NET Rate Limiting**
- **ASP.NET Core Health Checks**

---

## 📌 ملاحظات وتحسينات مستقبلية

- استخراج منطق التحقق من "هل الوقت ضمن ساعات العمل؟" لخدمة مشتركة (Domain Service) بدلاً من تكراره في `CreateAppointment` و `RescheduleAppointment`
- Integration Tests (`WebApplicationFactory`) لاختبار الـ API end-to-end ضد قاعدة بيانات حقيقية، خصوصًا لتغطية الـ Double Booking عبر الـ Unique Index الفعلي
- Docker + docker-compose لتشغيل المشروع وقاعدة البيانات بأمر واحد
- Outbox Pattern لضمان إرسال الـ Domain Events حتى في حالة فشل السيرفر بعد الـ SaveChanges مباشرة
- API Versioning — تم تقييمها والاتفاق على تأجيلها لتفادي مخاطرة غير ضرورية على مشروع تعليمي بحجم كبير من الاختبارات القائمة بالفعل
- نقل بيانات الـ Admin الافتراضي من الكود إلى `appsettings.json`
