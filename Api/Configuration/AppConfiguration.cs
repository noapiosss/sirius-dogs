namespace Api.Configuration
{
    public class AppConfiguration
    {
        public string PostgreConnectionString { get; set; }
        public string BucketName { get; set; }
        public string CredentialFile { get; set; }
    }
}