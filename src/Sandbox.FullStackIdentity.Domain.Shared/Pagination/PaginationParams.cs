namespace Sandbox.FullStackIdentity.Domain;

public class PaginationParams
{
    private const int MaxPageSize = 150;

    public int PageSize
    {
        get;
        set => field = value > MaxPageSize ? MaxPageSize : value;
    }

    public int PageNumber
    {
        get;
        set => field = value < 1 ? 1 : value;
    }

    public int Offset => (PageNumber - 1) * PageSize;


    public PaginationParams(int pageNumber = 1, int pageSize = 30)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
