namespace LocalAdmin.V2.IO
{
    public enum UnixErrorCode
    {
        INVALID_PORT_GIVEN = 22, // Errno 22: Invalid argument
        ERROR_FILE_NOT_FOUND = 8, // Errno 8: Exec format error
    }

    public enum WindowsErrorCode
    {
        INVALID_PORT_GIVEN = 160, // ERROR_BAD_ARGUMENTS: One or more arguments are not correct
        ERROR_FILE_NOT_FOUND = 2, // ERROR_FILE_NOT_FOUND: The system cannot find the file specified.
    }
}
