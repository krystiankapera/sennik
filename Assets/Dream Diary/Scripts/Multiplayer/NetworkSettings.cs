namespace Multiplayer {
    public static class NetworkSettings {
        static string ip;
        static int port;

        public static string IP => ip;
        public static int Port => port;

        public static void HostSetup(int portNr) {
            ip = null;
            port = portNr;
        }

        public static void ClientSetup(string ipAddress, int portNr) {
            ip = ipAddress;
            port = portNr;
        }

        public static void SingleplayerSetup() {
            ip = null;
            port = -1;
        }
    }
}
