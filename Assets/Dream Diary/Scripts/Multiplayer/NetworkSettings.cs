namespace Multiplayer {
    public static class NetworkSettings {
        static string ip;
        static int port;
        static NetworkGameMode mode;

        public static string IP => ip;
        public static int Port => port;
        public static NetworkGameMode Mode => mode;

        public static void HostSetup(int portNr) {
            ip = null;
            port = portNr;
            mode = NetworkGameMode.Host;
        }

        public static void ClientSetup(string ipAddress, int portNr) {
            ip = ipAddress;
            port = portNr;
            mode = NetworkGameMode.Client;
        }

        public static void SingleplayerSetup() {
            ip = null;
            port = -1;
            mode = NetworkGameMode.Singleplayer;
        }
    }

    public enum NetworkGameMode {
        Singleplayer = 0,
        Host = 1,
        Client = 2
    }
}
