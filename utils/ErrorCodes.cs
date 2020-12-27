using PacTheMan.Models;

namespace pactheman_server {

    public static class ErrorCodes {
        static byte[] ErrorFactory(string msg) {
            return (new Error() { ErrorMessage = msg}).Encode();
        }
        public static byte[] NoSessionGiven = ErrorFactory("no_sess_id");
        public static byte[] UnexpectedMessage = ErrorFactory("unexpected_msg_type");
        public static byte[] ToManyPlayers = ErrorFactory("to_many_player");
        public static byte[] InvalidPosition = ErrorFactory("inv_pos");
        public static byte[] InvalidScore = ErrorFactory("inv_score");
    }

}