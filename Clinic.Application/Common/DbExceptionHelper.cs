using Microsoft.EntityFrameworkCore;

namespace Clinic.Application.Common;

public static class DbExceptionHelper
{
    // مش أي DbUpdateException = تعارض حجز.
    // بنتأكد إن السبب فعلاً duplicate key (فهرس فريد اتخالف) قبل ما نعتبرها حجز متعارض،
    // ولو سبب تاني فالاستثناء هيطلّع برة وتتسجل كـ 500.
    public static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        for (Exception? inner = ex.InnerException; inner is not null; inner = inner.InnerException)
        {
            if (inner.Message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}