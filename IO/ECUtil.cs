using System;
using System.Runtime.InteropServices;

namespace LocalAdmin.V2.IO
{
    /// <summary>
    ///     Error codes for parsing.
    /// </summary>
    internal enum ErrorType
    {
        /// <summary>
        ///     Wrong port was given for the game.
        /// </summary>
        INVALID_PORT_GIVEN,
        /// <summary>
        ///     Unsupported game launch platform.
        /// </summary>
        UNSUPPORTED_PLATFORM,
        /// <summary>
        ///     Game launcher executable not found.
        /// </summary>
        ERROR_FILE_NOT_FOUND,
    }

    /// <summary>
    ///     Assistant determining the 
    ///     correct exit error codes for the application.
    /// </summary>
    internal static class ECUtil
    {
        private enum UnixErrorCode
        {
            INVALID_PORT_GIVEN = 22, // Errno 22: Invalid argument
            UNSUPPORTED_PLATFORM = 95, // Errno 95: Operation not supported
            ERROR_FILE_NOT_FOUND = 8, // Errno 8: Exec format error
        }

        private enum WindowsErrorCode
        {
            INVALID_PORT_GIVEN = 160, // ERROR_BAD_ARGUMENTS: One or more arguments are not correct
            UNSUPPORTED_PLATFORM = 10, // ERROR_BAD_ENVIRONMENT: The environment is incorrect
            ERROR_FILE_NOT_FOUND = 2, // ERROR_FILE_NOT_FOUND: The system cannot find the file specified.
        }

        /// <summary>
        ///     Platform-specific error code parsing.
        /// </summary>
        /// <returns>
        ///     Valid error code to exit the application,
        ///     otherwise -1.
        /// </returns>
        internal static int Parse(OSPlatform platform, ErrorType errorType)
        {
            return ParseByPratform(platform, errorType.ToString());
        }

        /// <summary>
        ///     Parsing error code according to platform by name.
        /// </summary>
        private static int ParseByPratform(OSPlatform platform, string name)
        {
            if (platform == OSPlatform.Windows)
                return EnumParseByName<WindowsErrorCode>(name);
            else if (platform == OSPlatform.Linux)
                return EnumParseByName<UnixErrorCode>(name);
            else
                return -1;
        }

        private static int EnumParseByName<T>(string name) where T : struct
        {
            if (Enum.TryParse<T>(name, true, out var result))
                return (int)Convert.ChangeType(result, TypeCode.Int32);

            return -1;
        }
    }
}
