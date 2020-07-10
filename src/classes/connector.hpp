#ifndef CONNECTOR_HEADER
#define CONNECTOR_HEADER
#define socket_t int
#include <arpa/inet.h>
#include <netdb.h>
#include <netinet/in.h>
#include <sys/socket.h>
#include <sys/types.h>
#include <unistd.h>

#include <cstring>

#include "logger.hpp"

class Connector {
   protected:
    socket_t sock1, sock2, sock3;
    int socket_places, ready, sock_max, max;
    int client_sock[FD_SETSIZE];
    fd_set overall_sock, read_sock;

   public:
    void init();

    int create_socket(int, int, int);

    void bind_socket(socket_t *, unsigned long, unsigned short);

    void listen_socket(socket_t *);

    void accept_socket(socket_t *, socket_t *);

    void TCP_recv(socket_t *, char *, size_t);

    void close_socket(socket_t *);
};
#endif //CONNECTOR_HEADER