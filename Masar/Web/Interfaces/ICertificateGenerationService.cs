namespace Web.Interfaces;

public interface ICertificateGenerationService
{
    /// <summary>
    /// Checks and generates certificates for all completed courses/tracks that don't have one
    /// </summary>
    Task<int> GenerateMissingCertificatesAsync(int studentId);
    
    /// <summary>
    /// Generates a certificate for a specific completed course
    /// </summary>
    Task<bool> GenerateCourseCertificateAsync(int studentId, int courseId);
    
    /// <summary>
    /// Generates a certificate for a specific completed track
    /// </summary>
    Task<bool> GenerateTrackCertificateAsync(int studentId, int trackId);
}