namespace LogIPServiceTestAssignment.Database;

public class LogEntry
{
    public int Id { get; set; }
    public DateTime OccurrenceDate { get; set; }
    public long UserId { get; set; }
    public string Ip { get; set; }
}