namespace TmsApi.Infrastructure.Caching;

public static class CacheKeys
{
    public const string CoursesTag = "courses";

    public static string Course(string code)
        => $"course:{code}";


    public static string Courses(int page, int pageSize)
        => $"courses:{page}:{pageSize}";


    public const string CoursesAll = "courses:all";
}


