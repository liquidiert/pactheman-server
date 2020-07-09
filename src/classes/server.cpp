#include "server.h"

class Server : Connector {
    Logger _logger;

   public:
    Server() : Connector() { 
        _logger = Logger().getInstance();
        this->init();
    }

    void main_loop() {
        while(1) {
            read_sock = overall_sock;

            // wait for new client messages
            ready = select(sock_max + 1, &read_sock, NULL, NULL, NULL);

            // new client connected
            if(FD_ISSET(sock1, &read_sock)) {
                accept_socket(&sock1, &sock2);
                /* Freien Platz für (Socket-)Deskriptor
                 * in client_sock suchen und vergeben */
                for(socket_places = 0; socket_places < FD_SETSIZE;
                    socket_places++) {
                    if(client_sock[socket_places] < 0) {
                        client_sock[socket_places] = sock2;
                        break;
                    }
                }
                /* Mehr als FD_SETSIZE Clients sind nicht möglich */
                if(socket_places == FD_SETSIZE)
                    _logger.error_n_out("Server überlastet – zu viele Clients");
                /* Den neuen (Socket-)Deskriptor zur
                 * (Gesamt-)Menge hinzufügen */
                FD_SET(sock2, &overall_sock);
                /* select() benötigt die höchste
                 * (Socket-)Deskriptor-Nummer */
                if(sock2 > sock_max) sock_max = sock2;
                /* höchster Index für client_sock
                 * für die anschließende Schleife benötigt */
                if(socket_places > max) max = socket_places;
                /* ... weitere (Lese-)Deskriptoren bereit? */
                if(--ready <= 0) continue;  // Nein ...
            }

            /* Ab hier werden alle Verbindungen von Clients auf
             * die Ankunft von neuen Daten überprüft */
            for(socket_places = 0; socket_places <= max; socket_places++) {
                if((sock3 = client_sock[socket_places]) < 0) continue;
                /* (Socket-)Deskriptor gesetzt ... */
                if(FD_ISSET(sock3, &read_sock)) {
                    /* ... dann die Daten lesen */
                    TCP_recv(&sock3, buffer, BUF - 1);
                    printf("Nachricht empfangen: %s\n", buffer);
                    /* Wenn quit erhalten wurde ... */
                    if(strcmp(buffer, "quit\n") == 0) {
                        /* ... hat sich der Client beendet */
                        // Socket schließen
                        close_socket(&sock3);
                        // aus Menge löschen
                        FD_CLR(sock3, &overall_sock);
                        client_sock[socket_places] = -1;  // auf -1 setzen
                        printf("Ein Client hat sich beendet\n");
                    }
                    /* Noch lesbare Deskriptoren vorhanden ... ? */
                    if(--ready <= 0) break;  // Nein ...
                }
            }
        }
    }
};