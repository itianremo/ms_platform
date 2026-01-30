namespace Chat.API.DTOs;

public class CreateChannelRequest
{
    public string Name { get; set; }
    public List<Guid> MemberIds { get; set; }
}

public class ChannelResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}
