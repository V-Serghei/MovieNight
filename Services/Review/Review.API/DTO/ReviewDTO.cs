namespace Review.API.DTO;

public record ReviewDTO( string FilmId, string Film, string UserId, 
    string Text, string User, DateTime Date);