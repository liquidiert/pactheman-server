using PacTheMan.Models;

namespace pactheman_server {

    public static class ErrorCodes {
        static byte[] ErrorFactory(string msg) {
            return (new Error() { ErrorMessage = msg}).Encode();
        }
        public static byte[] NoSessionGiven = ErrorFactory("No session id given");
        public static byte[] InvalidPosition = ErrorFactory("Invalid position; position exceeds valid range");
    }

}