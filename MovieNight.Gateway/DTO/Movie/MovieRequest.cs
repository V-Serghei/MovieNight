namespace MovieNight.Gateway.DTO.Movie;

public record MovieRequest(string Title, string PosterImage, string Quote, 
    string Description, DateTime ProductionYear, string ProductionYearS,
    string Country, string Director, DateTime Duration, string Certificate, 
    string ProductionCompany, string Budget, string GrossWorldwide, string Language,  
    List<string> Genre);