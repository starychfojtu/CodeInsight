using CodeInsight.Library;
using CodeInsight.Library.Types;

namespace CodeInsight.Github
{
    public class ApplicationConfiguration
    {
        public ApplicationConfiguration(NonEmptyString applicationName, NonEmptyString clientId, NonEmptyString clientSecret)
        {
            ApplicationName = applicationName;
            ClientId = clientId;
            ClientSecret = clientSecret;
        }

        public NonEmptyString ApplicationName { get; }
        
        public NonEmptyString ClientId { get; }
        
        public NonEmptyString ClientSecret { get; }
    }
}