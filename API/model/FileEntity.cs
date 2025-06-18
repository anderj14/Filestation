
namespace API.model
{
    public class FileEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public byte[] Data { get; set; }
        public DateTime UploadDate { get; set; } = DateTime.UtcNow;
    }
}