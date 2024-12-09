using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace Hotel.Models
{
    public class DashboardViewModel
{
    public List<int> HotelRatings { get; set; }
    public List<int> ServiceRatings { get; set; }
    
    public Dictionary<int, int> AverageHotelRatings { get; set; }
    public Dictionary<int, int> AverageServiceRatings { get; set; }
    public Dictionary<string, int> MonthlyBookings { get; set; }
    public Dictionary<string, double> RoomOccupancy { get; set; }
    public int TotalUsers { get; set; }
    public int TotalReservations { get; set; }
    public int TotalPayments { get; set; }
    public double GlobalRoomRating { get; set; } 
    public int TotalRooms { get; set; }
    public int TotalServices { get; set; }
    public int SelectedMonth { get; set; }
    public List<SelectListItem> Months { get; set; }

    public double GlobalHotelRating { get; set; } // Nueva propiedad
    public double GlobalServiceRating { get; set; } // Nueva propiedad

    public string BestRatedRoomNumber { get; set; } // Nueva propiedad
    public double BestRatedRoomScore { get; set; }
    
    public Dictionary<string, int> MostBookedRooms { get; set; }

    public Dictionary<string, int> MostRequestedServices { get; set; }

     public Dictionary<string, BestRatedRoomByTypeModel> BestRatedRoomByType { get; set; } // Nueva propiedad
}
}
