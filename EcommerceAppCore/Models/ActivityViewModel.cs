namespace EcommerceAppCore.Models
{
    public class ActivityViewModel
    {
        
            public List<ActivityLog> Logs { get; set; }
            public int CurrentPage { get; set; }
            public int TotalPages { get; set; }
            public int PageSize { get; set; }
        

    }
}
