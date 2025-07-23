
namespace API.model
{
    public class FileEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string FileName { get; set; }
        public required string ContentType { get; set; }
        public required byte[] Data { get; set; }
        public DateTime UploadDate { get; set; } = DateTime.UtcNow;
        public long Size => Data?.LongLength ?? 0;
        
        public Guid UserId { get; set; }
        public virtual AppUser User { get; set; }
    }
}