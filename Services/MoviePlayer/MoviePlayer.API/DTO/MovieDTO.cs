using MoviePlayer.Domain.Enums;

namespace MoviePlayer.API.DTO;

public record MovieDTO(string Title, string PosterImage, MovieCategory Category, string Quote, 
    string Description, int ProductionYear,
    string Country, string Director, string Duration, string Certificate, 
    string ProductionCompany, string Budget, string GrossWorldwide, string Language,  
    List<string> Genre);