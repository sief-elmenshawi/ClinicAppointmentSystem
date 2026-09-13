namespace Clinic.Application.Common;

public record DoctorDayStatusDto(
    int DoctorId,
    string DoctorName,
    string SpecializationName,
    decimal ConsultationFee,
    TimeSpan? StartTime,
    TimeSpan? EndTime,
    int SlotDurationMinutes,
    int BookedCount,
    int AvailableCount,
    // Working | Free | Absent | Off
    string Status);