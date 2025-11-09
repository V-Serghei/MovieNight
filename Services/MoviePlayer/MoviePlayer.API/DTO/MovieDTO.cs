using MoviePlayer.Domain.Enums;

namespace Auth.API.DTO;

public record MovieDTO(string Title, string PosterImage, MovieCategory Category, string Quote, 
    string Description, DateTime ProductionYear, string ProductionYearS,
    string Country, string Director, DateTime Duration, string Certificate, 
    string ProductionCompany, string Budget, string GrossWorldwide, string Language,  
    List<string> Genre);