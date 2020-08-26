using System;
namespace In.ProjectEKA.HipService.OpenMrs.Exceptions
{
    public class OpenMrsResponseException : Exception
    {
        public OpenMrsResponseException(string? message) : base(message)
        {
        }
    }
}
