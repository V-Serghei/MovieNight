namespace MovieNight.Gateway.DTO.Review;

public record ReviewRequest( string FilmId, string Film, string UserId, 
    string Text, string User, DateTime Date);